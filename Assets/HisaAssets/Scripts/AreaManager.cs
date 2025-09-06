using UnityEngine;
using static StageCell;

public class AreaManager : MonoBehaviour
{

    public StageCell curSelectStage;
    [SerializeField] TargetFollow2DScript cameraFollow;

    [SerializeField] SpriteRenderer curVisualStageImage;//UIのステージ内部の画像
    [SerializeField] AmpritudePosition imageAmpritude;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curSelectStage.GetSetActive = true;
        curSelectStage.SetSelectObj(false);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }


    //どのエリアを選ぶか確定した時にステージ選択を始められるようにする
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


    //選択してるセル(ステージ)の切り替え

    public void ChangeCell(Vector2 inputDire)
    {
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
}
