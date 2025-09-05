using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering; // GraphicsFormat
#endif

[DisallowMultipleComponent]
public class CameraToSphere : MonoBehaviour
{
    [Header("Source")]
    public Camera sourceCamera;

    [Tooltip("映像を表示する Renderer（Sphere 等）")]
    public Renderer targetRenderer;

    [Header("RenderTexture")]
    [Tooltip("画面サイズに追従するならON")]
    public bool useScreenSize = false;
    public int textureWidth = 1024;
    public int textureHeight = 1024;
#if UNITY_2020_2_OR_NEWER
    public GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
    public GraphicsFormat depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
#else
    public RenderTextureFormat colorFormatLegacy = RenderTextureFormat.ARGB32;
    public int depthBitsLegacy = 24;
#endif

    [Header("UV Transform")]
    public bool flipX = false;
    public bool flipY = false;

    [Header("Edit Mode")]
    [Tooltip("編集モード中も見た目を更新したい場合ON")]
    public bool updateInEditMode = false;

    RenderTexture _rt;
    MaterialPropertyBlock _mpb;   // ← null のまま使わない
    Vector2 _appliedScale;
    Vector2 _appliedOffset;
    Texture _appliedTex;

    void OnEnable()
    {
        EnsureMPB();
        SafeSetup();
        ApplyPropertyBlockOnce(); // 最初の一回反映
    }

    void OnDisable()
    {
        if (sourceCamera && sourceCamera.targetTexture == _rt)
            sourceCamera.targetTexture = null;
    }

    void OnDestroy()
    {
        Teardown(); // 実際の破棄はここで
    }

    void Update()
    {
        if (!updateInEditMode && !Application.isPlaying) return;
        if (sourceCamera == null || targetRenderer == null) return;

        // MPBが未生成ならここで生成（保険）
        EnsureMPB();

        // 画面サイズ追従
        if (useScreenSize)
        {
            int w = Mathf.Max(1, Screen.width);
            int h = Mathf.Max(1, Screen.height);
            if (_rt == null || _rt.width != w || _rt.height != h)
                RecreateRT(w, h);
        }

        // Flip適用
        var scale = new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f);
        var offset = new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f);

        bool needApply = false;
        targetRenderer.GetPropertyBlock(_mpb); // ← 常に non-null

        if (_appliedTex != _rt)
        {
            _mpb.SetTexture("_BaseMap", _rt);
            _mpb.SetTexture("_MainTex", _rt);
            _appliedTex = _rt;
            needApply = true;
        }

        if (_appliedScale != scale || _appliedOffset != offset)
        {
            var st = new Vector4(scale.x, scale.y, offset.x, offset.y);
            _mpb.SetVector("_BaseMap_ST", st); // URP
            _mpb.SetVector("_MainTex_ST", st); // Built-in
            _appliedScale = scale;
            _appliedOffset = offset;
            needApply = true;
        }

        if (needApply)
            targetRenderer.SetPropertyBlock(_mpb);
    }

    void OnValidate()
    {
        // フィールドが未割り当ての可能性もあるのでガード多め
        EnsureMPB();
        SafeSetup();

        // Update を直接呼ばない。初期化順の問題を避けつつ1回だけ反映
        ApplyPropertyBlockOnce();
    }

    // --- helpers ---

    void EnsureMPB()
    {
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
    }

    void ApplyPropertyBlockOnce()
    {
        if (targetRenderer == null) return;
        EnsureMPB();

        targetRenderer.GetPropertyBlock(_mpb);
        if (_rt != null)
        {
            _mpb.SetTexture("_BaseMap", _rt);
            _mpb.SetTexture("_MainTex", _rt);
        }
        var scale = new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f);
        var offset = new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f);
        var st = new Vector4(scale.x, scale.y, offset.x, offset.y);
        _mpb.SetVector("_BaseMap_ST", st);
        _mpb.SetVector("_MainTex_ST", st);

        targetRenderer.SetPropertyBlock(_mpb);

        _appliedTex = _rt;
        _appliedScale = scale;
        _appliedOffset = offset;
    }

    void SafeSetup()
    {
        if (sourceCamera == null || targetRenderer == null) return;
        int w = useScreenSize ? Mathf.Max(1, Screen.width) : Mathf.Max(1, textureWidth);
        int h = useScreenSize ? Mathf.Max(1, Screen.height) : Mathf.Max(1, textureHeight);
        if (_rt == null || _rt.width != w || _rt.height != h)
            RecreateRT(w, h);
    }

    void RecreateRT(int w, int h)
    {
        if (_rt != null)
        {
            if (sourceCamera && sourceCamera.targetTexture == _rt)
                sourceCamera.targetTexture = null;
            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }

#if UNITY_2020_2_OR_NEWER
        var desc = new RenderTextureDescriptor(w, h)
        {
            msaaSamples = 1,
            mipCount = 1,
            useMipMap = false,
            autoGenerateMips = false,
            sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear),
            graphicsFormat = colorFormat,
            depthStencilFormat = depthStencilFormat
        };
        _rt = new RenderTexture(desc)
        {
            name = $"CameraToSphere_RT_{w}x{h}",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        _rt.Create();
#else
        _rt = new RenderTexture(w, h, depthBitsLegacy, colorFormatLegacy)
        {
            name = $"CameraToSphere_RT_{w}x{h}",
            useMipMap = false,
            autoGenerateMips = false,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        _rt.Create();
#endif
        if (sourceCamera) sourceCamera.targetTexture = _rt;

        // 次の反映で貼り直し
        _appliedTex = null;
    }

    void Teardown()
    {
        if (sourceCamera && sourceCamera.targetTexture == _rt)
            sourceCamera.targetTexture = null;

        if (_rt != null)
        {
            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }

        if (targetRenderer != null && _mpb != null)
        {
            _mpb.Clear();
            targetRenderer.SetPropertyBlock(_mpb);
        }

        _appliedTex = null;
    }
}
