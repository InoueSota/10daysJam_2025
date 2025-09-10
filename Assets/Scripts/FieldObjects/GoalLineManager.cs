using UnityEngine;

public class GoalLineManager : MonoBehaviour
{
    private Transform pointA; // 始点
    private Transform pointB; // 終点
    private LineRenderer lineRenderer;

    [SerializeField] private LayerMask characterLayer;
    Material goalLineMaterial;
    [SerializeField] private Material neonGoalLineMaterial;

    private float delayTimer;
    private GameManager gameManager;

    public void Initialize(Transform _pointA, Transform _pointB, float alpha, GameManager _gameManager)
    {
        // LineRendererを追加
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // 線の太さ
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // マテリアル（デフォルトだと見えにくいので）
        string areaName = _gameManager.GetAreaName();
        if (areaName == "Area5") { goalLineMaterial = neonGoalLineMaterial; }
        else {goalLineMaterial = new Material(Shader.Find("Sprites/Default")); }

        lineRenderer.material = goalLineMaterial;
         lineRenderer.startColor = new(0.99f, 0.42f, 0.41f, 1f);
        lineRenderer.endColor = new(0.99f, 0.42f, 0.41f, 1f);

        // 頂点数は2
        lineRenderer.positionCount = 2;

        // 2点の設定
        pointA = _pointA;
        pointB = _pointB;

        //レイヤー指定
        lineRenderer.sortingOrder = 40;

        // 透明度の設定
        SetAlpha(alpha);

        gameManager = _gameManager;

        // ディレイの初期化
        delayTimer = 0.03f;
    }
    public void SetAlpha(float alpha)
    {
        // 現在の色を取得
        Color start = lineRenderer.startColor;
        Color end = lineRenderer.endColor;

        // alphaを変更
        start.a = alpha;
        end.a = alpha;

        // 設定
        lineRenderer.startColor = start;
        lineRenderer.endColor = end;
    }

    void Update()
    {
        delayTimer -= Time.deltaTime;
        if (gameManager.GetUndoOrReset()) { delayTimer = 0.03f; }

        if (!pointA.gameObject.activeSelf || !pointB.gameObject.activeSelf) { Destroy(gameObject); }

        // 2点間を設定
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);

        // ゴール判定
        if (delayTimer < 0f)
        {
            // プレイヤーが触れたか判定
            RaycastHit2D hit = Physics2D.Linecast(pointA.position, pointB.position, characterLayer);

            if (hit.collider != null)
            {
                GameManager gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
                gameManager.CheckGoal();
            }
        }
    }
}
