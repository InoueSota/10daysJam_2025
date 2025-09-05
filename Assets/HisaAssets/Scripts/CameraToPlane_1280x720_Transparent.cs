using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif
#if UNITY_2D_PIXELPERFECT
using UnityEngine.U2D;
#endif

/// 1280x720 の RT に透明背景でキャプチャして、平面メッシュに貼る決め打ち版。
/// - 複製カメラで描画（rect/pixelRect/PixelPerfectの歪みを避ける）
/// - 背景は α=0 に固定（青いSolidColorは透明化）
/// - メッシュは 16:9 に自動スケール
[DisallowMultipleComponent]
public class CameraToPlane_1280x720_Transparent : MonoBehaviour
{
    [Header("Source / Target")]
    public Camera sourceCamera;          // 写したいカメラ
    public Renderer targetRenderer;      // QuadのMeshRenderer
    public string texturePropertyName = "_BaseMap";

    [Header("RenderTexture (fixed)")]
    public const int kWidth = 1280;
    public const int kHeight = 720;
    [Range(0, 32)] public int depthBits = 24;
#if UNITY_2020_2_OR_NEWER
    public GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_UNorm; // 透過用にα付き
#endif

    [Header("View / Scale")]
    public float planeHeight = 1f;       // 見た目の高さ（幅は自動）
    public bool flipY = false;           // 必要に応じて上下反転

    Camera _copyCam;
    RenderTexture _rt;
    MaterialPropertyBlock _mpb;

#if UNITY_2D_PIXELPERFECT
    PixelPerfectCamera _ppcSrc, _ppcCopy;
#endif

    void OnEnable()
    {
        if (!sourceCamera || !targetRenderer)
        {
            Debug.LogError("[CameraToPlane_1280x720_Transparent] sourceCamera / targetRenderer を設定してください。");
            enabled = false; return;
        }
        CreateCopyCamera();
        CreateOrResizeRT();
        BindToRenderer();
        FitPlane();
    }

    void OnDisable()
    {
        if (_copyCam)
        {
            if (_copyCam.targetTexture == _rt) _copyCam.targetTexture = null;
            DestroyImmediate(_copyCam.gameObject);
        }
        if (_rt)
        {
            _rt.Release(); DestroyImmediate(_rt);
        }
    }

    void LateUpdate()
    {
        if (!_copyCam) return;

        // 毎フレーム、元カメラの見え方を同期
        SyncCopyCameraFromSource();

        // 透明でクリア（URPのクリアより確実にα=0にしたいので明示）
        var prev = RenderTexture.active;
        RenderTexture.active = _rt;
        GL.Clear(true, true, new Color(
            sourceCamera.backgroundColor.r,  // RGBは元の青でもOK
            sourceCamera.backgroundColor.g,
            sourceCamera.backgroundColor.b,
            0f));                            // ただし α を 0 に固定
        RenderTexture.active = prev;

        // 描画
        _copyCam.Render();
    }

    // ---- internal ----

    void CreateCopyCamera()
    {
        var go = new GameObject($"{name}_CaptureCam");
        go.hideFlags = HideFlags.HideAndDontSave;
        _copyCam = go.AddComponent<Camera>();
        _copyCam.enabled = false;
        _copyCam.rect = new Rect(0, 0, 1, 1);       // 常にフル
        _copyCam.forceIntoRenderTexture = true;
        _copyCam.allowMSAA = false;
        _copyCam.allowHDR = false;

        // 透明背景（RGBは元の色、αだけ0に）
        var srcBg = sourceCamera ? sourceCamera.backgroundColor : Color.black;
        _copyCam.clearFlags = CameraClearFlags.SolidColor;
        _copyCam.backgroundColor = new Color(srcBg.r, srcBg.g, srcBg.b, 0f);

        go.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);

#if UNITY_2D_PIXELPERFECT
        _ppcSrc = sourceCamera.GetComponent<PixelPerfectCamera>();
        if (_ppcSrc)
        {
            _ppcCopy = go.AddComponent<PixelPerfectCamera>();
            _ppcCopy.assetsPPU      = _ppcSrc.assetsPPU;
            _ppcCopy.refResolutionX = _ppcSrc.refResolutionX;
            _ppcCopy.refResolutionY = _ppcSrc.refResolutionY;
            _ppcCopy.upscaleRT      = _ppcSrc.upscaleRT;
            _ppcCopy.pixelSnapping  = _ppcSrc.pixelSnapping;
            _ppcCopy.cropFrameX     = _ppcSrc.cropFrameX;
            _ppcCopy.cropFrameY     = _ppcSrc.cropFrameY;
            _ppcCopy.stretchFill    = _ppcSrc.stretchFill;
        }
#endif
    }

    void CreateOrResizeRT()
    {
        if (_rt && (_rt.width != kWidth || _rt.height != kHeight))
        {
            if (_copyCam && _copyCam.targetTexture == _rt) _copyCam.targetTexture = null;
            _rt.Release(); DestroyImmediate(_rt); _rt = null;
        }

        if (_rt == null)
        {
#if UNITY_2020_2_OR_NEWER
            var desc = new RenderTextureDescriptor(kWidth, kHeight)
            {
                depthBufferBits = depthBits,
                msaaSamples = 1,
                sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear)
            };
            desc.graphicsFormat = colorFormat;
            _rt = new RenderTexture(desc);
#else
            _rt = new RenderTexture(kWidth, kHeight, depthBits, RenderTextureFormat.ARGB32);
#endif
            _rt.name = $"CaptureRT_{kWidth}x{kHeight}";
            _rt.useMipMap = false;
            _rt.autoGenerateMips = false;
            _rt.filterMode = FilterMode.Point; // ドットを保つ
            _rt.anisoLevel = 0;
            _rt.Create();
        }

        _copyCam.targetTexture = _rt;
    }

    void BindToRenderer()
    {
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(texturePropertyName, _rt);
        _mpb.SetTexture("_MainTex", _rt); // 互換
        targetRenderer.SetPropertyBlock(_mpb);
    }

    void FitPlane()
    {
        float aspect = (float)kWidth / kHeight; // 16:9 固定
        Vector3 s = new Vector3(planeHeight * aspect, planeHeight, 1f);
        if (flipY) s.y *= -1f;
        transform.localScale = s;
    }

    void SyncCopyCameraFromSource()
    {
        _copyCam.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);

        _copyCam.orthographic = sourceCamera.orthographic;
        if (_copyCam.orthographic)
            _copyCam.orthographicSize = sourceCamera.orthographicSize;
        else
            _copyCam.fieldOfView = sourceCamera.fieldOfView;

        _copyCam.nearClipPlane = sourceCamera.nearClipPlane;
        _copyCam.farClipPlane = sourceCamera.farClipPlane;
        _copyCam.cullingMask = sourceCamera.cullingMask;
        _copyCam.depthTextureMode = sourceCamera.depthTextureMode;
        // pixelRect/rect は常にフル（0,0,1,1）→ 勝手な縮小の影響を受けない
    }
}
