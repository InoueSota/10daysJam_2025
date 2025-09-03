using UnityEngine;

public class PlayerTear : MonoBehaviour
{
    // 他コンポーネント
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLinePrefab;
    private GameObject divisionLineObj;

    // 分断座標
    private Vector2 divisionPosition;
    // 分断フラグ
    private bool isDivision;

    public void Initialize()
    {

    }

    public void ManualUpdate()
    {
        // 十字ボタンの左右どちらかを押したら、左右どちらかを破り捨てる
        if (Input.GetButtonDown("Special"))
        {
            if (!isDivision) { isDivision = true; }
            divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

            if (divisionLineObj != null) { Destroy(divisionLineObj.gameObject); }
            divisionLineObj = Instantiate(divisionLinePrefab, new Vector3(divisionPosition.x, 6f, 0f), Quaternion.identity);

            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                // Parent
                if (fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x)) { fieldObject.transform.parent = objectParent1; }
                else { fieldObject.transform.parent = objectParent2; }
            }
        }
    }

    // Getter
    public bool GetIsDivision() { return isDivision; }
    public Vector2 GetDivisionPosition() { return divisionPosition; }
    public Transform GetObjectTransform(int _num)
    {
        if (_num == 1)
        {
            return objectParent1;
        }
        return objectParent2;
    }
}
