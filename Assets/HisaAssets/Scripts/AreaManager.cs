using UnityEngine;
using static StageCell;

public class AreaManager : MonoBehaviour
{

    public StageCell curSelectStage;
    [SerializeField] TargetFollow2DScript cameraFollow;

    [SerializeField] SpriteRenderer curVisualStageImage;//UI�̃X�e�[�W�����̉摜
    [SerializeField] AmpritudePosition imageAmpritude;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curSelectStage.GetSetActive = true;
        curSelectStage.SetSelectObj(false);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }


    //�ǂ̃G���A��I�Ԃ��m�肵�����ɃX�e�[�W�I�����n�߂���悤�ɂ���
    public void AreaSelect()
    {
        curSelectStage.GetSetActive = true;
        curSelectStage.SetSelectObj(true);
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
}
