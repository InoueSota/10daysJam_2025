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
    private SoundInstantiateScript sound;

    public void Initialize(Transform _pointA, Transform _pointB, float alpha, GameManager _gameManager)
    {
        // 自コンポーネントの取得
        sound = GetComponent<SoundInstantiateScript>();
        // サウンド
        sound.PlaySound(0, 0.5f);

        // LineRendererを追加
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // 線の太さ
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // マテリアル（デフォルトだと見えにくいので）
        string areaName = _gameManager.GetAreaName();
        if (areaName == "Area5") { goalLineMaterial = neonGoalLineMaterial; }
        else { goalLineMaterial = new Material(Shader.Find("Sprites/Default")); }

        lineRenderer.material = goalLineMaterial;
        if (areaName == "Area4")
        {
            lineRenderer.startColor = new(0f, 0f, 0f, 1f);
            lineRenderer.endColor = new(0f, 0f, 0f, 1f);
        }
        else
        {
            lineRenderer.startColor = new(0.99f, 0.42f, 0.41f, 1f);
            lineRenderer.endColor = new(0.99f, 0.42f, 0.41f, 1f);
        }

        // 頂点数は2
        lineRenderer.positionCount = 2;

        // 2点の設定
        pointA = _pointA;
        pointB = _pointB;

        //レイヤー指定
        lineRenderer.sortingOrder = 40;

        // 透明度の設定
        SetAlpha(alpha);

        // GameManagerの取得
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
            SelfCheckGoal();
        }
    }

    public void SelfCheckGoal()
    {
        // プレイヤーが触れたか判定
        RaycastHit2D hit = Physics2D.Linecast(pointA.position, pointB.position, characterLayer);

        if (hit.collider != null)
        {
            GameManager gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

            // 縦ゴール配置
            if (Mathf.Approximately(pointA.position.x, pointB.position.x))
            {
                gameManager.CheckGoal(false);
            }
            // 横ゴール配置
            else if (Mathf.Approximately(pointA.position.y, pointB.position.y))
            {
                gameManager.CheckGoal(true);
            }
        }
    }
}
