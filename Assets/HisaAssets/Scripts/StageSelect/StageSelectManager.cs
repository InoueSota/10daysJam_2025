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
    public float curStageChangeCT;
    public float inputCoolTime;
    [SerializeField] SmoothDampRotate areaPixelCamera;

    [SerializeField, Header("ステージ、エリア選択のアニメーション")] Animator[] selectAnime;

    // [SerializeField] SpriteRenderer curVisualStageImage;
    //[SerializeField] AmpritudePosition imageAmpritude;

    public static int curSelectAreaIndex;
    int preSelectAreaIndex = -1;

    public static bool cellInit;
    public static int[] cellSelectTmp = new int[5];//それぞれのエリアで最後に選んだセルを保存する

    bool areaSelect;


    bool debugActive;
    [SerializeField] GradientRampScroller gradientObj;

    [SerializeField] SceneTransition sceneTransitionPrefab;
    [SerializeField] SceneTransition gameStartTransitionPrefab;
    SceneTransition sceneTransitionObj;
    public bool isSceneChange;
    float sceneChangeCT;

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
        areaSelect = true;
        for (int i = 0; i < areaManagers.Length; i++)
        {
            areaManagers[i].SetSelectSell(cellSelectTmp[i]);//セレクト画面に戻ったら保存したセルに移動させる
        }
    }

    // Update is called once per frame
    void Update()
    {
        ChangeScene();
        InputDire();

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

        if (curSelectAreaIndex >= areaManagers.Length)
        {
            curSelectAreaIndex = 0;
        }
        else if (curSelectAreaIndex < 0)
        {
            curSelectAreaIndex = areaManagers.Length - 1;
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
        }

    }

    void StageSelect()
    {
        if (isSceneChange) { return; }
        if (Input.GetButtonDown("Back"))
        {
            areaSelect = true;
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
                Debug.Log("セレクト");
                sceneTransitionObj = Instantiate(gameStartTransitionPrefab);
                sceneTransitionObj.StartTransition(areaManagers[curSelectAreaIndex].GetCellStageName());
                areaManagers[curSelectAreaIndex].AreaSelectAnime("GameStart");//次のアニメーションは再生する
                cellSelectTmp[curSelectAreaIndex] = areaManagers[curSelectAreaIndex].GetSelectSell();//最後に選んだステージを保存する
                isSceneChange = true;

                string debugLogtext="";

                for (int i = 0; i < cellSelectTmp.Length; i++)
                {
                    debugLogtext += cellSelectTmp[i] + "\n";
                }
                Debug.Log(debugLogtext);
            }
        }
        //ステージ選択画面→タイトルへの遷移
        else if (Input.GetButtonDown("Back"))
        {
            isSceneChange = true;
            sceneTransitionObj = Instantiate(sceneTransitionPrefab);
            sceneTransitionObj.StartTransition("TitleScene");
            Debug.Log("バック");
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
    void SaveDelete()
    {
        SaveSystem.Delete(1);
        SceneManager.LoadScene("StageSelectScene");

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
}
