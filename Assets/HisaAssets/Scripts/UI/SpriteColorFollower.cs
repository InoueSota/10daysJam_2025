using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteColorFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("色を参照する相手（SpriteRenderer）")]
    public SpriteRenderer target;

    [Tooltip("target未指定時、このTagのオブジェクトから最初のSpriteRendererを探す（任意）")]
    public string fallbackTargetTag;

    [Header("Behavior")]
    [Tooltip("有効化時にまず一度だけコピー")]
    public bool copyOnEnable = true;

    [Tooltip("毎フレーム追従するか")]
    public bool followEveryFrame = true;

    [Tooltip("追従の補間速度（0で即時）。指数補間を使用")]
    [Min(0f)] public float lerpSpeed = 0f;

    [Tooltip("アルファもコピーする")]
    public bool copyAlpha = true;

    [Header("HSV Adjustments (任意)")]
    [Range(-1f, 1f)] public float addHue = 0f;          // 周期（±1で1周）
    [Range(-1f, 1f)] public float addSaturation = 0f;   // 彩度の加算
    [Range(-1f, 1f)] public float addValue = 0f;        // 明度の加算
    [Min(0f)] public float multiplyValue = 1f;          // 明度の乗算

    private SpriteRenderer self;

    void Reset()
    {
        self = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (!self) self = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        TryResolveTarget();
        if (copyOnEnable) ApplyColor(instant: true);
    }

    void Update()
    {
        if (followEveryFrame) ApplyColor(instant: false);
    }

    void TryResolveTarget()
    {
        if (target) return;
        if (!string.IsNullOrEmpty(fallbackTargetTag))
        {
            var go = GameObject.FindGameObjectWithTag(fallbackTargetTag);
            if (go) target = go.GetComponent<SpriteRenderer>();
        }
    }

    public void ApplyColor(bool instant)
    {
        if (!self || !target) return;

        // 参照元の色を取得
        Color src = target.color;

        // アルファをコピーしない設定なら、元の自分のアルファを維持
        if (!copyAlpha) src.a = self.color.a;

        // HSVで微調整（任意）
        Color.RGBToHSV(src, out float h, out float s, out float v);
        h = Mathf.Repeat(h + addHue, 1f);
        s = Mathf.Clamp01(s + addSaturation);
        v = Mathf.Clamp01(v * multiplyValue + addValue);
        Color dst = Color.HSVToRGB(h, s, v);
        dst.a = src.a; // アルファは上で決めたものを使う

        if (instant || lerpSpeed <= 0f)
        {
            self.color = dst;
        }
        else
        {
            // exp補間でフレームレートに依存しにくい追従
            float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
            self.color = Color.Lerp(self.color, dst, t);
        }
    }

    // 右クリックメニューから即時適用
    [ContextMenu("Apply Now")]
    void ApplyNowContext() => ApplyColor(instant: true);
}
