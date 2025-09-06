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

    bool stageChangeFlag;
    float stageChangeCT = 0.5f;//ステージ遷移を受け付けるまでの時間。短すぎると、連打しながらシーン遷移した時にバグる可能性大
    float curStageChangeCT;
    public float inputCoolTime;
    [SerializeField] SmoothDampRotate areaPixelCamera;

    [SerializeField, Header("ステージ、エリア選択のアニメーション")] Animator[] selectAnime;

    // [SerializeField] SpriteRenderer curVisualStageImage;
    //[SerializeField] AmpritudePosition imageAmpritude;

    int curSelectAreaIndex;
    int preSelectAreaIndex;

    bool areaSelect;


    bool debugActive;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ////SetColor(Color.red);
        //curSelectStage.GetSetActive=true;
        //curSelectStage.SetSelectObj(true);
        //curVisualStageImage.sprite= curSelectStage.GetStageImage();
        areaSelect = true;

        areaManagers[0].AreaSelectAnime("ChangeArea");//エリア1のアニメーションは再生する
    }

    // Update is called once per frame
    void Update()
    {
        ChangeScene();
        InputDire();

        if (areaSelect)
        {
            AreaSelect();
            
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
        inputDire = Vector2.zero;

        if (inputCoolTime > 0)
        {
            inputCoolTime -= Time.deltaTime;
            return;
        }

        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");
        if (inputDire.magnitude > 0)
        {
            inputCoolTime = 0.2f;
        }
    }

    void AreaSelect()
    {
        if (inputDire.x > 0)
        {
            curSelectAreaIndex++;
            
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

        



        if (preSelectAreaIndex != curSelectAreaIndex) {
            areaManagers[preSelectAreaIndex].AreaSelectAnime("BackAreaSelect");//前のアニメーションはStop状態にして
            areaManagers[curSelectAreaIndex].AreaSelectAnime("ChangeArea");//次のアニメーションは再生する

            areaPixelCamera.StartRotation(90f * curSelectAreaIndex);

            preSelectAreaIndex = curSelectAreaIndex;

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
        if (Input.GetButtonDown("Back"))
        {
            areaSelect = true;
            areaManagers[curSelectAreaIndex].AreaSelectAnime(false);
            areaManagers[curSelectAreaIndex].SetSelectActive(false);
        }
        areaManagers[curSelectAreaIndex].ChangeCell(inputDire);
    }

    

    void ChangeScene()
    {
        if (stageChangeFlag) { return; }

        if (curStageChangeCT < 0.5f)
        {
            curStageChangeCT += Time.deltaTime;
            return;
        }

        //シーン遷移
        if (Input.GetButtonDown("Select"))
        {
            Debug.Log("セレクト");

            //SceneManager.LoadScene(curSelectStage.GetStageName());
        }
        else if (Input.GetButtonDown("Back"))
        {
            stageChangeFlag = true;
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
        
    }
}
