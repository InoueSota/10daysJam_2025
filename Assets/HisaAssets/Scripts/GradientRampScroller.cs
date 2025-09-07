using UnityEngine;
using System.Collections.Generic;

public class GradientRampScroller : MonoBehaviour
{
    [System.Serializable]
    public class GradientLoop
    {
        public Gradient gradient;     // ← Particle System と同じUIで編集
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
    public int rampWidth = 256;     // ランプ解像度（横）
    public bool scrollUp = true;    // false で上→下に反転（必要なら）

    // shader prop IDs
    static readonly int ID_RampA = Shader.PropertyToID("_RampA");
    static readonly int ID_RampB = Shader.PropertyToID("_RampB");
    static readonly int ID_Blend = Shader.PropertyToID("_Blend");
    static readonly int ID_Tiling = Shader.PropertyToID("_TilingY");
    static readonly int ID_Offset = Shader.PropertyToID("_Offset");

    Material mat;
    Texture2D rampA, rampB; // 現行/次用
    float blendT;           // 0..1
    float offset;
    float curSpeed, curTiling;
    float fromSpeed, toSpeed;
    float fromTiling, toTiling;

    int current = 0;
    int next = -1;
    float transTimer;
    bool transitioning;

    [SerializeField] int index;
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

        // ★ テクセルサイズを渡す（端パディング用）
        mat.SetFloat(Shader.PropertyToID("_RampTexelSize"), 1.0f / rampWidth);

        // 初期ループ適用
        BakeGradientTo(rampA, loops[current].gradient);
        ApplyLoopImmediate(loops[current]);
    }

    void Update()
    {
        if (mat == null || loops.Count == 0) return;

        float dir = scrollUp ? 1f : -1f;

        if (transitioning)
        {
            transTimer += (transitionTime <= 0f) ? 1f : Time.deltaTime / transitionTime;
            blendT = Mathf.Clamp01(transTimer);

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
    }

    /// <summary>指定ループにフェード切替（再生中に呼んでOK）</summary>
    public void SwitchLoop(int loopIndex)
    {
        if (loopIndex < 0 || loopIndex >= loops.Count) return;
        if (loopIndex == current) return;

        // 目的ランプをBへベイクしてフェード準備
        BakeGradientTo(rampB, loops[loopIndex].gradient);

        fromSpeed = curSpeed;
        toSpeed = loops[loopIndex].scrollSpeed;
        fromTiling = curTiling;
        toTiling = loops[loopIndex].tilingY;

        next = loopIndex;
        transTimer = 0f;
        blendT = 0f;
        transitioning = true;
    }

    /// <summary>現在ループを即時適用（フェードなし）</summary>
    void ApplyLoopImmediate(GradientLoop loop)
    {
        curSpeed = loop.scrollSpeed;
        curTiling = loop.tilingY;
        offset = 0f; // 任意。引き継ぎたいなら消す

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
            false,                    // mip なし
            isLinear);                // Linear プロジェクトは true
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;     // 端は自動で繰り返す
        return tex;
    }

    void BakeGradientTo(Texture2D tex, Gradient g)
    {
        int w = tex.width;
        bool isLinear = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        var cols = new Color[w];

        for (int x = 0; x < w - 1; x++)              // 最後の1画素は後で埋める
        {
            float t = ((x + 0.5f) / w);              // [0,1) のテクセル中心
            Color c = g.Evaluate(t);
            cols[x] = isLinear ? c.linear : c;
        }
        cols[w - 1] = cols[0];                       // ★ 端の色を一致させる

        tex.SetPixels(cols);
        tex.Apply(false, false);
    }



#if UNITY_EDITOR
    // インスペクタで Gradient を触ったとき再ベイク
    void OnValidate()
    {
        if (!Application.isPlaying || mat == null || loops.Count == 0 || rampA == null) return;
        BakeGradientTo(rampA, loops[Mathf.Clamp(current, 0, loops.Count - 1)].gradient);
        mat.SetTexture(ID_RampA, rampA);
    }
#endif

    [ContextMenu("色変え")]
    public void ChangeColor()
    {
        SwitchLoop(index);
    }
}
