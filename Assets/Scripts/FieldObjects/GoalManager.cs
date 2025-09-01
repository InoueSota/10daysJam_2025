using UnityEngine;

public class GoalManager : MonoBehaviour
{
    // 自コンポーネント
    private AllFieldObjectManager allFieldObjectManager;

    [Header("GoalLine")]
    [SerializeField] private GameObject goalLinePrefab;

    // フラグ類
    private bool isLineActive;

    void Start()
    {
        allFieldObjectManager = GetComponent<AllFieldObjectManager>();
    }

    void Update()
    {
        foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
        {
            if (fieldObject != gameObject &&
                fieldObject.GetComponent<AllFieldObjectManager>().GetStatus() == allFieldObjectManager.GetStatus() &&
                fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.GOAL &&
                !fieldObject.GetComponent<GoalManager>().GetIsLineActive())
            {
                if (Mathf.Abs(transform.position.x - fieldObject.transform.position.x) < 0.1f ||
                    Mathf.Abs(transform.position.y - fieldObject.transform.position.y) < 0.1f)
                {
                    GameObject goalLine = Instantiate(goalLinePrefab);
                    goalLine.GetComponent<GoalLineManager>().Initialize(transform, fieldObject.transform);

                    isLineActive = true;
                    break;
                }
            }
        }
    }

    // Getter
    public bool GetIsLineActive() { return isLineActive; }
}
