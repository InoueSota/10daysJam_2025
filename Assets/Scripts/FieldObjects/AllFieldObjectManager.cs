using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{
    // 該当するObjectType
    public enum ObjectType
    {
        GROUND,
        GOAL,
        BLOCK,
        SPONGE,
        FRAGILE
    }
    [SerializeField] private ObjectType objectType;

    // 座標群
    private Vector3 prePosition;
    private Vector3 currentPosition;

    void Start()
    {
        currentPosition = transform.position;
    }

    /// <summary>
    /// 動かされたあとの処理
    /// </summary>
    public void AfterHeadbutt(bool _horizontalHeadbutt)
    {
        // 前フレーム座標の保存
        prePosition = currentPosition;
        // 座標の更新
        currentPosition = transform.position;

        // 分断線の取得
        GameObject divisionLine = GameObject.FindGameObjectWithTag("DivisionLine");

        switch (objectType)
        {
            case ObjectType.GROUND:



                break;
            case ObjectType.GOAL:
            case ObjectType.BLOCK:
            case ObjectType.SPONGE:

                // 横方向からの頭突き
                if (_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                {
                    if ((prePosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= currentPosition.x) ||
                        (currentPosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= prePosition.x))
                    {
                        gameObject.SetActive(false);
                    }
                }
                // 縦方向からの頭突き
                else if (!_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                {
                    if ((prePosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= currentPosition.y) ||
                        (currentPosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= prePosition.y))
                    {
                        gameObject.SetActive(false);
                    }
                }

                break;

            case ObjectType.FRAGILE:



                break;
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
}
