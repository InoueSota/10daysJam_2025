using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;
    private PlayerCut cut;

    // 子コンポーネント
    [SerializeField] private DeathEffectSpawner deathEffectSpawner;

    // 他コンポーネント
    private UndoManager undoManager;
    private Camera mainCamera;

    void Start()
    {
        // 自コンポーネントを取得
        controller = GetComponent<PlayerController>();
        cut = GetComponent<PlayerCut>();

        // 他コンポーネントを取得
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // スタックしていないとき
        if (!controller.GetIsStacking())
        {
            cut.ManualUpdate();
        }
        controller.ManualUpdate();

        // 死亡処理
        DeathChecker();

        // Undo
        if (Input.GetButtonDown("Undo")) { undoManager.Undo(); }

        // Reset
        if (Input.GetButtonDown("Reset")) { undoManager.ResetToInitialState(); }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    void DeathChecker()
    {
        // プレイヤーの位置をビューポート座標に変換
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // 画面内チェック（0～1の範囲）
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            // 死亡箇所にエフェクトを出す
            deathEffectSpawner.SpawnEffect(transform.position);

            undoManager.Undo();
        }
    }
}
