using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 自コンポーネント
    private UndoManager undoManager;

    // 他コンポーネント
    private UIManager uiManager;
    private PlayerManager playerManager;

    // ゴール関係
    private bool isGoal;
    private enum GoalDirection { LEFT, RIGHT, UP, DOWN }
    private GoalDirection goalDirection;

    void Start()
    {
        // 自コンポーネントの取得
        undoManager = GetComponent<UndoManager>();

        // 他コンポーネントの取得
        uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    void Update()
    {
        // ゴール判定
        CheckGoal();
    }

    /// <summary>
    /// ゴール判定
    /// </summary>
    void CheckGoal()
    {
        if (!isGoal)
        {
            GameObject goalLine = GameObject.FindGameObjectWithTag("GoalLine");

            if (goalLine != null && goalLine.GetComponent<GoalLineManager>().IsGoal())
            {
                // プレイヤーからゴール方向を取得する
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (!player.GetComponent<PlayerController>().GetIsRocketMoving()) { goalDirection = GoalDirection.DOWN; }
                else { goalDirection = (GoalDirection)player.GetComponent<PlayerController>().GetDirection(); }

                // UIの更新
                uiManager.Goal((int)goalDirection);

                isGoal = true;
            }
        }
        else
        {
            if (Input.GetButtonDown("Reset")) { uiManager.Reset(); isGoal = false; }
        }
    }

    void LateUpdate()
    {
        // Undo
        if (!playerManager.GetIsDeath() && Input.GetButtonDown("Undo")) { undoManager.Undo(); }

        // Reset
        if (!playerManager.GetIsDeath() && Input.GetButtonDown("Reset")) { undoManager.ResetToInitialState(); }
    }
}
