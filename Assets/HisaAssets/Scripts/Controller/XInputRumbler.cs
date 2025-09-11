using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Xbox系コントローラの振動(XInput)ユーティリティ（パターン再生対応）。
/// ・InputManagerはそのまま、振動だけXInputで実現
/// ・StartRumble(left,right,duration) で単発
/// ・PlayPattern(segments) / PlayJaki() で“ジャキッ”などの複合パターン
/// </summary>
[DisallowMultipleComponent]
public class XInputRumbler : MonoBehaviour
{
    [Range(0, 3)] public int userIndex = 0;
    [Range(0f, 1f)] public float defaultLow = 0.5f;   // 小(左)
    [Range(0f, 1f)] public float defaultHigh = 1.0f;  // 大(右)

    // ---- 単発制御 ----
    float _oneshotTimer;
    bool _oneshotActive;

    // ---- パターン制御（非コルーチン / Update駆動）----
    struct Segment { public float L, R, T; public Segment(float l, float r, float t) { L = l; R = r; T = t; } }
    readonly Queue<Segment> _pattern = new();
    float _segRemain;      // 現在セグメントの残り秒
    bool _patternActive;
    float _patternGain = 1f; // 全体スケール

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [StructLayout(LayoutKind.Sequential)] struct XINPUT_VIBRATION { public ushort wLeftMotorSpeed; public ushort wRightMotorSpeed; }
    [StructLayout(LayoutKind.Sequential)] struct XINPUT_GAMEPAD { public ushort wButtons; public byte bLeftTrigger, bRightTrigger; public short sThumbLX, sThumbLY, sThumbRX, sThumbRY; }
    [StructLayout(LayoutKind.Sequential)] struct XINPUT_STATE { public uint dwPacketNumber; public XINPUT_GAMEPAD Gamepad; }
    const int ERROR_SUCCESS = 0;
    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")] static extern uint XInputSetState_1_4(uint i, ref XINPUT_VIBRATION v);
    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")] static extern uint XInputGetState_1_4(uint i, out XINPUT_STATE s);
    [DllImport("xinput1_3.dll", EntryPoint = "XInputSetState")] static extern uint XInputSetState_1_3(uint i, ref XINPUT_VIBRATION v);
    [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")] static extern uint XInputGetState_1_3(uint i, out XINPUT_STATE s);
    [DllImport("xinput9_1_0.dll", EntryPoint = "XInputSetState")] static extern uint XInputSetState_9_1_0(uint i, ref XINPUT_VIBRATION v);
    [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")] static extern uint XInputGetState_9_1_0(uint i, out XINPUT_STATE s);
    static bool _tried14, _tried13, _tried910, _has14, _has13, _has910;

    static uint SafeGetState(uint idx, out XINPUT_STATE st)
    {
        st = default;
        try { if (!_tried14) { _tried14 = true; _has14 = true; } if (_has14) return XInputGetState_1_4(idx, out st); } catch (DllNotFoundException) { _has14 = false; }
        try { if (!_tried13) { _tried13 = true; _has13 = true; } if (_has13) return XInputGetState_1_3(idx, out st); } catch (DllNotFoundException) { _has13 = false; }
        try { if (!_tried910) { _tried910 = true; _has910 = true; } if (_has910) return XInputGetState_9_1_0(idx, out st); } catch (DllNotFoundException) { _has910 = false; }
        return 1;
    }
    static uint SafeSetState(uint idx, ref XINPUT_VIBRATION vib)
    {
        try { if (_has14 || !_tried14) { _tried14 = true; _has14 = true; return XInputSetState_1_4(idx, ref vib); } } catch (DllNotFoundException) { _has14 = false; }
        try { if (_has13 || !_tried13) { _tried13 = true; _has13 = true; return XInputSetState_1_3(idx, ref vib); } } catch (DllNotFoundException) { _has13 = false; }
        try { if (_has910 || !_tried910) { _tried910 = true; _has910 = true; return XInputSetState_9_1_0(idx, ref vib); } } catch (DllNotFoundException) { _has910 = false; }
        return 1;
    }
#endif

    // ======= Public API =======

    public bool IsConnected()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return SafeGetState((uint)userIndex, out var _) == ERROR_SUCCESS;
#else
        return false;
#endif
    }

    /// 単発：0～1強度で duration 秒だけ振動
    public void StartRumble(float left, float right, float duration)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // パターン再生中なら止める（上書き）
        StopPattern();
        SetVibration01(left, right);
        _oneshotTimer = Mathf.Max(0f, duration);
        _oneshotActive = duration > 0f;
#else
        Debug.LogWarning("XInput rumble is only available on Windows.");
#endif
    }
    public void StartRumble(float duration) => StartRumble(defaultLow, defaultHigh, duration);

    public void StopRumble()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        SetVibration01(0, 0);
        _oneshotTimer = 0f;
        _oneshotActive = false;
        StopPattern();
#endif
    }

    /// パターンの再生。与えたセグメント列を順に再生（各セグメントは L/R 強度と秒）。
    public void PlayPattern(IEnumerable<(float L, float R, float T)> segments, float gain = 1f)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        _pattern.Clear();
        foreach (var s in segments)
        {
            var l = Mathf.Clamp01(s.L * gain);
            var r = Mathf.Clamp01(s.R * gain);
            var t = Mathf.Max(0f, s.T);
            if (t > 0f) _pattern.Enqueue(new Segment(l, r, t));
        }
        _patternGain = 1f;   // すでに gain 反映済み
        _patternActive = _pattern.Count > 0;
        _oneshotActive = false; // 上書き
        NextSegOrStop();
#endif
    }

    /// “ジャキッ”：鋭い刃→休止→軽い余韻（総尺 ~90〜120ms）
    /// intensity: 全体スケール、hardness: 右(高周波)の鋭さを強める比率
    public void PlayJaki(float intensity = 1.0f, float hardness = 1.0f)
    {
        //intensity = Mathf.Clamp01(intensity);
        //hardness = Mathf.Clamp01(hardness);

        // 推奨プロファイル（編集OK）
        // 1) 右モータ 18ms 強スパイク（刃）
        // 2) 休止 20ms（無音で“切れ味”を感じさせる）
        // 3) 左モータ 35ms 中弱（手に残る余韻）
        // 4) 微スパイク 8ms（仕上げの“カチッ”）
        float r1 = Mathf.Lerp(0.8f, 1.0f, hardness) * intensity;
        float r4 = Mathf.Lerp(0.3f, 0.6f, hardness) * intensity;
        float l3 = Mathf.Clamp01(0.5f * intensity);

        var list = new (float L, float R, float T)[]{
            (0.9f * intensity, 0.3f, 0.050f), // 左をメインに50msズドン
        (0f, 0f,            0.020f),      // 休止で切れ味
        (0.7f * intensity, 0.0f, 0.080f), // 左だけでゴォンと余韻
        (0.4f * intensity, 0.2f, 0.030f), // 小さめの追撃
        };
        PlayPattern(list, 1f);
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void SetVibration01(float left, float right)
    {
        var vib = new XINPUT_VIBRATION
        {
            wLeftMotorSpeed = (ushort)Mathf.RoundToInt(left * 65535f),
            wRightMotorSpeed = (ushort)Mathf.RoundToInt(right * 65535f)
        };
        _ = SafeSetState((uint)userIndex, ref vib);
    }
#endif

    void StopPattern()
    {
        _pattern.Clear();
        _patternActive = false;
        _segRemain = 0f;
    }

    void NextSegOrStop()
    {
        if (_pattern.Count == 0)
        {
            _patternActive = false;
            _segRemain = 0f;
            SetVibration01(0, 0);
            return;
        }
        var s = _pattern.Dequeue();
        SetVibration01(s.L, s.R);
        _segRemain = s.T;
    }

    void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        float dt = Time.unscaledDeltaTime; // ポーズ中もパターンは進む

        // 単発
        if (_oneshotActive)
        {
            _oneshotTimer -= dt;
            if (_oneshotTimer <= 0f)
            {
                _oneshotActive = false;
                SetVibration01(0, 0);
            }
        }

        // パターン
        if (_patternActive)
        {
            _segRemain -= dt;
            if (_segRemain <= 0f)
            {
                NextSegOrStop();
            }
        }

        //// --- デモ: A ボタンで「ジャキッ」 ---
        //if (Input.GetKeyDown(KeyCode.JoystickButton0))
        //{
        //    PlayJaki(1.0f, 1.0f);
        //}
#endif
    }
}
