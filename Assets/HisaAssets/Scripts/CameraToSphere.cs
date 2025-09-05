using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering; // GraphicsFormat
#endif

[DisallowMultipleComponent]
public class CameraToSphere : MonoBehaviour
{
    [Header("Source")]
    public Camera sourceCamera;

    [Tooltip("�f����\������ Renderer�iSphere ���j")]
    public Renderer targetRenderer;

    [Header("RenderTexture")]
    [Tooltip("��ʃT�C�Y�ɒǏ]����Ȃ�ON")]
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
    [Tooltip("�ҏW���[�h���������ڂ��X�V�������ꍇON")]
    public bool updateInEditMode = false;

    RenderTexture _rt;
    MaterialPropertyBlock _mpb;   // �� null �̂܂܎g��Ȃ�
    Vector2 _appliedScale;
    Vector2 _appliedOffset;
    Texture _appliedTex;

    void OnEnable()
    {
        EnsureMPB();
        SafeSetup();
        ApplyPropertyBlockOnce(); // �ŏ��̈�񔽉f
    }

    void OnDisable()
    {
        if (sourceCamera && sourceCamera.targetTexture == _rt)
            sourceCamera.targetTexture = null;
    }

    void OnDestroy()
    {
        Teardown(); // ���ۂ̔j���͂�����
    }

    void Update()
    {
        if (!updateInEditMode && !Application.isPlaying) return;
        if (sourceCamera == null || targetRenderer == null) return;

        // MPB���������Ȃ炱���Ő����i�ی��j
        EnsureMPB();

        // ��ʃT�C�Y�Ǐ]
        if (useScreenSize)
        {
            int w = Mathf.Max(1, Screen.width);
            int h = Mathf.Max(1, Screen.height);
            if (_rt == null || _rt.width != w || _rt.height != h)
                RecreateRT(w, h);
        }

        // Flip�K�p
        var scale = new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f);
        var offset = new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f);

        bool needApply = false;
        targetRenderer.GetPropertyBlock(_mpb); // �� ��� non-null

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
        // �t�B�[���h�������蓖�Ẳ\��������̂ŃK�[�h����
        EnsureMPB();
        SafeSetup();

        // Update �𒼐ڌĂ΂Ȃ��B���������̖��������1�񂾂����f
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

        // ���̔��f�œ\�蒼��
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
