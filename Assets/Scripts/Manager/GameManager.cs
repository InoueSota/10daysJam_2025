using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 自コンポーネント
    private UndoManager undoManager;

    // 他コンポーネント
    private UIManager uiManager;
    private PlayerManager playerManager;

    // ゴール関係
    private bool isGoal;
    private enum GoalDirection { LEFT = 0, RIGHT = 2, UP = 3, DOWN = 1 }
    private GoalDirection goalDirection;

    //ステージ情報
    [SerializeField] string areaName;//どのエリアか(Area1,Area2)
    [SerializeField] string stageName;//どのステージか(Stage1,Stage2)

    string connectStage;

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
        SceneChange();
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
                if (!player.GetComponent<PlayerController>().GetIsRocketMoving()) { goalDirection = GoalDirection.UP; }
                else { goalDirection = (GoalDirection)player.GetComponent<PlayerController>().GetDirection(); }

                // UIの更新
                uiManager.Goal((int)goalDirection);
                Debug.Log("goalDirection" + goalDirection);

                isGoal = true;

                //隣接するステージの決定

                switch (goalDirection)
                {
                    case GoalDirection.LEFT:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Left, out var lStage))
                        {
                            Debug.Log($"次は {lStage.areaId} / {lStage.stageId}");
                            connectStage = lStage.stageId;
                        }
                        else
                        {
                            Debug.Log("隣接が未設定です");
                        }

                        break;
                    case GoalDirection.RIGHT:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Right, out var rStage))
                        {
                            Debug.Log($"次は {rStage.areaId} / {rStage.stageId}");
                            connectStage = rStage.stageId;
                        }
                        else
                        {
                            Debug.Log("隣接が未設定です");
                        }
                        break;
                    case GoalDirection.UP:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Up, out var uStage))
                        {
                            Debug.Log($"次は {uStage.areaId} / {uStage.stageId}");
                            connectStage = uStage.stageId;
                        }
                        else
                        {
                            Debug.Log("隣接が未設定です");
                        }
                        break;
                    case GoalDirection.DOWN:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Down, out var dStage))
                        {
                            Debug.Log($"次は {dStage.areaId} / {dStage.stageId}");
                            connectStage = dStage.stageId;
                        }
                        else
                        {
                            Debug.Log("隣接が未設定です");
                        }
                        break;
                    default:
                        break;


                }
                Debug.Log(connectStage);
            }
        }
        else
        {
            if (Input.GetButtonDown("Reset")) { uiManager.Reset(); isGoal = false; }
        }
    }

    void SceneChange()
    {
        if (!isGoal) { return; }
        if (Input.GetButtonDown("Select"))
        {


            if (connectStage != null)
            {
                SceneManager.LoadScene(connectStage);
            }
            else
            {
                SceneManager.LoadScene("StageSelectScene");

            }
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
