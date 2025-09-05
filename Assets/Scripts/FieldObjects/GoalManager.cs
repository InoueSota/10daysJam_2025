using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("GoalLine")]
    [SerializeField] private GameObject goalLinePrefab;

    [Header("Hit Parameter")]
    [SerializeField] private LayerMask groundLayer;

    // ゴール線
    private GameObject goalLineObj;
    // 他ゴール
    private GameObject otherGoalObj;

    // フラグ類
    private bool isLineActive;

    void Start()
    {

    }

    void Update()
    {
        if (!isLineActive)
        {
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (fieldObject != gameObject &&
                    fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.GOAL &&
                    !fieldObject.GetComponent<GoalManager>().GetIsLineActive())
                {
                    if (Mathf.Abs(transform.position.x - fieldObject.transform.position.x) < 0.1f ||
                        Mathf.Abs(transform.position.y - fieldObject.transform.position.y) < 0.1f)
                    {
                        bool noBlock = true;

                        foreach (RaycastHit2D hit in Physics2D.RaycastAll(transform.position, (fieldObject.transform.position - transform.position).normalized, Vector3.Distance(transform.position, fieldObject.transform.position), groundLayer))
                        {
                            // TagがFieldObjectなら
                            if (hit && hit.collider.gameObject.CompareTag("FieldObject") && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.GOAL)
                            {
                                noBlock = false;
                                break;
                            }
                        }

                        if (noBlock)
                        {
                            goalLineObj = Instantiate(goalLinePrefab);
                            goalLineObj.GetComponent<GoalLineManager>().Initialize(transform, fieldObject.transform, 1f);

                            otherGoalObj = fieldObject;

                            isLineActive = true;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            bool noBlock = true;

            foreach (RaycastHit2D hit in Physics2D.RaycastAll(transform.position, (otherGoalObj.transform.position - transform.position).normalized, Vector3.Distance(transform.position, otherGoalObj.transform.position), groundLayer))
            {
                // TagがFieldObjectなら
                if (hit && hit.collider.gameObject.CompareTag("FieldObject") && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.GOAL)
                {
                    noBlock = false;
                    break;
                }
            }

            if (!noBlock)
            {
                Destroy(goalLineObj);
                isLineActive = false;
            }

            if (Mathf.Abs(transform.position.x - otherGoalObj.transform.position.x) > 0.1f && Mathf.Abs(transform.position.y - otherGoalObj.transform.position.y) > 0.1f)
            {
                Destroy(goalLineObj);
                isLineActive = false;
            }
        }
    }

    // Setter
    public void SetIsLineActive(bool _isLineActive) { isLineActive = _isLineActive; }

    // Getter
    public bool GetIsLineActive() { return isLineActive; }
}
