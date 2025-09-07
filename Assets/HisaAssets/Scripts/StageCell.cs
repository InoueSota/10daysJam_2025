using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour
{

    public enum StageDirection
    {
        up = 0,
        down = 1,
        left = 2,
        right = 3


    }


    // [SerializeField] StageCell[] connectStage = new StageCell[4];
    [SerializeField, Header("このセルで遷移するステージ")] string stageName;
    [SerializeField, Header("このセルのステージ画像")] Sprite stageImage;
    [Header("自分を基準に接続先のステージ")]
    [SerializeField] StageCell upConnectStage;
    [SerializeField] StageCell leftConnectStage;
    [SerializeField] StageCell downConnectStage;
    [SerializeField] StageCell rightConnectStage;

    [Header("コンポーネント")]
    [SerializeField] GameObject activeObj;
    [SerializeField] GameObject notActiveObj;
    [SerializeField] GameObject clearObj;
    [SerializeField] GameObject selectObj;
    [SerializeField] GameObject clearEffect;

    public bool activeFlag;

    public bool clear;

    //アクティブ(選択可能)にする
    public bool GetSetActive
    {
        get { return activeFlag; }
        set
        {
            activeFlag = value;
            if (value)
            {
                activeObj.SetActive(true);
                notActiveObj.SetActive(false);
            }
            else
            {
                activeObj.SetActive(false);
                notActiveObj.SetActive(true);
            }

        }
    }

    public bool GetSetClear
    {
        set { clear = value;
            if (clear) {
                clearObj.SetActive(true);
            }
        }
        get { return clear; }
    }

    public void SetSelectObj(bool active) { selectObj.SetActive(active); }

    public Sprite GetStageImage() { return stageImage; }

    private void Awake()
    {
        activeObj.SetActive(false);
        selectObj.SetActive(false);
        clearObj.SetActive(clear);
        notActiveObj.SetActive(true);

        GetSetActive = activeFlag;
    }

    //public void SetClearObjs()
    //{
    //    activeObj.SetActive(true);
    //    selectObj.SetActive(false);
    //    clearObj.SetActive(true);
    //    notActiveObj.SetActive(false);
    //    GetSetActive = true;
    //}



    [ContextMenu("起動")]
    void SetActiveTrue()
    {
        GetSetActive = true;
    }

    public void SetConnectStage(StageDirection direction, StageCell targetStage)
    {
        if (direction == StageDirection.up)
        {
            upConnectStage = targetStage;
        }
        else if (direction == StageDirection.left)
        {
            leftConnectStage = targetStage;
        }
        else if (direction == StageDirection.down)
        {
            downConnectStage = targetStage;
        }
        else if (direction == StageDirection.right)
        {
            rightConnectStage = targetStage;
        }
    }

    public StageCell GetStageCell(StageDirection direction)
    {
        //if (connectStage[(int)direction] != null && connectStage[(int)direction].gameObject.activeSelf)
        //{
        //    return connectStage[(int)direction];

        //}

        if (direction == StageDirection.up && upConnectStage != null )
        {
            return upConnectStage;
        }
        else if (direction == StageDirection.left && leftConnectStage != null )
        {
            return leftConnectStage;
        }
        else if (direction == StageDirection.down && downConnectStage != null )
        {
            return downConnectStage;
        }
        else if (direction == StageDirection.right && rightConnectStage != null )
        {
            return rightConnectStage;
        }

        return null;
    }

    public string GetStageName() { return stageName; }

    public void StartClearEffect()
    {
       
        clearEffect.SetActive(true);
    }

}
