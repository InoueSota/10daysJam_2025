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
    private enum GoalDirection { LEFT = 0, RIGHT = 2, UP = 3, DOWN = 1, NONE = -1 }//俳句を出さない方向を指定するためにNoneを追加した
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
    public float stackTime;
    public float stackInstatiateTime;

    [SerializeField] SceneTransition sceneTransitionPrefab;
    SceneTransition sceneTransitionObj;
    bool isSceneChange;
    float sceneChangeCT;

    [SerializeField, Header("俳句を出さない方向")] GoalDirection notHaikuDire = GoalDirection.NONE;
    bool notHaikuFlag;//俳句を出さない方向にクリアした時に特別な処理をする
    int curAreaIndex;
    bool newAreaOpen;

    int goalTextType;//0は接続先なし、1はステージ開放、2は開放済み、3はステージの端(俳句なし)

    int SelectIndex;

    void Start()
    {
        // 自コンポーネントの取得
        undoManager = GetComponent<UndoManager>();
        pauseToggle = GetComponent<PauseToggle>();

        // 他コンポーネントの取得
        uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();

        stageName = SceneManager.GetActiveScene().name;

        //セレクト画面に戻った時に最後に遊んだステージに戻るようにする
        StageSelectManager.lastAreaName=areaName;
        StageSelectManager.lastStageName=stageName;

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
                        else if (controller.GetRocketVector() == Vector3.up)
                        {
                            goalDirection = GoalDirection.DOWN;
                            controller.SetDirection(1);
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


                //俳句を出さない方向でクリアした場合

                GoalDirection goalDire = GoalDirection.NONE;
                //ゴールの方向を見た目と一致させる
                switch (goalDirection)
                {
                    case GoalDirection.LEFT:
                        goalDire = GoalDirection.RIGHT;
                        break;
                    case GoalDirection.RIGHT:
                        goalDire = GoalDirection.LEFT;
                        break;
                    case GoalDirection.UP:
                        goalDire = GoalDirection.DOWN;
                        break;
                    case GoalDirection.DOWN:
                        goalDire = GoalDirection.UP;
                        break;
                    case GoalDirection.NONE:
                        break;
                    default:
                        break;
                }

                uiManager.ClearCanvasActive();
                if (notHaikuDire == goalDire)
                {
                    notHaikuFlag = true;
                    Debug.Log("俳句なし");
                    uiManager.SetActiveFalseIndex();
                }

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

                //エリア開放する処理
                AreaOpen();
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
        if (isGoal && Input.GetButtonDown("Select") && uiManager.GetInputDelay())
        {
            if (uiManager.GetCurSelectIndex() == 0)//次のステージ
            {
                if (connectStage != null)
                {

                    sceneTransitionObj = Instantiate(sceneTransitionPrefab);
                    sceneTransitionObj.StartTransition(connectStage);
                    isSceneChange = true;

                }
                else
                {
                    //接続先なしの時俳句へ
                    if (!notHaikuFlag)
                    {
                        sceneTransitionObj = Instantiate(sceneTransitionPrefab);
                        sceneTransitionObj.StartTransition("HaikuScene");
                        isSceneChange = true;
                    }
                }

            }
            else if (uiManager.GetCurSelectIndex() == 1)//やりなおす
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                sceneTransitionObj = Instantiate(sceneTransitionPrefab);
                sceneTransitionObj.StartTransition(currentSceneName);
                isSceneChange = true;
            }
            else if (uiManager.GetCurSelectIndex() == 2)//セレクト画面
            {
                sceneTransitionObj = Instantiate(sceneTransitionPrefab);
                sceneTransitionObj.StartTransition("StageSelectScene");
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
        int type = 0;
        switch (goalDirection)
        {
            case GoalDirection.LEFT:
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Right, true);//エリア1のステージ1を右方向にクリアした
                break;
            case GoalDirection.RIGHT:
                // type = SaveUtil.GetNeighborExistAndClearState(save, areaName, stageName, ClearDirection.Left);
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Left, true);//エリア1のステージ1を右方向にクリアした
                break;
            case GoalDirection.UP:
                //type = SaveUtil.GetNeighborExistAndClearState(save, areaName, stageName, ClearDirection.Down);
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Down, true);//エリア1のステージ1を右方向にクリアした
                break;
            case GoalDirection.DOWN:
                //type = SaveUtil.GetNeighborExistAndClearState(save, areaName, stageName, ClearDirection.Up);
                SaveUtil.SetCleared(save, areaName, stageName, ClearDirection.Up, true);//エリア1のステージ1を右方向にクリアした
                break;
            default:
                break;


        }


        SaveSystem.Save(save, 1);//セーブ
                                 //接続先にセーブがあるか確認
        type = SaveUtil.GetStageClearState(save, areaName, connectStage);

        if (type == 0)//接続先がなし
        {
            if (!notHaikuFlag)//俳句がある時は表示を変える
            {
                type = 3;
            }
        }
        //度でもクリアしてない時かつ接続先はあるがセーブが無い時
        if (type != 2 && connectStage != null && connectStage != "")
        {
            type = 1;//開放しました！にする
        }

        ////接続先がある時
        //if (connectStage != null && connectStage != "")
        //{
        //    if (type == 0)//移動先にデータが無いとき
        //    {
        //        type = 1;//俳句へ行くためにtypeを1にする
        //    }

        //}
        ////接続先が無い時
        //else
        //{
        //    type = 3;
        //}
        // UIの更新
        uiManager.Goal((int)goalDirection, type);
        Debug.Log("goalDirection" + goalDirection);

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
            Debug.Log("スタック解除");
            stackInstatiateTime = 0;
        }
        //スタック→undoの処理をする
        if (stackInstatiateTime > 0.4f)
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

    public string GetAreaName()
    {
        return areaName;
    }

    //クリア時にエリアのクリア数で次のエリアを開放する
    void AreaOpen()
    {
        if (areaName == "Area5") { return; }//エリア5のときは次が無いので早期リターン
        SaveData saveData = SaveSystem.Load(1);

        int areaClearNum = SaveUtil.GetClearedStageCount(saveData, areaName);

        string nextArea = "";
        curAreaIndex = 0;
        if (areaName == "Area1")
        {
            curAreaIndex = 1;
            nextArea = "Area2";
        }
        else if (areaName == "Area2")
        {
            curAreaIndex = 2;
            nextArea = "Area3";
        }
        else if (areaName == "Area3")
        {
            curAreaIndex = 3;
            nextArea = "Area4";
        }
        else if (areaName == "Area4")
        {
            curAreaIndex = 4;
            nextArea = "Area5";
        }

        //このエリアのクリア数が規定の数超えたら次のエリアを開放する
        if (areaClearNum >= StageSelectManager.areaOpenClearNum[curAreaIndex])
        {
            if (PlayerPrefs.GetInt(nextArea) != 1)//まだエリアを開放してない時
            {
                PlayerPrefs.SetInt(nextArea, 1);//1を開放状態として扱う
                PlayerPrefs.Save(); // 明示的に保存
                uiManager.AreaOpen();//テキストを表示
                uiManager.SetAreaOpenText(curAreaIndex + 1);
                //Debug.Log("エリアかいほううううううううううううううううううううううううう");
                //エリア開放フラグをtrueにしてキャンバスの内容を変える
            }
        }
    }

    public int GetCurAreIndex() { return curAreaIndex; }
    public int GetStageAreIndex() { return curAreaIndex; }
}
