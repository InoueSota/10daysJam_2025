using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VerticalScrollGradientRampBlend_DitherEase 用のバインダー＋プレイヤー。
/// 1) _RampA/_RampB/_RampTexelSize を必ず設定（未設定時は自動生成）
/// 2) グラデーション列をクロスフェード再生（A→B→C→…）
/// 3) 縦スクロール（上/下, 速度, スケール）
/// Renderer / UI(Graphic) どちらでも可
/// </summary>
[DisallowMultipleComponent]
public class GradientRampAutoBinder : MonoBehaviour
{
    [Header("Targets (どちらか1つでOK)")]
    public Renderer targetRenderer;
    public Graphic targetGraphic;

    [Header("Ramps (単体指定。未設定なら自動生成)")]
    public Texture2D rampA;
    public Texture2D rampB;

    [Tooltip("未設定のランプを自動生成（横1Dグラデーション）")]
    public bool autoGenerateIfMissing = true;

    [Tooltip("自動生成するランプの幅（2 のべき推奨）")]
    [Min(16)] public int generatedWidth = 256;

    [Header("Optional: 自動生成ランプの色（単体）")]
    public Gradient gradientA = DefaultGradientA();
    public Gradient gradientB = DefaultGradientB();

    // ====== 追加機能：複数グラデーションの再生 ======
    [Header("Sequence: 複数グラデーションを順再生（A→B→C→…）")]
    [Tooltip("テクスチャ列（設定があればこちらを優先）")]
    public List<Texture2D> rampSequence = new List<Texture2D>();

    [Tooltip("Gradient列（テクスチャ未設定時はこちらから自動生成）")]
    public List<Gradient> gradientSequence = new List<Gradient>();

    [Tooltip("各ステップのクロスフェード秒数")]
    [Min(0f)] public float crossfadeSeconds = 1.0f;

    [Tooltip("各ステップの表示キープ時間（クロスフェード間の待ち時間）")]
    [Min(0f)] public float holdSeconds = 0.0f;

    [Tooltip("再生開始時に自動でシーケンス再生")]
    public bool playOnStart = true;

    [Tooltip("最後まで再生したら先頭にループ")]
    public bool loopSequence = true;

    // ====== 追加機能：スクロール ======
    public enum ScrollDirection { Up, Down }
    [Header("Scroll: 縦スクロール（Offset更新）")]
    public ScrollDirection direction = ScrollDirection.Up;

    [Tooltip("1秒あたりのスクロール量（+で上方向、-で下方向相当）")]
    public float scrollSpeed = 0.2f;

    [Tooltip("TilingY（縦の繰り返し倍率）")]
    public float tilingY = 1.0f;

    [Tooltip("TimeScale 影響を無視")]
    public bool unscaledTime = false;

    // このシェーダ名は変更しない
    const string kShaderPath = "Hisa/URP/VerticalScrollGradientRampBlend_DitherEase";

    // 内部
    Material _mat;
    float _offset;
    int _curIndex;
    int _nextIndex;
    float _phase;       // 0..(hold->crossfade) の進行
    bool _isPlaying;

    void Awake()
    {
        EnsureBound(); // マテリアル／ランプの初期化
    }

    void Start()
    {
        // シーケンスが有効なら初期セット
        BuildSequenceIfNeeded();
        if (SequenceCount > 0)
        {
            PrepareStep(0, (0 + 1) % SequenceCount);
            if (playOnStart) _isPlaying = true;
        }
        // 初期の TilingY 反映
        if (_mat) _mat.SetFloat("_TilingY", Mathf.Max(1e-4f, tilingY));
    }

    void Reset()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (!targetGraphic) targetGraphic = GetComponent<Graphic>();
    }

    void Update()
    {
        if (_mat == null) return;

        // ---- スクロール ----
        float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float dir = (direction == ScrollDirection.Up) ? +1f : -1f;
        _offset += scrollSpeed * dir * dt;
        _mat.SetFloat("_Offset", _offset);
        // TilingY は Inspector 値を随時反映（必要なら）
        _mat.SetFloat("_TilingY", Mathf.Max(1e-4f, tilingY));

        // ---- シーケンス再生 ----
        if (_isPlaying && SequenceCount > 1)
        {
            float span = holdSeconds + crossfadeSeconds;
            if (span <= 0f) span = Mathf.Epsilon;

            _phase += dt;
            if (_phase < holdSeconds)
            {
                // Hold 中は Blend=0 のまま
                _mat.SetFloat("_Blend", 0f);
            }
            else
            {
                float t = Mathf.Clamp01((_phase - holdSeconds) / Mathf.Max(1e-6f, crossfadeSeconds));
                _mat.SetFloat("_Blend", t);

                // クロスフェード完了
                if (_phase >= span)
                {
                    // 次のステップへ
                    _curIndex = _nextIndex;
                    _nextIndex = (_curIndex + 1) % SequenceCount;
                    _phase = 0f;

                    // A/B 入れ替え・テクセルサイズ更新
                    ApplyRampsForStep(_curIndex, _nextIndex);
                    _mat.SetFloat("_Blend", 0f);

                    // ループしない & 最後に到達したら停止
                    if (!loopSequence && _curIndex == SequenceCount - 1)
                        _isPlaying = false;
                }
            }
        }
    }

    // ================== 初期バインド ==================
    void EnsureBound()
    {
        var shader = Shader.Find(kShaderPath);
        if (shader == null)
        {
            Debug.LogError($"[GradientRampAutoBinder] Shader.Find('{kShaderPath}') 失敗。", this);
            enabled = false;
            return;
        }

        // マテリアル取得
        if (targetRenderer)
        {
            _mat = targetRenderer.sharedMaterial != null ? targetRenderer.sharedMaterial
                                                         : targetRenderer.material;
            if (_mat == null) { _mat = new Material(shader); targetRenderer.sharedMaterial = _mat; }
        }
        else if (targetGraphic)
        {
            _mat = targetGraphic.material;
            if (_mat == null || _mat.shader == null) { _mat = new Material(shader); targetGraphic.material = _mat; }
        }
        else
        {
            Debug.LogError("[GradientRampAutoBinder] 対象が未指定です。Renderer か Graphic を割り当ててください。", this);
            enabled = false;
            return;
        }

        if (_mat.shader != shader) _mat.shader = shader;
        if (!_mat.shader || !_mat.shader.isSupported)
        {
            Debug.LogError($"[GradientRampAutoBinder] シェーダがサポート外です: {_mat.shader?.name}", this);
            enabled = false;
            return;
        }

        // 単体のランプを用意（未設定なら生成）
        if (rampA == null && autoGenerateIfMissing) rampA = Generate1DRamp(generatedWidth, gradientA);
        if (rampB == null && autoGenerateIfMissing) rampB = Generate1DRamp(generatedWidth, gradientB);

        if (rampA == null || rampB == null)
        {
            Debug.LogWarning("[GradientRampAutoBinder] _RampA/_RampB が未設定です。最低限 whiteTexture を適用します。", this);
            if (rampA == null) rampA = Texture2D.whiteTexture;
            if (rampB == null) rampB = Texture2D.whiteTexture;
        }

        _mat.SetTexture("_RampA", rampA);
        _mat.SetTexture("_RampB", rampB);
        SetTexelSizeFrom(rampA ? rampA : rampB);
        TrySetSamplerState(rampA);
        TrySetSamplerState(rampB);

        Debug.Log($"[GradientRampAutoBinder] OK : shader={_mat.shader.name}, _RampA={rampA?.name}, _RampB={rampB?.name}", this);
    }

    // ================== シーケンス周り ==================
    int SequenceCount => (rampSequence != null && rampSequence.Count > 0)
        ? rampSequence.Count
        : (gradientSequence != null ? gradientSequence.Count : 0);

    void BuildSequenceIfNeeded()
    {
        // テクスチャ列が空で、Gradient列がある場合は生成
        if ((rampSequence == null || rampSequence.Count == 0) && gradientSequence != null && gradientSequence.Count > 0)
        {
            rampSequence = new List<Texture2D>(gradientSequence.Count);
            for (int i = 0; i < gradientSequence.Count; i++)
                rampSequence.Add(Generate1DRamp(Mathf.Max(16, generatedWidth), gradientSequence[i]));
        }
    }

    void PrepareStep(int cur, int next)
    {
        _curIndex = cur;
        _nextIndex = next;
        _phase = 0f;
        ApplyRampsForStep(_curIndex, _nextIndex);
        _mat.SetFloat("_Blend", 0f);
    }

    void ApplyRampsForStep(int cur, int next)
    {
        var a = GetRamp(cur);
        var b = GetRamp(next);
        if (a == null) a = Texture2D.whiteTexture;
        if (b == null) b = Texture2D.whiteTexture;

        _mat.SetTexture("_RampA", a);
        _mat.SetTexture("_RampB", b);
        SetTexelSizeFrom(a);
        TrySetSamplerState(a);
        TrySetSamplerState(b);
    }

    Texture2D GetRamp(int index)
    {
        if (rampSequence != null && index >= 0 && index < rampSequence.Count)
            return rampSequence[index];
        return null;
    }

    void SetTexelSizeFrom(Texture2D tex)
    {
        int w = Mathf.Max(1, tex ? tex.width : generatedWidth);
        _mat.SetFloat("_RampTexelSize", 1.0f / w);
    }

    // ================== ユーティリティ ==================
    static void TrySetSamplerState(Texture2D tex)
    {
        if (!tex || tex == Texture2D.whiteTexture) return;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
    }

    public static Texture2D Generate1DRamp(int width, Gradient grad)
    {
        width = Mathf.Max(16, width);
        var tex = new Texture2D(width, 1, TextureFormat.RGBA32, false, /*linear*/ true);
        tex.name = $"Ramp_{grad.colorKeys.Length}_{width}x1";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var cols = new Color32[width];
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            cols[x] = grad.Evaluate(t);
        }
        tex.SetPixels32(cols);
        tex.Apply(false, false);
        return tex;
    }

    static Gradient DefaultGradientA()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }
    static Gradient DefaultGradientB()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0f, 0f), 0f), new GradientColorKey(new Color(0f, 0f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    // ===== 公開API =====
    /// <summary>シーケンス再生を開始/停止</summary>
    public void SetPlaying(bool playing) => _isPlaying = playing;

    /// <summary>先頭から再生し直す</summary>
    public void Restart()
    {
        if (SequenceCount == 0) return;
        PrepareStep(0, (0 + 1) % SequenceCount);
        _isPlaying = true;
    }

    /// <summary>インデックスへジャンプ</summary>
    public void JumpTo(int index, bool keepPlaying = true)
    {
        if (SequenceCount == 0) return;
        index = Mathf.Clamp(index, 0, SequenceCount - 1);
        PrepareStep(index, (index + 1) % SequenceCount);
        _isPlaying = keepPlaying;
    }
}
