using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StageCell;

public class StageSelectManager : MonoBehaviour
{

    //public StageCell curSelectStage;
    public Vector2 inputDire = Vector2.zero;
    //[SerializeField] TargetFollow2DScript cameraFollow;
    [SerializeField] AreaManager[] areaManagers;
    [SerializeField] Transform areaPixelCameraTransform;

    float stageChangeCT = 0.5f;//ステージ遷移を受け付けるまでの時間。短すぎると、連打しながらシーン遷移した時にバグる可能性大
    float curStageChangeCT;
    float inputCoolTime;
    [SerializeField] SmoothDampRotate areaPixelCamera;

    [SerializeField, Header("ステージ、エリア選択のアニメーション")] Animator[] selectAnime;

    // [SerializeField] SpriteRenderer curVisualStageImage;
    //[SerializeField] AmpritudePosition imageAmpritude;

    public static int curSelectAreaIndex;
    int preSelectAreaIndex = -1;

    public static bool cellInit;
    public static int[] cellSelectTmp = new int[5];//それぞれのエリアで最後に選んだセルを保存する
    public static string lastStageName;//最後に遊んだステージ
    public static string lastAreaName;//最後に遊んだエリア
    public static bool areaSelect;


    bool debugActive;
    [SerializeField] GradientRampScroller gradientObj;

    [SerializeField] SceneTransition sceneTransitionPrefab;
    [SerializeField] SceneTransition gameStartTransitionPrefab;
    SceneTransition sceneTransitionObj;
    public bool isSceneChange;
    float sceneChangeCT;

    //エリア1は目的の値から＋1する、最後のindexは次のエリアが無いので数を大きくする
    public static int[] areaOpenClearNum = new int[5] { 7, 10, 8, 5, 500 };// { 7, 10, 8, 6, 500 };//

    public bool[] areaOpenFlag = new bool[5];

    [SerializeField] private PauseToggle pauseToggle;

    [SerializeField, Header("会話シーンがある場合は名前を入力")] string[] talkSceneName;
    float changeTalkSceneTime;//初期化の揺れ対策で、一瞬だけ待つ
    bool talkEnd;

    public int maxIndex;
    [SerializeField] GameObject[] arrowImage;
    [SerializeField] AudioPlay audioPlay;
    [SerializeField] GameObject abutton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        SaveData save = SaveSystem.Load(1) ?? new SaveData();//セーブを書き込む準備
        SaveUtil.SetCleared(save, "Area1", "Area1Stage1", ClearDirection.Right, true);//エリア1のステージ1を右方向にクリアした
        SaveSystem.Save(save, 1);//セーブ
        gradientObj.SetIndex(curSelectAreaIndex);//最後に選択したindexを保存できると良い

        if (!cellInit)
        {
            Debug.Log("セルの設定の初期化");
            for (int i = 0; i < cellSelectTmp.Length; i++)
            {
                cellSelectTmp[i] = 0;//ゲームをはじめたてはステージ1を保存する
            }
            cellInit = true;
        }

        string debugLogtext = "";

        for (int i = 0; i < cellSelectTmp.Length; i++)
        {
            debugLogtext += cellSelectTmp[i] + "\n";
        }
        Debug.Log(debugLogtext);


        for (int i = 0; i < areaManagers.Length; i++)
        {
            areaManagers[i].SetSelectSell(cellSelectTmp[i]);//セレクト画面に戻ったら保存したセルに移動させる
        }

        //エリアの開放状態
        for (int i = 1; i < areaOpenFlag.Length; i++)
        {
            string areaName = "Area" + (i + 1);

            if (PlayerPrefs.GetInt(areaName) >= 1)
            {
                areaOpenFlag[i] = true;
            }
        }
        areaOpenFlag[0] = true;//エリア1は最初から開放する

        //最後に遊んだステージが保存されてる時はそっちにする
        if (lastAreaName != "")
        {
            for (int i = 0; i < areaManagers.Length; i++)
            {
                string areaName = "Area" + (i + 1);

                if (areaName == lastAreaName)
                {
                    curSelectAreaIndex = i;//現在選択してるエリアの設定
                    for (int j = 0; j < areaManagers[i].GetStageCells().Count; j++)
                    {
                        if (areaManagers[i].GetCellStageName(j) == lastStageName)
                        {
                            areaManagers[i].SetSelectSell(j);//セレクト画面に戻ったら保存したセルに移動させる
                            break;
                        }

                    }
                    break;
                }
            }
        }

        //最初の一回だけ0で保存する
        if (!PlayerPrefs.HasKey("SelectTalk"))
        {
            PlayerPrefs.SetInt("SelectTalk", -1);
        }

        PlayerPrefs.Save();


        for (int i = 0; i < areaOpenFlag.Length; i++)
        {

            if (!areaOpenFlag[i])
            {

                break;
            }
            maxIndex = i;
        }

        foreach (var arrow in arrowImage)
        {
            arrow.SetActive(false);
        }

        for (int i = 0; i < (maxIndex) * 2; i++)
        {
            arrowImage[i].SetActive(true);
        }
        if (areaOpenFlag[4])
        {
            foreach (var arrow in arrowImage)
            {
                arrow.SetActive(true);
            }
        }

        //エリア解放された時にカメラ選択を自動でする
        if (PlayerPrefs.GetInt("Area2") == 1) {
            PlayerPrefs.SetInt("Area2", 2);
            curSelectAreaIndex = 1;
        }else if (PlayerPrefs.GetInt("Area3") == 1)
        {
            PlayerPrefs.SetInt("Area3", 2);
            curSelectAreaIndex = 2;
        }
        else if (PlayerPrefs.GetInt("Area4") == 1)
        {
            PlayerPrefs.SetInt("Area4", 2);
            curSelectAreaIndex = 3;
        }
        else if (PlayerPrefs.GetInt("Area5") == 1)
        {
            PlayerPrefs.SetInt("Area5", 2);
            curSelectAreaIndex = 4;
        }

        if (!areaSelect)
        {
            areaPixelCamera.StartRotation(72f * curSelectAreaIndex);
            gradientObj.SetIndex(curSelectAreaIndex);

            areaManagers[curSelectAreaIndex].AreaSelectAnime("ChangeArea");//次のアニメーションは再生する

            areaManagers[curSelectAreaIndex].AreaSelectAnime(true);
            areaManagers[curSelectAreaIndex].SetSelectActive(true);
            areaManagers[curSelectAreaIndex].ClearEffect();
            preSelectAreaIndex=curSelectAreaIndex;
        }

    }

    // Update is called once per frame
    void Update()
    {
        abutton.SetActive(areaSelect);
        
        StartTalk();
        ChangeScene();
        InputDire();
        Debug.Log(areaSelect);
        if (areaSelect)
        {
            AreaSelect();
            curStageChangeCT = 0;
        }
        else
        {
            StageSelect();
        }

        //ChangeCell();

#if UNITY_EDITOR
        DebugUpdate();
#endif

    }

    void InputDire()
    {
        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");
        if (inputCoolTime > 0)
        {
            inputCoolTime -= Time.deltaTime;


            //ボタン連打で動けるようにする
            if (inputDire.magnitude <= 0)
            {
                inputCoolTime = 0;

            }
            inputDire = Vector2.zero;
            return;
        }

        if (inputDire.magnitude > 0)
        {
            inputCoolTime = 0.3f;
        }

        //Debug.Log("InputDire" + inputDire);

    }

    void AreaSelect()
    {
        if (isSceneChange) { return; }

        if (inputDire.x > 0)
        {
            curSelectAreaIndex++;
            Debug.Log("UP");

        }
        else if (inputDire.x < 0)
        {
            curSelectAreaIndex--;
        }




        if (curSelectAreaIndex > maxIndex)
        {
            curSelectAreaIndex = 0;
        }

        if (curSelectAreaIndex < 0)
        {
            if (areaOpenFlag[4]) //最後のエリアが解放されたら一周できるようにする
            {
                curSelectAreaIndex = areaManagers.Length - 1;

            }
            else
            {
                curSelectAreaIndex = 0;
            }
        }




        //エリアを切り替えた時
        if (preSelectAreaIndex != curSelectAreaIndex)
        {
            if (preSelectAreaIndex >= 0 && preSelectAreaIndex < areaManagers.Length) areaManagers[preSelectAreaIndex].AreaSelectAnime("BackAreaSelect");//前のアニメーションはStop状態にして
            areaManagers[curSelectAreaIndex].AreaSelectAnime("ChangeArea");//次のアニメーションは再生する

            areaPixelCamera.StartRotation(72f * curSelectAreaIndex);

            preSelectAreaIndex = curSelectAreaIndex;
            areaManagers[curSelectAreaIndex].ClearEffect();
            gradientObj.SetIndex(curSelectAreaIndex);
        }

        if (Input.GetButtonDown("Select"))
        {
            areaSelect = false;
            areaManagers[curSelectAreaIndex].AreaSelectAnime(true);
            areaManagers[curSelectAreaIndex].SetSelectActive(true);
            audioPlay.SE1();
        }

    }

    void StageSelect()
    {
        if (isSceneChange) { return; }
        if (Input.GetButtonDown("Back")|| Input.GetButtonDown("Menu"))
        {
            areaSelect = true;
            Debug.Log("curSelectAreaIndex" + curSelectAreaIndex);
            areaManagers[curSelectAreaIndex].AreaSelectAnime(false);
            areaManagers[curSelectAreaIndex].SetSelectActive(false);

        }
        //セルの移動
        areaManagers[curSelectAreaIndex].ChangeCell(inputDire);
    }



    void ChangeScene()
    {
        if (isSceneChange) { return; }

        if (sceneChangeCT < 0.5f)
        {
            sceneChangeCT += Time.deltaTime;
            return;
        }

        //シーン遷移
        //ステージに入る時
        if (!areaSelect)
        {
            if (curStageChangeCT < 0.2f)
            {
                curStageChangeCT += Time.deltaTime;
                return;
            }
            if (Input.GetButtonDown("Select"))
            {
                // Debug.Log("セレクト");
                sceneTransitionObj = Instantiate(gameStartTransitionPrefab);
                sceneTransitionObj.StartTransition(areaManagers[curSelectAreaIndex].GetCellStageName());
                areaManagers[curSelectAreaIndex].AreaSelectAnime("GameStart");//次のアニメーションは再生する
                cellSelectTmp[curSelectAreaIndex] = areaManagers[curSelectAreaIndex].GetSelectSell();//最後に選んだステージを保存する
                isSceneChange = true;

                string debugLogtext = "";

                for (int i = 0; i < cellSelectTmp.Length; i++)
                {
                    debugLogtext += cellSelectTmp[i] + "\n";
                }
                Debug.Log(debugLogtext);
                audioPlay.SE1();
            }
        }
        //ステージ選択画面→タイトルへの遷移
        else if (Input.GetButtonDown("Menu"))
        {
            isSceneChange = true;
            sceneTransitionObj = Instantiate(sceneTransitionPrefab);
            sceneTransitionObj.StartTransition("TitleScene");
            Debug.Log("バック");
            audioPlay.SE1();
        }

    }

    [ContextMenu("エリアセレクト")]
    public void AreaSelectAnime()
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", false);
        }
    }

    [ContextMenu("ステージセレクト")]
    public void StageSelectAnime()
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", true);
        }
    }

    void DebugUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            debugActive = !debugActive;
            for (int i = 0; i < selectAnime.Length; i++)
            {
                selectAnime[i].SetBool("StageSelect", debugActive);
            }
        }
    }

    [ContextMenu("セーブ削除")]
    public static void SaveDelete()
    {
        //Initalize();
        SaveSystem.Delete(1);
        //SceneManager.LoadScene("StageSelectScene");

    }

    [ContextMenu("エリア開放セーブ削除")]
    public static void AreaSaveDelete()
    {
        PlayerPrefs.DeleteAll();
    }

    [ContextMenu("StageDateReset")]
    public void StageDateReset()
    {
        var g = GameBootstrap.Graph as EditableJsonStageGraph;
        if (g == null) { Debug.LogError("EditableJsonStageGraph が未初期化 or 型違いです"); return; }

        g.BeginCapture();

        g.SaveOverrideDelta();
        g.EndCapture();
    }

    public static void Initalize()
    {
        lastStageName = "Stage1→";
        lastAreaName = "Area1";
        for (int i = 0; i < cellSelectTmp.Length; i++)
        {
            cellSelectTmp[i] = 0;//ゲームをはじめたてはステージ1を保存する
        }
        curSelectAreaIndex = 0;
        AreaSaveDelete();
        SaveDelete();
    }
    void ChangeTalkScene(int index)
    {
        if (talkSceneName[index] != "" && !talkEnd)
        {
            if (changeTalkSceneTime > 0)
            {
                PlayerPrefs.SetInt("SelectTalk", index);
                PlayerPrefs.Save();
                Debug.Log("会話へ移行");
                pauseToggle.Pause(talkSceneName[index]);
                talkEnd = true;
            }
            changeTalkSceneTime += Time.deltaTime;

        }
    }

    void StartTalk()
    {
        if (PlayerPrefs.GetInt("SelectTalk") < 0)
        {
            ChangeTalkScene(0);
        }
        else if (areaOpenFlag[1] && PlayerPrefs.GetInt("SelectTalk") < 1)
        {
            ChangeTalkScene(1);
        }
        else if (areaOpenFlag[2] && PlayerPrefs.GetInt("SelectTalk") < 2)
        {
            ChangeTalkScene(2);
        }
        else if (areaOpenFlag[3] && PlayerPrefs.GetInt("SelectTalk") < 3)
        {
            ChangeTalkScene(3);
        }
    }
}
