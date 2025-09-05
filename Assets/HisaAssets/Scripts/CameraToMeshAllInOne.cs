using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering; // GraphicsFormat
#endif

/// <summary>
/// 指定カメラの描画を RenderTexture に出力し、任意の Renderer に貼り付けるオールインワン。
/// - 画面サイズ追従/固定解像度の切替
/// - MaterialPropertyBlockで安全に適用
/// - targetTexture解除→破棄の順でクリーンアップ（エラー回避）
/// - アスペクト補正: Stretch / Contain / Cover
/// - URP系プロパティ名(_BaseMap/_BaseColorMap/_MainTex)と *_ST を自動解決
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class CameraToMeshAllInOne : MonoBehaviour
{
    public enum FitMode { Stretch, Contain, Cover }

    [Header("Source")]
    [Tooltip("映像を取得するカメラ")]
    public Camera sourceCamera;

    [Header("Target")]
    [Tooltip("映像を貼る先の Renderer（MeshRenderer/SkinnedMeshRenderer/SpriteRenderer など）")]
    public Renderer targetRenderer;

    [Tooltip("テクスチャを入れるシェーダープロパティ名。空なら自動（_BaseMap→_BaseColorMap→_MainTex）")]
    public string texturePropertyName = "";

    [Header("RenderTexture")]
    [Tooltip("ONで画面サイズに追従（width/heightは無視）")]
    public bool useScreenSize = true;
    [Min(1)] public int textureWidth = 1024;
    [Min(1)] public int textureHeight = 1024;

#if UNITY_2020_2_OR_NEWER
    public GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
    public GraphicsFormat depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
#else
    public RenderTextureFormat colorFormatLegacy = RenderTextureFormat.ARGB32;
    public RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;
    public int depthBufferBits = 24;
#endif
    [Range(1, 8)] public int antiAliasing = 1;
    public bool useSRGB = true;
    public bool useMipMap = false;

    [Header("Aspect Fit (表示が一部しか出ない対策)")]
    public FitMode fitMode = FitMode.Contain;

    [Header("Misc")]
    [Tooltip("エディタ停止中でも反映（ExecuteAlways時）")]
    public bool runInEditMode = true;

    RenderTexture _rt;
    MaterialPropertyBlock _mpb;
    int _texPropId = -1;
    string _stVectorName = null; // *_ST の実プロパティ名
    Vector2Int _cachedSize = Vector2Int.zero;

    static Vector2Int ScreenSize => new Vector2Int(Screen.width, Screen.height);

    // ===== Unity hooks =====
    void OnEnable()
    {
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        ResolveTexturePropertyName();
        ResolveSTVectorName();
        TryInit();
        ApplyTextureToRenderer();
        ApplyAspectFitST(); // 初回も反映
    }

    void OnDisable() => Teardown();
    void OnDestroy() => Teardown();

    void OnValidate()
    {
        antiAliasing = Mathf.Clamp(antiAliasing, 1, 8);
        if (enabled && gameObject.activeInHierarchy)
        {
            ResolveTexturePropertyName();
            ResolveSTVectorName();
            // サイズや形式が変わったら作り直す
            if (!useScreenSize)
                RecreateRT();
        }
    }

    void Update()
    {
        if (!Application.isPlaying && !runInEditMode) return;
        if (!sourceCamera || !targetRenderer) return;

        if (useScreenSize)
        {
            var s = ScreenSize;
            if (_cachedSize != s) RecreateRT();
        }

        // カメラにRTを確実にセット
        if (sourceCamera.targetTexture != _rt)
        {
            if (sourceCamera.targetTexture) sourceCamera.targetTexture = null;
            sourceCamera.targetTexture = _rt;
        }

        ApplyTextureToRenderer();
        ApplyAspectFitST();
    }

    // ===== Core =====
    void TryInit()
    {
        if (!sourceCamera || !targetRenderer) return;
        CreateRTIfNeeded();
        sourceCamera.targetTexture = _rt;
    }

    void CreateRTIfNeeded()
    {
        if (_rt != null) return;

        var size = useScreenSize
            ? ScreenSize
            : new Vector2Int(Mathf.Max(1, textureWidth), Mathf.Max(1, textureHeight));
        _cachedSize = size;

#if UNITY_2020_2_OR_NEWER
        var desc = new RenderTextureDescriptor(size.x, size.y)
        {
            msaaSamples = antiAliasing,
            graphicsFormat = colorFormat,
            depthStencilFormat = depthStencilFormat,
            sRGB = useSRGB,
            useMipMap = useMipMap,
            autoGenerateMips = false
        };
        _rt = new RenderTexture(desc);
#else
        _rt = new RenderTexture(size.x, size.y, depthBufferBits, colorFormatLegacy, readWrite)
        {
            antiAliasing = antiAliasing,
            useMipMap = useMipMap,
            autoGenerateMips = false
        };
#endif
        _rt.name = $"CameraToMesh_RT_{size.x}x{size.y}";
        _rt.Create();
    }

    void RecreateRT()
    {
        // カメラから外してから破棄（エラー回避）
        if (sourceCamera && sourceCamera.targetTexture == _rt)
            sourceCamera.targetTexture = null;

        if (_rt)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) Destroy(_rt);
            else DestroyImmediate(_rt);
#else
            _rt.Release();
            Destroy(_rt);
#endif
            _rt = null;
        }

        CreateRTIfNeeded();

        if (sourceCamera) sourceCamera.targetTexture = _rt;
    }

    void Teardown()
    {
        if (sourceCamera && sourceCamera.targetTexture == _rt)
            sourceCamera.targetTexture = null;

        if (_rt)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) Destroy(_rt);
            else DestroyImmediate(_rt);
#else
            _rt.Release();
            Destroy(_rt);
#endif
            _rt = null;
        }
    }

    void ApplyTextureToRenderer()
    {
        if (!targetRenderer) return;
        if (_texPropId < 0) ResolveTexturePropertyName();
        if (_texPropId < 0) return;

        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_texPropId, _rt);
        targetRenderer.SetPropertyBlock(_mpb);
    }

    void ResolveTexturePropertyName()
    {
        if (!string.IsNullOrEmpty(texturePropertyName))
        {
            _texPropId = Shader.PropertyToID(texturePropertyName);
            return;
        }
        _texPropId = -1;
        if (!targetRenderer || !targetRenderer.sharedMaterial) return;

        var mat = targetRenderer.sharedMaterial;
        string[] candidates = { "_BaseMap", "_BaseColorMap", "_MainTex" };
        foreach (var p in candidates)
        {
            if (mat.HasProperty(p))
            {
                texturePropertyName = p;
                _texPropId = Shader.PropertyToID(p);
                break;
            }
        }
        if (_texPropId < 0)
        {
            // 最後の手段として _MainTex を使う
            texturePropertyName = "_MainTex";
            _texPropId = Shader.PropertyToID("_MainTex");
        }
    }

    void ResolveSTVectorName()
    {
        _stVectorName = null;
        if (!targetRenderer || !targetRenderer.sharedMaterial) return;
        var mat = targetRenderer.sharedMaterial;

        if (mat.HasProperty("_BaseMap_ST")) { _stVectorName = "_BaseMap_ST"; return; }
        if (mat.HasProperty("_BaseColorMap_ST")) { _stVectorName = "_BaseColorMap_ST"; return; }
        if (mat.HasProperty("_MainTex_ST")) { _stVectorName = "_MainTex_ST"; return; }
        // 無い場合、このシェーダは *_ST を使っていない可能性が高い
    }

    // ===== Aspect fit (一部しか映らない/潰れる対策) =====
    void ApplyAspectFitST()
    {
        if (_rt == null || !targetRenderer) return;
        if (string.IsNullOrEmpty(_stVectorName)) return; // 対象シェーダが *_ST 非対応

        float texAspect = (float)_rt.width / Mathf.Max(1, _rt.height);

        // メッシュの「見せ面」アスペクト。汎用化のため1:1想定（板ポリならこれでOK）
        // 厳密にやるなら targetRenderer.bounds のX/Zやスクリーン投影から算出する。
        float meshAspect = 1.0f;

        Vector2 scale = Vector2.one;
        Vector2 offset = Vector2.zero;

        switch (fitMode)
        {
            case FitMode.Stretch:
                scale = Vector2.one; offset = Vector2.zero;
                break;

            case FitMode.Contain:
                if (texAspect > meshAspect)
                {
                    // 横長テクスチャ → 縦を縮めて上下に余白
                    float vh = texAspect / meshAspect; // >1
                    scale = new Vector2(1f, 1f / vh);
                    offset = new Vector2(0f, (1f - scale.y) * 0.5f);
                }
                else
                {
                    // 縦長テクスチャ → 横を縮めて左右に余白
                    float hw = meshAspect / texAspect; // >=1
                    scale = new Vector2(1f / hw, 1f);
                    offset = new Vector2((1f - scale.x) * 0.5f, 0f);
                }
                break;

            case FitMode.Cover:
                if (texAspect > meshAspect)
                {
                    // 横長テクスチャ → 縦を広げて上下をトリミング
                    float vh = texAspect / meshAspect;
                    scale = new Vector2(1f, vh);
                    offset = new Vector2(0f, (1f - scale.y) * 0.5f);
                }
                else
                {
                    // 縦長テクスチャ → 横を広げて左右をトリミング
                    float hw = meshAspect / texAspect;
                    scale = new Vector2(hw, 1f);
                    offset = new Vector2((1f - scale.x) * 0.5f, 0f);
                }
                break;
        }

        // *_ST = (scale.x, scale.y, offset.x, offset.y)
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetVector(_stVectorName, new Vector4(scale.x, scale.y, offset.x, offset.y));
        targetRenderer.SetPropertyBlock(_mpb);
    }

    // ===== Public utilities =====
    /// <summary>外部から再生成を要求（形式変更後など）</summary>
    public void RequestRecreate() => RecreateRT();

    /// <summary>現在の RenderTexture を取得</summary>
    public RenderTexture GetRenderTexture() => _rt;
}
