using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    [SerializeField] private LayerMask groundLayer;

    [Header("[Prefab] 予測ボックス")]
    [SerializeField] private GameObject predictionBoxPrefab;
    private GameObject predictionBox;
    private SpriteRenderer predictionBoxRenderer;

    [Header("色変化速度")]
    [SerializeField] private float alphaChangePower;
    private float alphaTargetValue;

    [Header("移動速度")]
    [SerializeField] private float targetPower;
    private Vector3 targetPosition;

    [SerializeField] LineRenderer targetLinePrefab;

    // 半分の大きさ
    private float halfSize;

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start()
    {
        // 自コンポーネントの取得
        controller = GetComponent<PlayerController>();

        // ゴースト用オブジェクトを生成して非表示
        predictionBox = Instantiate(predictionBoxPrefab, transform.position, Quaternion.identity);
        predictionBoxRenderer = predictionBox.GetComponent<SpriteRenderer>();

        // 目標値の初期化
        alphaTargetValue = 0f;
        targetPosition = transform.position;

        halfSize = transform.localScale.x * 0.5f;

        
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    public void ManualUpdate()
    {
        // 表示 / 非表示を切り替える
        ToggleHide();
        // 表示位置の決定
        ShowPrediction();
        // 表示位置に移動
        PositionUpdate();
        //線の更新
        LineUpdate();
    }

    /// <summary>
    /// 表示 / 非表示の切り替え
    /// </summary>
    void ToggleHide()
    {
        // 非表示にする
        if (!controller.GetJustStanding() || (Mathf.Abs(Input.GetAxisRaw("Horizontal")) <= 0.5f && Mathf.Abs(Input.GetAxisRaw("Vertical")) <= 0.5f))
        {
            alphaTargetValue = 0f;
        }
        // 表示にする
        else
        {
            alphaTargetValue = 1f;
        }

        // 現在の透明度の取得
        Color currentColor = predictionBoxRenderer.color;

        // 目標の透明度に変化
        currentColor.a += (alphaTargetValue - currentColor.a) * (alphaChangePower * Time.deltaTime);

        // SpriteRendererに反映
        predictionBoxRenderer.color = currentColor;
    }

    /// <summary>
    /// 表示位置の決定
    /// </summary>
    void ShowPrediction()
    {
        Vector2 start = transform.position;
        Vector2 direction = Vector2.zero;

        if (controller.GetJustStanding() && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            // 左右入力の方が上下入力よりも値を上回っているとき
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > Mathf.Abs(Input.GetAxisRaw("Vertical")))
            {
                if (Input.GetAxisRaw("Horizontal") > 0f)
                {
                    direction.x = 1f;
                }
                else
                {
                    direction.x = -1f;
                }
            }
            // 上下入力の方が左右入力よりも値を上回っているとき
            else
            {
                if (Input.GetAxisRaw("Vertical") > 0f)
                {
                    direction.y = 1f;
                }
                else
                {
                    direction.y = -1f;
                }
            }
        }
        else
        {
            targetPosition = transform.position;
        }

        // プレイヤーの方向にRayを飛ばす
        RaycastHit2D hit = Physics2D.Raycast(start, direction.normalized, 100f, groundLayer);

        if (hit.collider != null && !hit.collider.GetComponent<AllFieldObjectManager>().GetIsTriggerObject())
        {
            // 壁までの距離 -1 マスを考慮
            Vector2 finalPos = hit.collider.transform.position;
            finalPos -= direction; // 1マス手前に調整

            // 予測ボックスをそこに表示
            targetPosition = finalPos;
        }
        // １マス手前に調整する必要のないものに触れたらその座標にする
        else if (hit.collider != null && hit.collider.GetComponent<AllFieldObjectManager>().GetIsTriggerObject())
        {
            // 壁までの距離
            Vector2 finalPos = hit.collider.transform.position;

            // 予測ボックスをそこに表示
            targetPosition = finalPos;
        }
        // 進行方向にブロックが1つもないなら画面端まで飛ばす
        else if (hit.collider == null)
        {
            // 左
            if (direction.x == -1f) { targetPosition = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + halfSize, transform.position.y, 0f); }
            // 右
            if (direction.x == 1f) { targetPosition = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0.75f, 0, 0)).x - halfSize, transform.position.y, 0f); }
            // 上
            if (direction.y == 1f) { targetPosition = new Vector3(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - halfSize, 0f); }
            // 下
            if (direction.y == -1f) { targetPosition = new Vector3(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + halfSize, 0f); }
        }
    }

    /// <summary>
    /// 表示位置に移動
    /// </summary>
    void PositionUpdate()
    {
        // 現在位置の取得
        Vector3 currentPosition = predictionBox.transform.position;

        // 目標位置に移動
        currentPosition += (targetPosition - currentPosition) * (targetPower * Time.deltaTime);

        // Transformに反映
        predictionBox.transform.position = currentPosition;
    }

    void LineUpdate()
    {
        targetLinePrefab.SetPosition(0, controller.transform.position);
        Debug.Log(controller.transform.position);
        if (controller.GetJustStanding() && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            targetLinePrefab.SetPosition(1, predictionBox.transform.position);
        }
        else
        {
            targetLinePrefab.SetPosition(1, targetPosition);
        }
    }
}
