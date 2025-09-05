using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Rendering; // GraphicsFormat
#endif

/// <summary>
/// �w��J�����̕`��� RenderTexture �ɏo�͂��A�C�ӂ� Renderer �ɓ\��t����I�[���C�������B
/// - ��ʃT�C�Y�Ǐ]/�Œ�𑜓x�̐ؑ�
/// - MaterialPropertyBlock�ň��S�ɓK�p
/// - targetTexture�������j���̏��ŃN���[���A�b�v�i�G���[����j
/// - �A�X�y�N�g�␳: Stretch / Contain / Cover
/// - URP�n�v���p�e�B��(_BaseMap/_BaseColorMap/_MainTex)�� *_ST ����������
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class CameraToMeshAllInOne : MonoBehaviour
{
    public enum FitMode { Stretch, Contain, Cover }

    [Header("Source")]
    [Tooltip("�f�����擾����J����")]
    public Camera sourceCamera;

    [Header("Target")]
    [Tooltip("�f����\���� Renderer�iMeshRenderer/SkinnedMeshRenderer/SpriteRenderer �Ȃǁj")]
    public Renderer targetRenderer;

    [Tooltip("�e�N�X�`��������V�F�[�_�[�v���p�e�B���B��Ȃ玩���i_BaseMap��_BaseColorMap��_MainTex�j")]
    public string texturePropertyName = "";

    [Header("RenderTexture")]
    [Tooltip("ON�ŉ�ʃT�C�Y�ɒǏ]�iwidth/height�͖����j")]
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

    [Header("Aspect Fit (�\�����ꕔ�����o�Ȃ��΍�)")]
    public FitMode fitMode = FitMode.Contain;

    [Header("Misc")]
    [Tooltip("�G�f�B�^��~���ł����f�iExecuteAlways���j")]
    public bool runInEditMode = true;

    RenderTexture _rt;
    MaterialPropertyBlock _mpb;
    int _texPropId = -1;
    string _stVectorName = null; // *_ST �̎��v���p�e�B��
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
        ApplyAspectFitST(); // ��������f
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
            // �T�C�Y��`�����ς�������蒼��
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

        // �J������RT���m���ɃZ�b�g
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
        // �J��������O���Ă���j���i�G���[����j
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
            // �Ō�̎�i�Ƃ��� _MainTex ���g��
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
        // �����ꍇ�A���̃V�F�[�_�� *_ST ���g���Ă��Ȃ��\��������
    }

    // ===== Aspect fit (�ꕔ�����f��Ȃ�/�ׂ��΍�) =====
    void ApplyAspectFitST()
    {
        if (_rt == null || !targetRenderer) return;
        if (string.IsNullOrEmpty(_stVectorName)) return; // �ΏۃV�F�[�_�� *_ST ��Ή�

        float texAspect = (float)_rt.width / Mathf.Max(1, _rt.height);

        // ���b�V���́u�����ʁv�A�X�y�N�g�B�ėp���̂���1:1�z��i�|���Ȃ炱���OK�j
        // �����ɂ��Ȃ� targetRenderer.bounds ��X/Z��X�N���[�����e����Z�o����B
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
                    // �����e�N�X�`�� �� �c���k�߂ď㉺�ɗ]��
                    float vh = texAspect / meshAspect; // >1
                    scale = new Vector2(1f, 1f / vh);
                    offset = new Vector2(0f, (1f - scale.y) * 0.5f);
                }
                else
                {
                    // �c���e�N�X�`�� �� �����k�߂č��E�ɗ]��
                    float hw = meshAspect / texAspect; // >=1
                    scale = new Vector2(1f / hw, 1f);
                    offset = new Vector2((1f - scale.x) * 0.5f, 0f);
                }
                break;

            case FitMode.Cover:
                if (texAspect > meshAspect)
                {
                    // �����e�N�X�`�� �� �c���L���ď㉺���g���~���O
                    float vh = texAspect / meshAspect;
                    scale = new Vector2(1f, vh);
                    offset = new Vector2(0f, (1f - scale.y) * 0.5f);
                }
                else
                {
                    // �c���e�N�X�`�� �� �����L���č��E���g���~���O
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
    /// <summary>�O������Đ�����v���i�`���ύX��Ȃǁj</summary>
    public void RequestRecreate() => RecreateRT();

    /// <summary>���݂� RenderTexture ���擾</summary>
    public RenderTexture GetRenderTexture() => _rt;
}
