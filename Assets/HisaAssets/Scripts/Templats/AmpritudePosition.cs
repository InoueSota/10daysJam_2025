using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmpritudePosition : MonoBehaviour
{
    [Header("揺れ量・時間")]
    [SerializeField] float ampritude = 1f;   // 振幅（単位: 距離）
    [SerializeField] float period = 0.2f;    // 揺れの周期（秒）
    [SerializeField] float easeTime = 0.5f;  // 総再生時間（秒）
    float easeT;
    public bool startEasing;

    [Header("方向設定")]
    [SerializeField] bool onlyY = true;                 // Y のみ揺らす
    [SerializeField] Vector3 direction = Vector3.up;    // onlyY=false のときに使う移動方向

    [Header("座標/時間の基準（3D/Transform 用）")]
    [SerializeField] bool useLocalPosition = true;      // ローカル座標で動かすか
    [SerializeField] bool unscaledTime = false;         // スローモーションの影響を受けない

    [SerializeField, Header("Image/RectTransform の場合")] bool imagePos = false;

    // 3D/Transform 用
    Vector3 initPos;

    // UI(Image) 用
    RectTransform rt;
    Vector2 initAnchoredPos;

    void Start()
    {
        easeT = 0f;

        if (imagePos)
        {
            // ImageでなくてもRectTransformがあればOK（UI要素）
            rt = GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning("imagePos が有効ですが RectTransform が見つかりません。通常の Transform 処理にフォールバックします。");
                imagePos = false; // フォールバック
            }
            else
            {
                initAnchoredPos = rt.anchoredPosition;
            }
        }

        if (!imagePos)
        {
            initPos = useLocalPosition ? transform.localPosition : transform.position;
        }
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

        if (imagePos)
        {
            // UI用：anchoredPosition で揺らす（X/Yのみ）
            Vector2 dir2D = onlyY
                ? Vector2.up
                : new Vector2(direction.x, direction.y);
            if (dir2D.sqrMagnitude < 1e-6f) dir2D = Vector2.up;
            dir2D = dir2D.normalized;

            Vector2 target = initAnchoredPos + dir2D * offsetMag;
            rt.anchoredPosition = target;
        }
        else
        {
            // 3D/Transform 用：position or localPosition
            Vector3 dir = onlyY ? Vector3.up :
                           (direction.sqrMagnitude < 1e-6f ? Vector3.up : direction.normalized);

            Vector3 targetPos = initPos + dir * offsetMag;
            if (useLocalPosition) transform.localPosition = targetPos;
            else transform.position = targetPos;
        }

        // 終了処理
        if (easeT >= easeTime)
        {
            startEasing = false;
            easeT = 0f;

            if (imagePos)
            {
                rt.anchoredPosition = initAnchoredPos;
            }
            else
            {
                if (useLocalPosition) transform.localPosition = initPos;
                else transform.position = initPos;
            }
        }
    }

    public void EaseStop()
    {
        easeT = 0f;
        startEasing = false;

        if (imagePos && rt != null)
        {
            rt.anchoredPosition = initAnchoredPos;
        }
        else
        {
            if (useLocalPosition) transform.localPosition = initPos;
            else transform.position = initPos;
        }
    }

    [ContextMenu("start")]
    public void EaseStart()
    {
        // 既に起動中ならリスタート
        if (startEasing) easeT = 0f;
        startEasing = true;

        // 基準位置を取り直したい場合は以下を有効化
        // if (imagePos) initAnchoredPos = rt.anchoredPosition;
        // else initPos = useLocalPosition ? transform.localPosition : transform.position;
    }
}
