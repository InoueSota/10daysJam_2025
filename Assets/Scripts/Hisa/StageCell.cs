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
    [Header("自分を基準に接続先のステージ")]
    [SerializeField] StageCell upConnectStage;
    [SerializeField] StageCell leftConnectStage;
    [SerializeField] StageCell downConnectStage;
    [SerializeField] StageCell rightConnectStage;

    public void SetColor(Color set) {
    
        GetComponent<SpriteRenderer>().color = set;
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

        if (direction == StageDirection.up && upConnectStage != null&& upConnectStage.gameObject.activeSelf)
        {
            return upConnectStage;
        }
        else if (direction == StageDirection.left && leftConnectStage != null && leftConnectStage.gameObject.activeSelf)
        {
            return leftConnectStage;
        }
        else if (direction == StageDirection.down && downConnectStage != null && downConnectStage.gameObject.activeSelf)
        {
            return downConnectStage;
        }
        else if (direction == StageDirection.right && rightConnectStage != null && rightConnectStage.gameObject.activeSelf)
        {
            return rightConnectStage;
        }

        return null;
    }



}
