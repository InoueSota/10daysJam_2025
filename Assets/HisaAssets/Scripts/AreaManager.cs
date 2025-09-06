using UnityEngine;
using static StageCell;

public class AreaManager : MonoBehaviour
{

    public StageCell curSelectStage;
    [SerializeField] TargetFollow2DScript cameraFollow;

    [SerializeField] SpriteRenderer curVisualStageImage;//UI�̃X�e�[�W�����̉摜
    [SerializeField] AmpritudePosition imageAmpritude;
    [SerializeField, Header("�X�e�[�W�A�G���A�I���̃A�j���[�V����")] Animator[] selectAnime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curSelectStage.GetSetActive = true;
        curSelectStage.SetSelectObj(false);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }


    //�ǂ̃G���A��I�Ԃ��m�肵�����ɃX�e�[�W�I�����n�߂���悤�ɂ���,�G���A�I���ɂ��ǂ鎞�ɃX�e�[�W�I���̘g������
    public void SetSelectActive(bool active)
    {
        //curSelectStage.GetSetActive = active;
        curSelectStage.SetSelectObj(active);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }

    void SelectCell(StageDirection direction)
    {
        if (curSelectStage.GetStageCell(direction) != null)
        {
            curSelectStage.GetStageCell(direction).SetSelectObj(true);
            curSelectStage.SetSelectObj(false);
            curSelectStage = curSelectStage.GetStageCell(direction);
            cameraFollow.SetTarget(curSelectStage.transform);
            //curVisualStageImage.sprite = curSelectStage.GetStageImage();
            //imageAmpritude.EaseStart();
        }
    }


    //�I�����Ă�Z��(�X�e�[�W)�̐؂�ւ�

    public void ChangeCell(Vector2 inputDire)
    {
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

    public void AreaSelectAnime(bool flag)
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", flag);
        }
    }

    public void AreaSelectAnime(string triggerName)
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetTrigger(triggerName);
        }
    }

   
}
