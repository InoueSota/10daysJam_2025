using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 自コンポーネント
    private UndoManager undoManager;
    private PauseToggle pauseToggle;

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

    [SerializeField, Header("会話シーンがある場合は名前を入力")] string talkSceneName;
    float changeTalkSceneTime;//初期化の揺れ対策で、一瞬だけ待つ
    bool talkEnd;

    //エフェクト
    [SerializeField] GameObject undoCanvas;
    [SerializeField] GameObject stackCanvas;
    private float stackTime;
    float stackInstatiateTime;

    [SerializeField] SceneTransition sceneTransitionPrefab;
    SceneTransition sceneTransitionObj;
    bool isSceneChange;
    float sceneChangeCT;

    void Start()
    {
        // 自コンポーネントの取得
        undoManager = GetComponent<UndoManager>();
        pauseToggle = GetComponent<PauseToggle>();

        // 他コンポーネントの取得
        uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();

        stageName = SceneManager.GetActiveScene().name;


    }

    void Update()
    {
        ChangeTalkScene();//会話シーンへの遷移
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
                PlayerController controller = player.GetComponent<PlayerController>();

                if (!controller.GetIsRocketMoving())
                {
                    if (controller.GetIsMoving())
                    {
                        // 右壁にぶつかってノックバック
                        if (controller.GetRocketVector() == Vector3.right)
                        {
                            goalDirection = GoalDirection.RIGHT;
                            controller.SetDirection(2);
                        }
                        else if (controller.GetRocketVector() == Vector3.left)
                        {
                            goalDirection = GoalDirection.LEFT;
                            controller.SetDirection(0);
                        }
                    }
                    else
                    {
                        goalDirection = GoalDirection.UP;
                        controller.SetDirection(3);
                    }
                }
                else { goalDirection = (GoalDirection)controller.GetDirection(); }

                // プレイヤーの動きを止める
                controller.SetStop();

                // UIの更新
                uiManager.Goal((int)goalDirection);
                Debug.Log("goalDirection" + goalDirection);

                isGoal = true;

                //隣接するステージの決定

                switch (goalDirection)
                {
                    case GoalDirection.LEFT:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Right, out var lStage))
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
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Left, out var rStage))
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
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Down, out var uStage))
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
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Up, out var dStage))
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

                CellActive(goalDirection);
                Debug.Log(connectStage);
            }
        }
    }

    void SceneChange()
    {
        if (isSceneChange) { return; }

        if (sceneChangeCT < 0.5f)
        {
            sceneChangeCT += Time.deltaTime;
            return;
        }
        if (isGoal && Input.GetButtonDown("Select"))
        {
            if (connectStage != null)
            {
               
                sceneTransitionObj = Instantiate(sceneTransitionPrefab);
                sceneTransitionObj.StartTransition(connectStage);
                isSceneChange = true;
            }
            else
            {
                sceneTransitionObj = Instantiate(sceneTransitionPrefab);
                sceneTransitionObj.StartTransition("HaikuScene");
                isSceneChange = true;
            }
        }
        //ポーズ画面を開く
        if (!isGoal && Input.GetButtonDown("Menu"))
        {
            pauseToggle.Pause("PauseScene");
            //sceneTransitionObj = Instantiate(sceneTransitionPrefab);
            //sceneTransitionObj.StartTransition("PauseScene");
            //isSceneChange = true;
            Debug.Log("バック");
        }
    }

    void CellActive(GoalDirection goalDirection)
    {
        SaveData save = SaveSystem.Load(1) ?? new SaveData();//セーブを書き込む準備

        switch (goalDirection)
        {
            case GoalDirection.LEFT:
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Right, true);//エリア1のステージ1を右方向にクリアした
                break;
            case GoalDirection.RIGHT:
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Left, true);//エリア1のステージ1を右方向にクリアした
                break;
            case GoalDirection.UP:
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Down, true);//エリア1のステージ1を右方向にクリアした
                break;
            case GoalDirection.DOWN:
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Up, true);//エリア1のステージ1を右方向にクリアした
                break;
            default:
                break;


        }

        SaveSystem.Save(save, 1);//セーブ
    }

    void LateUpdate()
    {
        if (isGoal) { return; }
        // Undo
        if (!playerManager.GetIsDeath() && !playerManager.GetIsStack() && Input.GetButtonDown("Undo"))
        {
            uiManager.Reset(); isGoal = false; undoManager.Undo();
            Instantiate(undoCanvas);
        }

        // Reset
        if (!playerManager.GetIsDeath() && !playerManager.GetIsStack() && Input.GetButtonDown("Reset")) { uiManager.Reset(); isGoal = false; undoManager.ResetToInitialState(); }


        //スタックの処理
        //少しの間スタックし続けてたら
        if (playerManager.GetIsStack())
        {
            stackInstatiateTime += Time.deltaTime;
            Debug.Log("スタック中");
        }
        else
        {
            stackInstatiateTime = 0;
        }
        //スタック→undoの処理をする
        if (stackInstatiateTime > 0.2f)
        {
            if (stackTime == 0)
            {
                Instantiate(stackCanvas);
            }
            stackTime += Time.deltaTime;

            if (stackTime > 1.3f)
            {

                uiManager.Reset(); isGoal = false; undoManager.Undo();
                Instantiate(undoCanvas);
                stackTime = 0;
                playerManager.SetStack(false);
            }
        }

    }
    void ChangeTalkScene()
    {
        if (talkSceneName != "" && !talkEnd)
        {
            if (changeTalkSceneTime > 0)
            {
                Debug.Log("会話へ移行");
                pauseToggle.Pause(talkSceneName);
                talkEnd = true;
            }
            changeTalkSceneTime += Time.deltaTime;

        }

    }

    public bool GetIsGoal() { return isGoal; }
}
