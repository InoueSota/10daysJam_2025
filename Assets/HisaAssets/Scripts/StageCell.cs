using UnityEngine;

public class StageCell : MonoBehaviour
{

    public enum StageDirection
    {
        up = 0,
        left = 1,
        down = 2,
        right = 3
    }


    // [SerializeField] StageCell[] connectStage = new StageCell[4];
    [SerializeField, Header("このセルで遷移するステージ")] string stageName;
    [Header("自分を基準に接続先のステージ")]
    [SerializeField] StageCell upConnectStage;
    [SerializeField] StageCell leftConnectStage;
    [SerializeField] StageCell downConnectStage;
    [SerializeField] StageCell rightConnectStage;

    [SerializeField] GameObject activeObj;
    [SerializeField] GameObject notActiveObj;

    [SerializeField] GameObject selectObj;

    public bool active;
    public bool GetSetActive
    {
        get { return active; }
        set
        {
            active = value;
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

    public void SetSelectObj(bool active) { selectObj.SetActive(active); }

    private void Awake()
    {
        activeObj.SetActive(false);
        selectObj.SetActive(false);
        notActiveObj.SetActive(true);
    }



    [ContextMenu("起動")]
    void SetActiveTrue()
    {
        GetSetActive = true;
    }

    public void SetGetConnectStage(StageDirection direction, StageCell targetStage)
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

        if (direction == StageDirection.up && upConnectStage != null && upConnectStage.GetSetActive)
        {
            return upConnectStage;
        }
        else if (direction == StageDirection.left && leftConnectStage != null && leftConnectStage.GetSetActive)
        {
            return leftConnectStage;
        }
        else if (direction == StageDirection.down && downConnectStage != null && downConnectStage.GetSetActive)
        {
            return downConnectStage;
        }
        else if (direction == StageDirection.right && rightConnectStage != null && rightConnectStage.GetSetActive)
        {
            return rightConnectStage;
        }

        return null;
    }

    public string GetStageName() { return stageName; }

}
