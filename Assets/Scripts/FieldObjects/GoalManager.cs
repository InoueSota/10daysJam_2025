using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("GoalLine")]
    [SerializeField] private GameObject goalLinePrefab;

    [Header("Hit Parameter")]
    [SerializeField] private LayerMask groundLayer;

    Animator animator;

    // ÉSÅ[Éãê¸
    private GameObject goalLineObj;
    // ëºÉSÅ[Éã
    private GameObject otherGoalObj;

    // ÉtÉâÉOóﬁ
    [SerializeField] private bool isCreateLine;
    private bool isLineActive;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isCreateLine)
        {
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (fieldObject != gameObject &&
                    fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.GOAL &&
                    !fieldObject.GetComponent<GoalManager>().GetIsCreateLine() && fieldObject.activeSelf)
                {
                    if (Mathf.Abs(transform.position.x - fieldObject.transform.position.x) < 0.1f ||
                        Mathf.Abs(transform.position.y - fieldObject.transform.position.y) < 0.1f)
                    {
                        bool noBlock = true;

                        foreach (RaycastHit2D hit in Physics2D.RaycastAll(transform.position, (fieldObject.transform.position - transform.position).normalized, Vector3.Distance(transform.position, fieldObject.transform.position), groundLayer))
                        {
                            // TagÇ™FieldObjectÇ»ÇÁ
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

                            isCreateLine = true;
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
                // TagÇ™FieldObjectÇ»ÇÁ
                if (hit && hit.collider.gameObject.CompareTag("FieldObject") && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.GOAL)
                {
                    noBlock = false;
                    break;
                }
            }

            if (!noBlock || !otherGoalObj.activeSelf || (Mathf.Abs(transform.position.x - otherGoalObj.transform.position.x) > 0.1f && Mathf.Abs(transform.position.y - otherGoalObj.transform.position.y) > 0.1f))
            {
                Destroy(goalLineObj);
                SetIsCreateLine(false);
            }
        }

        animator.SetBool("on", isLineActive);
    }

    // Setter
    public void SetIsCreateLine(bool _isCreateLine)
    {
        isCreateLine = _isCreateLine;

        // Ç‡Çµê¸Çè¡Ç∑Ç»ÇÁå©ÇΩñ⁄Ç‡èCê≥Ç∑ÇÈ
        if (!_isCreateLine)
        {
            isLineActive = false;
            if (otherGoalObj) { otherGoalObj.GetComponent<GoalManager>().SetIsLineActive(false); }
        }
    }
    public void SetIsLineActive(bool _isLineActive) { isLineActive = _isLineActive; }

    // Getter
    public bool GetIsCreateLine() { return isCreateLine; }
    public bool GetIsLineActive() { return isLineActive; }
}
