using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // 目標座標
    private Vector3 targetPosition;

    [Header("速度")]
    [SerializeField] private float targetPower;

    [SerializeField] public LayerMask groundLayer; // 壁や障害物用レイヤー
    public GameObject predictionBoxPrefab; // 予測ボックス（ゴースト）
    private GameObject predictionBox;

    void Start()
    {
        // 自コンポーネントの取得
        controller = GetComponent<PlayerController>();

        // ゴースト用オブジェクトを生成して非表示
        predictionBox = Instantiate(predictionBoxPrefab);
        predictionBox.SetActive(false);
    }

    public void ShowPrediction(Vector2 direction)
    {
        Vector2 start = transform.position;

        // プレイヤーの方向にRayを飛ばす
        RaycastHit2D hit = Physics2D.Raycast(start, direction, 100f, groundLayer);

        if (hit.collider != null)
        {
            // 壁の手前に到達する座標を求める
            Vector2 hitPos = hit.point;

            // --- グリッドにスナップする場合 ---
            // 壁までの距離 -1 マスを考慮
            Vector2 finalPos = hit.collider.transform.position;
            finalPos -= direction; // 1マス手前に調整

            // 予測ボックスをそこに表示
            predictionBox.SetActive(true);
            predictionBox.transform.position = finalPos;
        }
        else
        {
            // 壁がない場合は非表示
            predictionBox.SetActive(false);
        }
    }

    public void ManualUpdate()
    {
        
    }
}
