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
        NAIL,
        CRAB,
        SWITCH,
        LASER
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
        // 移動すべきオブジェクトか判断する
        if (transform.parent == _movingParent)
        {
            // 前フレーム座標の保存
            prePosition = transform.position;
            // 座標の更新
            currentPosition = transform.position + _rocketVector;

            // 分断線の取得
            GameObject divisionLine = GameObject.FindGameObjectWithTag("DivisionLine");

            // 可動オブジェクトのみの処理
            if (GetIsMoveableObject())
            {
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
            }

            // ObjectType別処理
            switch (objectType)
            {
                case ObjectType.CRAB:

                    if (_rocketVector == Vector3.up) { GetComponent<CrabManager>().SetThrowDirection(CrabManager.ThrowDirection.UP); }
                    else if (_rocketVector == Vector3.down) { GetComponent<CrabManager>().SetThrowDirection(CrabManager.ThrowDirection.DOWN); }
                    else if (_rocketVector == Vector3.left) { GetComponent<CrabManager>().SetThrowDirection(CrabManager.ThrowDirection.LEFT); }
                    else if (_rocketVector == Vector3.right) { GetComponent<CrabManager>().SetThrowDirection(CrabManager.ThrowDirection.RIGHT); }

                    break;
                case ObjectType.SWITCH:

                    // 可動オブジェクトのみの処理
                    if (GetIsMoveableObject() && GetComponent<SwitchManager>().GetStatus() == SwitchManager.Status.ON) { GetComponent<SwitchManager>().SetStatus(SwitchManager.Status.OFF); }
                    else if (GetIsMoveableObject() && GetComponent<SwitchManager>().GetStatus() == SwitchManager.Status.OFF) { GetComponent<SwitchManager>().SetStatus(SwitchManager.Status.ON); }

                    break;
            }

            // 釘ブロックに当たったら消滅する
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, _rocketVector, 0.4f, groundLayer);
            if (objectType != ObjectType.NAIL && hit.collider != null && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() == ObjectType.NAIL) { gameObject.SetActive(false); }
        }
    }
    bool GetIsMoveableObject()
    {
        if (objectType == ObjectType.NAIL)
        {
            return false;
        }
        return true;
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
    public Vector3 GetPrePosition() { return prePosition; }
    public Vector3 GetCurrentPosition() { return currentPosition; }

    // Setter
    public void SetPrePosition(Vector3 _prePosition) { prePosition = _prePosition; }
    public void SetCurrentPosition(Vector3 _currentPosition) { currentPosition = _currentPosition; }
}
