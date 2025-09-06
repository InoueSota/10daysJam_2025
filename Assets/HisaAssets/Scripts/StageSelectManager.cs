using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StageCell;

public class StageSelectManager : MonoBehaviour
{

    public StageCell curSelectStage;
    public Vector2 inputDire = Vector2.zero;
    [SerializeField] TargetFollow2DScript cameraFollow;

    bool stageChangeFlag;
    float stageChangeCT=0.5f;//ステージ遷移を受け付けるまでの時間。短すぎると、連打しながらシーン遷移した時にバグる可能性大
    float curStageChangeCT;

    [SerializeField, Header("ステージ、エリア選択のアニメーション")] Animator[] selectAnime;

    [SerializeField] SpriteRenderer curVisualStageImage;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //SetColor(Color.red);
        curSelectStage.GetSetActive=true;
        curSelectStage.SetSelectObj(true);
        curVisualStageImage.sprite= curSelectStage.GetStageImage();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeScene();
        InputDire();
        ChangeCell();


    }

    void InputDire()
    {
        inputDire = Vector2.zero;

        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");

    }
    void ChangeCell()
    {
        //選択してるセル(ステージ)の切り替え
        //左右
        if (inputDire.x != 0 && inputDire.y == 0)
        {
            //左
            if (inputDire.x < 0)
            {
                SelectCell(StageDirection.left);
            }
            else
            {
                SelectCell(StageDirection.right);
            }
        }
        else if (inputDire.x == 0 && inputDire.y != 0)
        {
            //左
            if (inputDire.y < 0)
            {
                SelectCell(StageDirection.down);
            }
            else
            {
                SelectCell(StageDirection.up);
            }
        }
    }

    void SelectCell(StageDirection direction)
    {
        if (curSelectStage.GetStageCell(direction) != null)
        {
            curSelectStage.GetStageCell(direction).SetSelectObj(true);
            curSelectStage.SetSelectObj(false);
            curSelectStage = curSelectStage.GetStageCell(direction);
            cameraFollow.SetTarget(curSelectStage.transform);
            curVisualStageImage.sprite = curSelectStage.GetStageImage();
        }
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

            SceneManager.LoadScene(curSelectStage.GetStageName());
        }else if (Input.GetButtonDown("Back"))
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
            selectAnime[i].SetBool("StageSelect",false);
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
}
