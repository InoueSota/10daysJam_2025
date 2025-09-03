using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("GoalLine")]
    [SerializeField] private GameObject goalLinePrefab;

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
                        goalLineObj = Instantiate(goalLinePrefab);
                        goalLineObj.GetComponent<GoalLineManager>().Initialize(transform, fieldObject.transform, 1f);

                        otherGoalObj = fieldObject;

                        isLineActive = true;
                        break;
                    }
                }
            }
        }
        else
        {
            if (Mathf.Abs(transform.position.x - otherGoalObj.transform.position.x) > 0.1f && Mathf.Abs(transform.position.y - otherGoalObj.transform.position.y) > 0.1f)
            {
                Destroy(goalLineObj);
                isLineActive = false;
            }
        }
    }

    // Getter
    public bool GetIsLineActive() { return isLineActive; }
}
