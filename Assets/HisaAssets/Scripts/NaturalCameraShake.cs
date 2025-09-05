using UnityEngine;

/// <summary>
/// カメラや任意のオブジェクトを「自然に」揺らすスクリプト。
/// ・PerlinNoiseで滑らかに揺れる常時Sway
/// ・イベント用の単発ShakeOnce
/// ・位置/回転の強度・軸・周波数・減衰を調整可能
/// 使い方：カメラ(または親の空オブジェクト)にアタッチ
/// </summary>
[DisallowMultipleComponent]
public class NaturalCameraShake : MonoBehaviour
{
    [Header("共通")]
    [Tooltip("Time.timeScaleの影響を受けない揺れにする")]
    public bool unscaledTime = true;

    [Tooltip("開始時から常にゆるく揺らす")]
    public bool playSwayOnStart = true;

    [Tooltip("ノイズの動く速さ(周波数に相当)")]
    [Min(0f)] public float swaySpeed = 0.6f;

    [Header("Sway 強度（常時ゆらぎ）")]
    public Vector3 swayPosStrength = new Vector3(0.01f, 0.01f, 0.0f); // メートル
    public Vector3 swayRotStrength = new Vector3(0.2f, 0.2f, 0.3f);   // 度

    [Header("Shake（単発）")]
    [Tooltip("ShakeOnceのデフォルト時間(秒)")]
    [Min(0f)] public float defaultShakeDuration = 0.35f;
    [Tooltip("Shake開始時の強度（位置・回転の乗数）")]
    [Range(0f, 5f)] public float shakeAmplitude = 1.0f;
    [Tooltip("Shakeの速さ（Swayに対する倍率）")]
    [Range(0.1f, 5f)] public float shakeSpeedMultiplier = 2.0f;
    [Tooltip("Shake減衰。大きいほど早く収束")]
    [Min(0f)] public float shakeDamping = 3.0f;

    [Header("ノイズと座標設定")]
    [Tooltip("ローカル座標で揺らす（親子付けに便利）")]
    public bool useLocalSpace = true;
    [Tooltip("ノイズのランダムオフセット種")]
    public int noiseSeed = 1234;

    // 内部状態
    Vector3 _baseLocalPos;
    Quaternion _baseLocalRot;
    float _tSway;
    float _shakeTimeLeft;
    float _shakePhase;
    float _shakeAmpRuntime; // ランタイム振幅

    // 固定シフトで各軸のノイズ位相を分離
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
        // 元の姿勢に戻す
        transform.localPosition = _baseLocalPos;
        transform.localRotation = _baseLocalRot;
        _shakeTimeLeft = 0f;
        _shakeAmpRuntime = 0f;
    }

    void LateUpdate()
    {
        float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        // ベース：常時Sway
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

        // 単発Shake（時間減衰）
        if (_shakeTimeLeft > 0f)
        {
            _shakeTimeLeft -= dt;
            float t = Mathf.Clamp01(_shakeTimeLeft / Mathf.Max(0.0001f, defaultShakeDuration));
            // 減衰カーブ：指数的（お好みで）
            float decay = Mathf.Exp(-shakeDamping * (1f - t));

            _shakePhase += swaySpeed * shakeSpeedMultiplier * dt;
            GetPerlinTriplet(_shakePhase, out float sx, out float sy, out float sz);

            // Swayに上乗せ
            posSway += new Vector3(sx, sy, sz) * _shakeAmpRuntime * 0.5f;
            rotSway += new Vector3(sz, sx, sy) * _shakeAmpRuntime * 3.0f;

            _shakeAmpRuntime *= decay;
        }
        else
        {
            _shakeAmpRuntime = 0f;
        }

        // 反映
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
    /// 単発シェイクを発火。引数省略可。
    /// </summary>
    public void ShakeOnce(float amplitude = -1f, float duration = -1f)
    {
        if (duration <= 0f) duration = defaultShakeDuration;
        if (amplitude < 0f) amplitude = shakeAmplitude;

        _shakeTimeLeft = duration;
        _shakeAmpRuntime = amplitude;
        // 位相をずらして前回と違う“ゆらぎ”
        _shakePhase += 37.123f;
    }

    /// <summary>
    /// Swayの有効/無効（停止時は元姿勢に戻す）
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

    // 0±1のPerlinノイズ3軸
    void GetPerlinTriplet(float t, out float x, out float y, out float z)
    {
        x = Mathf.PerlinNoise(_offsetX + t, 0.0f) * 2f - 1f;
        y = Mathf.PerlinNoise(_offsetY + t, 0.0f) * 2f - 1f;
        z = Mathf.PerlinNoise(_offsetZ + t, 0.0f) * 2f - 1f;
    }
}
