using UnityEngine;

public class PlayerTear : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // 他コンポーネント
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    private UndoManager undoManager;

    // 分断座標
    private Vector2 divisionPosition;
    // 分断フラグ
    private bool isDivision;

    void Start()
    {
        controller = GetComponent<PlayerController>();

        // 他コンポーネントを取得
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
    }

    public void ManualUpdate()
    {
        // ロケット移動をしておらず、地面に接地している時に分断可能
        if (Input.GetButtonDown("Special") && !controller.GetIsRocketMoving() && controller.IsGrounded())
        {
            // 移動前に保存
            undoManager.SaveState();

            // まだ分断していなかったら、初分断フラグをtrueにする
            if (!isDivision) { isDivision = true; }
            // 分断座標は整数丸めをしたプレイヤー座標
            divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

            // 分断線の再表示
            if (!divisionLineObj.activeSelf)
            {
                divisionLineObj.transform.parent = null;
                divisionLineObj.SetActive(true);
            }
            // 分断線の位置を修正
            divisionLineObj.transform.position = new Vector3(divisionPosition.x, 6f, 0f);
            // 分断線に情報を与える
            divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL);

            // 分断処理
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                // 左側
                if (fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x)) { fieldObject.transform.parent = objectParent1; }
                // 右側
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

    // Setter
    public void SetDivisionPosition(Vector2 _divisionPosition) { divisionPosition = _divisionPosition; }
    public void SetIsDivision(bool _isDivision) { isDivision = _isDivision; }
}
