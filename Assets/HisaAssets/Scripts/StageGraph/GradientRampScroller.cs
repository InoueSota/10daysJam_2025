using UnityEngine;
using System.Collections.Generic;

public class GradientRampScroller : MonoBehaviour
{
    [System.Serializable]
    public class GradientLoop
    {
        public Gradient gradient;
        public float scrollSpeed = 0.1f;  // 下→上（+で上昇）
        public float tilingY = 1f;        // 縦に何回繰り返すか
    }

    [Header("Target (どちらか)")]
    [SerializeField] Renderer targetRenderer;
    [SerializeField] UnityEngine.UI.Graphic targetUI;

    [Header("Loops (複数設定可)")]
    public List<GradientLoop> loops = new();

    [Header("切替フェード秒数")]
    [Min(0f)] public float transitionTime = 1.0f;

    [Header("その他")]
    public int rampWidth = 256;
    public bool scrollUp = true;

    // shader prop IDs
    static readonly int ID_RampA = Shader.PropertyToID("_RampA");
    static readonly int ID_RampB = Shader.PropertyToID("_RampB");
    static readonly int ID_Blend = Shader.PropertyToID("_Blend");
    static readonly int ID_Tiling = Shader.PropertyToID("_TilingY");
    static readonly int ID_Offset = Shader.PropertyToID("_Offset");

    Material mat;
    Texture2D rampA, rampB; // A=現在のベース, B=ターゲット
    float blendT;           // 0..1
    float offset;
    float curSpeed, curTiling;   // 現在（見た目ベース）
    float fromSpeed, toSpeed;    // 補間用
    float fromTiling, toTiling;  // 補間用

    public int current = 0;
    public int next = -1;
    public float transTimer;
    public bool transitioning;

    [SerializeField] int index;
    int preIndex;

    void Awake()
    {
        if (targetRenderer != null) mat = targetRenderer.material;
        else if (targetUI != null) { mat = Instantiate(targetUI.material); targetUI.material = mat; }
        else { Debug.LogError("[GradientRampScroller] Target 未設定"); return; }

        if (loops.Count == 0) { Debug.LogWarning("[GradientRampScroller] Loops が空です"); return; }

        // Ramp テクスチャ準備
        rampA = CreateRampTexture(rampWidth);
        rampB = CreateRampTexture(rampWidth);
        mat.SetTexture(ID_RampA, rampA);
        mat.SetTexture(ID_RampB, rampB);

        // 端パディング用（使用しているシェーダが受け取る場合）
        mat.SetFloat(Shader.PropertyToID("_RampTexelSize"), 1.0f / rampWidth);

        // 初期ループ適用
        BakeGradientTo(rampA, loops[current].gradient);
        ApplyLoopImmediate(loops[current]);

        var sh = Shader.Find("Hisa/URP/VerticalScrollGradientRampBlend_DitherEase");
        if (sh == null)
        {
            Debug.LogError("Shader not found in Player! Add it to 'Always Included Shaders'.");
            enabled = false;
            return;
        }
        if (targetRenderer != null)
        {
            var m = targetRenderer.sharedMaterial ?? targetRenderer.material;
            if (m == null || m.shader != sh)
            {
                m = new Material(sh);
                targetRenderer.sharedMaterial = m;
            }
            if (!m.HasTexture("_RampA") || m.GetTexture("_RampA") == null) m.SetTexture("_RampA", rampA);
            if (!m.HasTexture("_RampB") || m.GetTexture("_RampB") == null) m.SetTexture("_RampB", rampB);
            mat = m;
        }
    }

    void Update()
    {
        if (mat == null || loops.Count == 0) return;

        float dir = scrollUp ? 1f : -1f;

        if (transitioning)
        {
            transTimer += (transitionTime <= 0f) ? 1f : Time.deltaTime / transitionTime;
            blendT = Mathf.Clamp01(transTimer);

            // スピード/タイル補間
            curSpeed = Mathf.Lerp(fromSpeed, toSpeed, blendT);
            curTiling = Mathf.Lerp(fromTiling, toTiling, blendT);

            offset += curSpeed * Time.deltaTime * dir;

            mat.SetFloat(ID_Tiling, Mathf.Max(1e-4f, curTiling));
            mat.SetFloat(ID_Offset, offset);
            mat.SetFloat(ID_Blend, blendT);

            if (blendT >= 1f)
            {
                // 完了：B→Aへスワップして常にAを現行に
                var tmp = rampA; rampA = rampB; rampB = tmp;
                mat.SetTexture(ID_RampA, rampA);
                mat.SetTexture(ID_RampB, rampB);
                mat.SetFloat(ID_Blend, 0f);

                current = next;
                next = -1;
                transitioning = false;

                // 以後のベース値を確定
                fromSpeed = toSpeed;
                fromTiling = toTiling;
                curSpeed = fromSpeed;
                curTiling = fromTiling;
            }
        }
        else
        {
            var loop = loops[current];
            curSpeed = loop.scrollSpeed;
            curTiling = loop.tilingY;

            offset += curSpeed * Time.deltaTime * dir;

            mat.SetFloat(ID_Tiling, Mathf.Max(1e-4f, curTiling));
            mat.SetFloat(ID_Offset, offset);
            mat.SetFloat(ID_Blend, 0f);
        }

        if (preIndex != index)
        {
            preIndex = index;
            SwitchLoop(index);
        }
    }

    /// <summary>指定ループにフェード切替（再生中に呼んでOK）</summary>
    public void SwitchLoop(int loopIndex)
    {
        if (loopIndex < 0 || loopIndex >= loops.Count) return;
        if (!transitioning && loopIndex == current) return;

        // 目標ランプをBへベイク
        BakeGradientTo(rampB, loops[loopIndex].gradient);

        // ★ 切替中にさらに切替 => その瞬間の見た目（AとBの合成）をAへ焼き込み、そこから改めてブレンド開始
        if (transitioning)
        {
            FreezeCurrentVisualIntoA();       // A = Lerp(A,B,blendT)
            fromSpeed = curSpeed;            // いま見えている速度/タイルを“出発点”に
            fromTiling = curTiling;
            blendT = 0f;
            transTimer = 0f;
            mat.SetFloat(ID_Blend, 0f);
        }
        else
        {
            // 静止中に切替開始
            fromSpeed = curSpeed;
            fromTiling = curTiling;
            blendT = 0f;
            transTimer = 0f;
            mat.SetFloat(ID_Blend, 0f);
        }

        toSpeed = loops[loopIndex].scrollSpeed;
        toTiling = loops[loopIndex].tilingY;

        next = loopIndex;
        transitioning = true;
        // ※ current はフェード完了時に更新（途中では更新しない）
        // Debug.Log($"Index {loopIndex} に切り替え開始");
    }

    /// <summary>
    /// いま画面に見えている色（AとBのブレンド）をCPUで合成し、Aに焼き込む。
    /// こうすることで“途中からでも”自然に次へフェードできる。
    /// </summary>
    void FreezeCurrentVisualIntoA()
    {
        // A, B の画素を読み、Lerp(A,B,blendT) を A に書く
        var colsA = rampA.GetPixels();
        var colsB = rampB.GetPixels();

        int n = Mathf.Min(colsA.Length, colsB.Length);
        for (int i = 0; i < n; i++)
        {
            colsA[i] = Color.Lerp(colsA[i], colsB[i], blendT);
        }
        rampA.SetPixels(colsA);
        rampA.Apply(false, false);

        // マテリアル側の A を更新（ID_RampA は常に A）
        mat.SetTexture(ID_RampA, rampA);
    }

    /// <summary>現在ループを即時適用（フェードなし）</summary>
    void ApplyLoopImmediate(GradientLoop loop)
    {
        curSpeed = loop.scrollSpeed;
        curTiling = loop.tilingY;
        offset = 0f; // 任意。引き継ぎたい場合は消す

        BakeGradientTo(rampA, loop.gradient);
        mat.SetTexture(ID_RampA, rampA);
        mat.SetFloat(ID_Tiling, Mathf.Max(1e-4f, curTiling));
        mat.SetFloat(ID_Offset, offset);
        mat.SetFloat(ID_Blend, 0f);
    }

    Texture2D CreateRampTexture(int width)
    {
        bool isLinear = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        var tex = new Texture2D(Mathf.Max(2, width), 1,
            TextureFormat.RGBAHalf,  // 量子化を抑える
            false,
            isLinear);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    void BakeGradientTo(Texture2D tex, Gradient g)
    {
        int w = tex.width;
        bool isLinear = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        var cols = new Color[w];

        for (int x = 0; x < w - 1; x++)
        {
            float t = ((x + 0.5f) / w);
            Color c = g.Evaluate(t);
            cols[x] = isLinear ? c.linear : c;
        }
        cols[w - 1] = cols[0]; // 端の一致

        tex.SetPixels(cols);
        tex.Apply(false, false);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying || mat == null || loops.Count == 0 || rampA == null) return;
        BakeGradientTo(rampA, loops[Mathf.Clamp(current, 0, loops.Count - 1)].gradient);
        mat.SetTexture(ID_RampA, rampA);
    }
#endif

    [ContextMenu("色変え")]
    public void ChangeColor() => SwitchLoop(index);

    public void SetIndex(int set) => index = set;
}
