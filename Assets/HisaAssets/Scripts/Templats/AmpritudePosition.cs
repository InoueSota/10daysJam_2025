using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmpritudePosition : MonoBehaviour
{
    [Header("揺れ量・時間")]
    [SerializeField] float ampritude = 1f;   // 振幅（単位: 距離）
    [SerializeField] float period = 0.2f;    // 揺れの周期（秒）
    [SerializeField] float easeTime = 0.5f;  // 総再生時間（秒）
    float easeT;
    public bool startEasing;

    [Header("方向設定")]
    [SerializeField] bool onlyY = true;           // Y のみ揺らす
    [SerializeField] Vector3 direction = Vector3.up; // onlyY=false のときに使う移動方向

    [Header("座標/時間の基準")]
    [SerializeField] bool useLocalPosition = true; // ローカル座標で動かすか
    [SerializeField] bool unscaledTime = false;    // スローモーションの影響を受けない

    Vector3 initPos;

    void Start()
    {
        easeT = 0f;
        initPos = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        if (!startEasing) return;

        // 時間加算
        easeT += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // 0→1 に正規化
        float u = Mathf.Clamp01(easeT / Mathf.Max(0.0001f, easeTime));

        // 減衰エンベロープ（EaseOut）：1 → 0 へスムーズに
        float envelope = 1f - Mathf.SmoothStep(0f, 1f, u);

        // 正弦波
        float s = Mathf.Sin(2f * Mathf.PI * (easeT / Mathf.Max(0.0001f, period)));

        // 振幅 × 正弦 × 減衰
        float offsetMag = ampritude * s * envelope;

        // 方向決定
        Vector3 dir = onlyY ? Vector3.up :
                      (direction.sqrMagnitude < 1e-6f ? Vector3.up : direction.normalized);

        // 位置更新
        Vector3 targetPos = initPos + dir * offsetMag;
        if (useLocalPosition) transform.localPosition = targetPos;
        else transform.position = targetPos;

        // 終了処理
        if (easeT >= easeTime)
        {
            startEasing = false;
            easeT = 0f;
            if (useLocalPosition) transform.localPosition = initPos;
            else transform.position = initPos;
        }
    }

    public void EaseStop()
    {
        easeT = 0f;
        startEasing = false;
        if (useLocalPosition) transform.localPosition = initPos;
        else transform.position = initPos;
    }

    [ContextMenu("start")]
    public void EaseStart()
    {
        // 既に起動中ならリスタート
        if (startEasing) easeT = 0f;
        startEasing = true;

        // 基準位置を取り直したい場合は以下を有効化
        // initPos = useLocalPosition ? transform.localPosition : transform.position;
    }
}
