using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif
#if UNITY_2D_PIXELPERFECT
using UnityEngine.U2D;
#endif

/// 元カメラの見え方を“複製カメラ”でRTへ描画して平面に貼る。
/// PixelPerfectCamera が付いている場合は設定をコピーして整数拡大に揃える。
[DisallowMultipleComponent]
public class CameraToPlane_SafeCopyCam_Pixel : MonoBehaviour
{
    [Header("Source / Target")]
    public Camera sourceCamera;
    public Renderer targetRenderer;               // Quad等
    public string texturePropertyName = "_BaseMap";

    [Header("RenderTexture")]
    public bool useScreenSize = true;             // 画面サイズに追従
    [Min(1)] public int textureWidth = 1920;      // useScreenSize=false時のベース
    [Min(1)] public int textureHeight = 1080;
    [Range(0, 32)] public int depthBits = 24;
#if UNITY_2020_2_OR_NEWER
    public GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
#endif

    [Header("Pixel Perfect")]
    public bool inheritPixelPerfectFromSource = true; // 元カメラのPPCをコピー
    public bool forceIntegerScaleRT = true;           // RT解像度=参照解像度×整数スケールに丸める

    [Header("Aspect / Scale")]
    public float planeHeight = 1f;              // 見た目の高さ（幅はアスペクトで自動）
    public bool autoUpdateOnResolutionChange = true;
    public bool flipY = false;

    Camera _copyCam;
    RenderTexture _rt;
    int _lastW, _lastH;
    MaterialPropertyBlock _mpb;

#if UNITY_2D_PIXELPERFECT
    PixelPerfectCamera _ppcSrc, _ppcCopy;
#endif

    void OnEnable()
    {
        if (!sourceCamera || !targetRenderer)
        {
            Debug.LogWarning("[CameraToPlane] sourceCamera / targetRenderer を設定してください。");
            enabled = false; return;
        }
        CreateCopyCamera();
        SetupRTAndBindings();
    }

    void OnDisable()
    {
        if (_copyCam)
        {
            if (_copyCam.targetTexture == _rt) _copyCam.targetTexture = null;
            DestroyImmediate(_copyCam.gameObject);
            _copyCam = null;
        }
        if (_rt)
        {
            _rt.Release(); DestroyImmediate(_rt); _rt = null;
        }
    }

    void Update()
    {
        if (!_copyCam) return;

        int w = useScreenSize ? Mathf.Max(Screen.width, 1) : Mathf.Max(textureWidth, 1);
        int h = useScreenSize ? Mathf.Max(Screen.height, 1) : Mathf.Max(textureHeight, 1);
        if (autoUpdateOnResolutionChange && (w != _lastW || h != _lastH))
            SetupRTAndBindings();

        SyncCopyCameraFromSource();

        // フルフレーム描画（PPCがあればその処理を通る）
        _copyCam.Render();

        // メッシュの見た目アスペクトをRTに合わせる
        float aspect = (float)_rt.width / Mathf.Max(1, _rt.height);
        Vector3 s = new Vector3(planeHeight * aspect, planeHeight, 1f);
        if (flipY) s.y *= -1f;
        transform.localScale = s;
    }

    void CreateCopyCamera()
    {
        var go = new GameObject($"{name}_CaptureCam");
        go.hideFlags = HideFlags.HideAndDontSave;
        _copyCam = go.AddComponent<Camera>();
        _copyCam.enabled = false;
        _copyCam.rect = new Rect(0, 0, 1, 1);
        _copyCam.allowMSAA = false;  // アンチエイリアス無効（にじみ防止）
        _copyCam.allowHDR = false;

        _copyCam.clearFlags = CameraClearFlags.SolidColor;
        _copyCam.backgroundColor = new Color(0, 0, 0, 0);

        go.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);

#if UNITY_2D_PIXELPERFECT
        if (inheritPixelPerfectFromSource)
        {
            _ppcSrc = sourceCamera.GetComponent<PixelPerfectCamera>();
            if (_ppcSrc)
            {
                _ppcCopy = go.AddComponent<PixelPerfectCamera>();
                CopyPixelPerfectSettings(_ppcSrc, _ppcCopy);
            }
        }
#endif
    }

#if UNITY_2D_PIXELPERFECT
    void CopyPixelPerfectSettings(PixelPerfectCamera src, PixelPerfectCamera dst)
    {
        dst.assetsPPU      = src.assetsPPU;
        dst.refResolutionX = src.refResolutionX;
        dst.refResolutionY = src.refResolutionY;
        dst.upscaleRT      = src.upscaleRT;
        dst.pixelSnapping  = src.pixelSnapping;
        dst.cropFrameX     = src.cropFrameX;
        dst.cropFrameY     = src.cropFrameY;
        dst.stretchFill    = src.stretchFill;
    }
#endif

    void SetupRTAndBindings()
    {
        int targetW = useScreenSize ? Mathf.Max(Screen.width, 1) : Mathf.Max(textureWidth, 1);
        int targetH = useScreenSize ? Mathf.Max(Screen.height, 1) : Mathf.Max(textureHeight, 1);

#if UNITY_2D_PIXELPERFECT
        if (inheritPixelPerfectFromSource && _ppcSrc && forceIntegerScaleRT)
        {
            // 参照解像度×整数スケールに丸める（レタボ対応はPPC側に任せる）
            int refX = Mathf.Max(1, _ppcSrc.refResolutionX);
            int refY = Mathf.Max(1, _ppcSrc.refResolutionY);
            int k = Mathf.Max(1, Mathf.Min(targetW / refX, targetH / refY)); // 入る最大整数倍率
            targetW = refX * k;
            targetH = refY * k;
        }
#endif

        if (_rt && (_rt.width != targetW || _rt.height != targetH))
        {
            if (_copyCam && _copyCam.targetTexture == _rt) _copyCam.targetTexture = null;
            _rt.Release(); DestroyImmediate(_rt); _rt = null;
        }

        if (_rt == null)
        {
#if UNITY_2020_2_OR_NEWER
            var desc = new RenderTextureDescriptor(targetW, targetH)
            {
                depthBufferBits = depthBits,
                msaaSamples = 1,
                sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear)
            };
            desc.graphicsFormat = colorFormat;
            _rt = new RenderTexture(desc);
#else
            _rt = new RenderTexture(targetW, targetH, depthBits, RenderTextureFormat.ARGB32);
#endif
            _rt.name = $"CaptureRT_{targetW}x{targetH}";
            _rt.useMipMap = false;
            _rt.autoGenerateMips = false;
            _rt.filterMode = FilterMode.Point;   // ← 重要：最近傍で拡大
            _rt.anisoLevel = 0;
            _rt.Create();
        }

        if (_copyCam) _copyCam.targetTexture = _rt;

        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(texturePropertyName, _rt);
        _mpb.SetTexture("_MainTex", _rt);
        targetRenderer.SetPropertyBlock(_mpb);

        _lastW = targetW; _lastH = targetH;
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
    }
}
