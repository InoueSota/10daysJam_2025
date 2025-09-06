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
    float stageChangeCT=0.5f;//�X�e�[�W�J�ڂ��󂯕t����܂ł̎��ԁB�Z������ƁA�A�ł��Ȃ���V�[���J�ڂ������Ƀo�O��\����
    float curStageChangeCT;

    [SerializeField, Header("�X�e�[�W�A�G���A�I���̃A�j���[�V����")] Animator[] selectAnime;

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
        //�I�����Ă�Z��(�X�e�[�W)�̐؂�ւ�
        //���E
        if (inputDire.x != 0 && inputDire.y == 0)
        {
            //��
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
            //��
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

        //�V�[���J��
        if (Input.GetButtonDown("Select"))
        {
            Debug.Log("�Z���N�g");

            SceneManager.LoadScene(curSelectStage.GetStageName());
        }else if (Input.GetButtonDown("Back"))
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
            selectAnime[i].SetBool("StageSelect",false);
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
}
