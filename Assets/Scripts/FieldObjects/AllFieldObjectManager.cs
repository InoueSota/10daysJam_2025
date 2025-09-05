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
        FRAGILE,
        WARP,
        GLASS,
        NAIL
    }
    [SerializeField] private ObjectType objectType;

    // 座標群
    private Vector3 prePosition;
    private Vector3 currentPosition;

    [Header("Hit Layer")]
    [SerializeField] private LayerMask groundLayer;

    void Start()
    {
        currentPosition = transform.position;

        switch (objectType)
        {
            case ObjectType.NAIL:

                transform.parent = null;

                break;
        }
    }

    /// <summary>
    /// 動かされたあとの処理
    /// </summary>
    public void AfterHeadbutt(bool _horizontalHeadbutt, Vector3 _rocketVector, Transform _movingParent)
    {
        // 前フレーム座標の保存
        prePosition = transform.position;
        // 座標の更新
        currentPosition = transform.position + _rocketVector;

        // 分断線の取得
        GameObject divisionLine = GameObject.FindGameObjectWithTag("DivisionLine");

        // 移動すべきオブジェクトか判断する
        if (transform.parent == _movingParent)
        {
            switch (objectType)
            {
                case ObjectType.GROUND:
                case ObjectType.GOAL:
                case ObjectType.BLOCK:
                case ObjectType.SPONGE:
                case ObjectType.FRAGILE:
                case ObjectType.WARP:
                case ObjectType.GLASS:

                    // 横方向からの頭突き
                    if (_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                    {
                        if ((prePosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= currentPosition.x) ||
                            (currentPosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= prePosition.x))
                        {
                            if (objectType == ObjectType.GOAL) { GetComponent<GoalManager>().SetIsCreateLine(false); }

                            gameObject.SetActive(false);
                        }
                    }
                    // 縦方向からの頭突き
                    else if (!_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                    {
                        if ((prePosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= currentPosition.y) ||
                            (currentPosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= prePosition.y))
                        {
                            if (objectType == ObjectType.GOAL) { GetComponent<GoalManager>().SetIsCreateLine(false); }

                            gameObject.SetActive(false);
                        }
                    }

                    break;
            }

            // 釘ブロックに当たったら消滅する
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, _rocketVector, 0.4f, groundLayer);
            if (objectType != ObjectType.NAIL && hit.collider != null && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() == ObjectType.NAIL) { gameObject.SetActive(false); }
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
    public Vector3 GetPrePosition() { return prePosition; }
    public Vector3 GetCurrentPosition() { return currentPosition; }

    // Setter
    public void SetPrePosition(Vector3 _prePosition) { prePosition = _prePosition; }
    public void SetCurrentPosition(Vector3 _currentPosition) { currentPosition = _currentPosition; }
}
