using UnityEngine;
using static StageCell;

public class AreaManager : MonoBehaviour
{

    public StageCell curSelectStage;
    [SerializeField] TargetFollow2DScript cameraFollow;

    [SerializeField] SpriteRenderer curVisualStageImage;//UIのステージ内部の画像
    [SerializeField] AmpritudePosition imageAmpritude;
    [SerializeField, Header("ステージ、エリア選択のアニメーション")] Animator[] selectAnime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curSelectStage.GetSetActive = true;
        curSelectStage.SetSelectObj(false);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }


    //どのエリアを選ぶか確定した時にステージ選択を始められるようにする,エリア選択にもどる時にステージ選択の枠を消す
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
