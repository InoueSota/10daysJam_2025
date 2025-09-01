using UnityEngine;

public class GoalManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private AllFieldObjectManager allFieldObjectManager;

    [Header("GoalLine")]
    [SerializeField] private GameObject goalLinePrefab;

    // ���S�[��
    private GameObject otherGoalObj;
    // �S�[����
    private GameObject goalLineObj;

    // �t���O��
    private bool isLineActive;

    void Start()
    {
        allFieldObjectManager = GetComponent<AllFieldObjectManager>();
    }

    void Update()
    {
        if (!isLineActive)
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
                        goalLineObj = Instantiate(goalLinePrefab);

                        switch (allFieldObjectManager.GetStatus())
                        {
                            case AllFieldObjectManager.Status.FIRST:
                                goalLineObj.GetComponent<GoalLineManager>().Initialize(transform, fieldObject.transform, 1f);
                                break;
                            case AllFieldObjectManager.Status.SECOND:
                                goalLineObj.GetComponent<GoalLineManager>().Initialize(transform, fieldObject.transform, 0.2f);
                                break;
                        }

                        // ���S�[����ݒ�
                        otherGoalObj = fieldObject;

                        isLineActive = true;
                        break;
                    }
                }
            }
        }
        else
        {
            if (otherGoalObj.GetComponent<AllFieldObjectManager>().GetStatus() != allFieldObjectManager.GetStatus())
            {
                Destroy(goalLineObj);
                otherGoalObj = null;
                isLineActive = false;
            }
        }
    }

    // Getter
    public bool GetIsLineActive() { return isLineActive; }
}
