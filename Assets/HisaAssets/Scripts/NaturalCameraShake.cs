using UnityEngine;

/// <summary>
/// �J������C�ӂ̃I�u�W�F�N�g���u���R�Ɂv�h�炷�X�N���v�g�B
/// �EPerlinNoise�Ŋ��炩�ɗh���펞Sway
/// �E�C�x���g�p�̒P��ShakeOnce
/// �E�ʒu/��]�̋��x�E���E���g���E�����𒲐��\
/// �g�����F�J����(�܂��͐e�̋�I�u�W�F�N�g)�ɃA�^�b�`
/// </summary>
[DisallowMultipleComponent]
public class NaturalCameraShake : MonoBehaviour
{
    [Header("����")]
    [Tooltip("Time.timeScale�̉e�����󂯂Ȃ��h��ɂ���")]
    public bool unscaledTime = true;

    [Tooltip("�J�n�������ɂ�邭�h�炷")]
    public bool playSwayOnStart = true;

    [Tooltip("�m�C�Y�̓�������(���g���ɑ���)")]
    [Min(0f)] public float swaySpeed = 0.6f;

    [Header("Sway ���x�i�펞��炬�j")]
    public Vector3 swayPosStrength = new Vector3(0.01f, 0.01f, 0.0f); // ���[�g��
    public Vector3 swayRotStrength = new Vector3(0.2f, 0.2f, 0.3f);   // �x

    [Header("Shake�i�P���j")]
    [Tooltip("ShakeOnce�̃f�t�H���g����(�b)")]
    [Min(0f)] public float defaultShakeDuration = 0.35f;
    [Tooltip("Shake�J�n���̋��x�i�ʒu�E��]�̏搔�j")]
    [Range(0f, 5f)] public float shakeAmplitude = 1.0f;
    [Tooltip("Shake�̑����iSway�ɑ΂���{���j")]
    [Range(0.1f, 5f)] public float shakeSpeedMultiplier = 2.0f;
    [Tooltip("Shake�����B�傫���قǑ�������")]
    [Min(0f)] public float shakeDamping = 3.0f;

    [Header("�m�C�Y�ƍ��W�ݒ�")]
    [Tooltip("���[�J�����W�ŗh�炷�i�e�q�t���ɕ֗��j")]
    public bool useLocalSpace = true;
    [Tooltip("�m�C�Y�̃����_���I�t�Z�b�g��")]
    public int noiseSeed = 1234;

    // �������
    Vector3 _baseLocalPos;
    Quaternion _baseLocalRot;
    float _tSway;
    float _shakeTimeLeft;
    float _shakePhase;
    float _shakeAmpRuntime; // �����^�C���U��

    // �Œ�V�t�g�Ŋe���̃m�C�Y�ʑ��𕪗�
    float _offsetX, _offsetY, _offsetZ;

    void Awake()
    {
        _baseLocalPos = transform.localPosition;
        _baseLocalRot = transform.localRotation;

        var rng = new System.Random(noiseSeed);
        _offsetX = (float)rng.NextDouble() * 1000f;
        _offsetY = (float)rng.NextDouble() * 1000f + 111f;
        _offsetZ = (float)rng.NextDouble() * 1000f + 222f;
    }

    void OnEnable()
    {
        if (playSwayOnStart) _tSway = 0f;
    }

    void OnDisable()
    {
        // ���̎p���ɖ߂�
        transform.localPosition = _baseLocalPos;
        transform.localRotation = _baseLocalRot;
        _shakeTimeLeft = 0f;
        _shakeAmpRuntime = 0f;
    }

    void LateUpdate()
    {
        float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        // �x�[�X�F�펞Sway
        _tSway += swaySpeed * dt;
        Vector3 posSway = Vector3.zero;
        Vector3 rotSway = Vector3.zero;
        GetPerlinTriplet(_tSway, out float nx, out float ny, out float nz);

        posSway.x = (nx) * swayPosStrength.x;
        posSway.y = (ny) * swayPosStrength.y;
        posSway.z = (nz) * swayPosStrength.z;

        rotSway.x = (nx) * swayRotStrength.x;
        rotSway.y = (ny) * swayRotStrength.y;
        rotSway.z = (nz) * swayRotStrength.z;

        // �P��Shake�i���Ԍ����j
        if (_shakeTimeLeft > 0f)
        {
            _shakeTimeLeft -= dt;
            float t = Mathf.Clamp01(_shakeTimeLeft / Mathf.Max(0.0001f, defaultShakeDuration));
            // �����J�[�u�F�w���I�i���D�݂Łj
            float decay = Mathf.Exp(-shakeDamping * (1f - t));

            _shakePhase += swaySpeed * shakeSpeedMultiplier * dt;
            GetPerlinTriplet(_shakePhase, out float sx, out float sy, out float sz);

            // Sway�ɏ�悹
            posSway += new Vector3(sx, sy, sz) * _shakeAmpRuntime * 0.5f;
            rotSway += new Vector3(sz, sx, sy) * _shakeAmpRuntime * 3.0f;

            _shakeAmpRuntime *= decay;
        }
        else
        {
            _shakeAmpRuntime = 0f;
        }

        // ���f
        if (useLocalSpace)
        {
            transform.localPosition = _baseLocalPos + posSway;
            transform.localRotation = _baseLocalRot * Quaternion.Euler(rotSway);
        }
        else
        {
            transform.position = transform.parent ? transform.parent.TransformPoint(_baseLocalPos + posSway)
                                                  : _baseLocalPos + posSway;
            transform.rotation = (transform.parent ? transform.parent.rotation : Quaternion.identity)
                                 * _baseLocalRot * Quaternion.Euler(rotSway);
        }
    }

    /// <summary>
    /// �P���V�F�C�N�𔭉΁B�����ȗ��B
    /// </summary>
    public void ShakeOnce(float amplitude = -1f, float duration = -1f)
    {
        if (duration <= 0f) duration = defaultShakeDuration;
        if (amplitude < 0f) amplitude = shakeAmplitude;

        _shakeTimeLeft = duration;
        _shakeAmpRuntime = amplitude;
        // �ʑ������炵�đO��ƈႤ�g��炬�h
        _shakePhase += 37.123f;
    }

    /// <summary>
    /// Sway�̗L��/�����i��~���͌��p���ɖ߂��j
    /// </summary>
    public void SetSwayEnabled(bool enabled)
    {
        if (!enabled)
        {
            transform.localPosition = _baseLocalPos;
            transform.localRotation = _baseLocalRot;
        }
        playSwayOnStart = enabled;
    }

    // 0�}1��Perlin�m�C�Y3��
    void GetPerlinTriplet(float t, out float x, out float y, out float z)
    {
        x = Mathf.PerlinNoise(_offsetX + t, 0.0f) * 2f - 1f;
        y = Mathf.PerlinNoise(_offsetY + t, 0.0f) * 2f - 1f;
        z = Mathf.PerlinNoise(_offsetZ + t, 0.0f) * 2f - 1f;
    }
}
