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
    float stageChangeCT = 0.5f;//�X�e�[�W�J�ڂ��󂯕t����܂ł̎��ԁB�Z������ƁA�A�ł��Ȃ���V�[���J�ڂ������Ƀo�O��\����
    float curStageChangeCT;
    public float inputCoolTime;

    [SerializeField, Header("�X�e�[�W�A�G���A�I���̃A�j���[�V����")] Animator[] selectAnime;

    // [SerializeField] SpriteRenderer curVisualStageImage;
    //[SerializeField] AmpritudePosition imageAmpritude;

    int curSelectAreaIndex;

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
            curSelectAreaIndex++;
        }

        if (curSelectAreaIndex >= areaManagers.Length)
        {
            curSelectAreaIndex = 0;
        }
        else if (curSelectAreaIndex < 0)
        {
            curSelectAreaIndex = areaManagers.Length - 1;
        }


        areaPixelCameraTransform.rotation = Quaternion.Euler(0f, 90f * curSelectAreaIndex, 0f);

    }

    void StageSelect()
    {

    }

    //void ChangeCell()
    //{
    //    //�I�����Ă�Z��(�X�e�[�W)�̐؂�ւ�
    //    //���E
    //    if (inputDire.x != 0 && inputDire.y == 0)
    //    {
    //        //��
    //        if (inputDire.x < 0)
    //        {
    //            SelectCell(StageDirection.left);
    //        }
    //        else
    //        {
    //            SelectCell(StageDirection.right);
    //        }
    //    }
    //    else if (inputDire.x == 0 && inputDire.y != 0)
    //    {
    //        //��
    //        if (inputDire.y < 0)
    //        {
    //            SelectCell(StageDirection.down);
    //        }
    //        else
    //        {
    //            SelectCell(StageDirection.up);
    //        }
    //    }
    //}

    //void SelectCell(StageDirection direction)
    //{
    //    if (curSelectStage.GetStageCell(direction) != null)
    //    {
    //        curSelectStage.GetStageCell(direction).SetSelectObj(true);
    //        curSelectStage.SetSelectObj(false);
    //        curSelectStage = curSelectStage.GetStageCell(direction);
    //        cameraFollow.SetTarget(curSelectStage.transform);
    //        curVisualStageImage.sprite = curSelectStage.GetStageImage();
    //        imageAmpritude.EaseStart();
    //    }
    //}

    void ChangeScene()
    {
        if (stageChangeFlag) { return; }

        if (curStageChangeCT < 0.5f)
        {
            curStageChangeCT += Time.deltaTime;
            return;
        }

        //�V�[���J��
        if (Input.GetButtonDown("Select"))
        {
            Debug.Log("�Z���N�g");

            //SceneManager.LoadScene(curSelectStage.GetStageName());
        }
        else if (Input.GetButtonDown("Back"))
        {
            stageChangeFlag = true;
            Debug.Log("�o�b�N");
        }

    }

    [ContextMenu("�G���A�Z���N�g")]
    public void AreaSelectAnime()
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", false);
        }
    }

    [ContextMenu("�X�e�[�W�Z���N�g")]
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
}
