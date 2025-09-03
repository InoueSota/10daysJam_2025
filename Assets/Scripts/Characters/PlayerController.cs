using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 自コンポーネント
    private PlayerTear tear;
    private Rigidbody2D rbody2D;

    // 他コンポーネント
    private UndoManager undoManager;

    [Header("Basic Parameter")]
    [SerializeField] private float halfSize;
    [Header("Rocket Parameter")]
    [SerializeField] private float rocketSpeed;
    private Vector3 rocketVector;
    private bool isRocketMoving;
    [Header("Ground Judgement")]
    [SerializeField] private LayerMask groundLayer;

    void Start()
    {
        // 自コンポーネントを取得
        tear = GetComponent<PlayerTear>();
        rbody2D = GetComponent<Rigidbody2D>();

        // 他コンポーネントを取得
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
    }

    public void ManualUpdate()
    {
        // 左右移動処理
        MoveUpdate();
        // 頭突き処理
        HeadbuttUpdate();

        // Undo
        if (Input.GetButtonDown("Undo")) { undoManager.Undo(); }
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    void MoveUpdate()
    {
        if (!isRocketMoving && IsGrounded() && Input.GetButtonDown("Jump") &&
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            // 移動前に保存
            undoManager.SaveState();

            // 移動ベクトルの初期化
            rocketVector = Vector2.zero;
            // 重力を無くす
            rbody2D.gravityScale = 0f;

            // 右方向に入力
            if (Input.GetAxisRaw("Horizontal") > 0.5f) { rocketVector.x = rocketSpeed; }
            // 左方向に入力
            else if (Input.GetAxisRaw("Horizontal") < -0.5f) { rocketVector.x = -rocketSpeed; }
            // 上方向に入力
            else if (Input.GetAxisRaw("Vertical") > 0.5f) { rocketVector.y = rocketSpeed; }
            // 下方向に入力
            else if (Input.GetAxisRaw("Vertical") < -0.5f) { rocketVector.y = -rocketSpeed; }

            // フラグの変更
            isRocketMoving = true;
        }
    }

    /// <summary>
    /// 頭突き処理
    /// </summary>
    void HeadbuttUpdate()
    {
        // 頭突き処理
        if (isRocketMoving && IsHitHead())
        {
            // 分断されている場合
            if (tear.GetIsDivision())
            {
                // 左側
                if (transform.position.x < tear.GetDivisionPosition().x) { tear.GetObjectTransform(1).transform.position = tear.GetObjectTransform(1).transform.position + rocketVector.normalized; }
                // 右側
                else { tear.GetObjectTransform(2).transform.position = tear.GetObjectTransform(2).transform.position + rocketVector.normalized; }
            }
            // 分断されていない場合
            else { tear.GetObjectTransform(1).transform.position = tear.GetObjectTransform(1).transform.position + rocketVector.normalized; }

            // 分断処理
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.GetComponent<AllFieldObjectManager>().AfterHeadbutt(IsHorizontalHeadbutt()); }

            // 移動を無くす
            rbody2D.linearVelocity = Vector2.zero;
            // 重力を受けるように戻す
            rbody2D.gravityScale = 1f;

            // フラグの変更
            isRocketMoving = false;
        }
    }
    bool IsHorizontalHeadbutt()
    {
        if (Mathf.Abs(rocketVector.x) > 0.1f) { return true; }
        return false;
    }

    void FixedUpdate()
    {
        // ロケット移動をしている時のみRigidbody2Dに反映
        if (isRocketMoving) { rbody2D.linearVelocity = rocketVector; }
    }

    // 接地判定群
    public bool IsGrounded()
    {
        // 現在位置を反映
        Vector3 currentLeftPosition = transform.position;
        Vector3 currentRightPosition = transform.position;

        // ずらす
        currentLeftPosition.x -= halfSize;
        currentRightPosition.x += halfSize;

        // Rayの生成
        RaycastHit2D leftHit = Physics2D.Raycast(currentLeftPosition, Vector2.down, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentRightPosition, Vector2.down, 0.45f, groundLayer);

        // RayがgroundLayerに衝突していたら接地判定はtrueを返す
        if (leftHit.collider != null || rightHit.collider != null)
        {
            return true;
        }
        return false;
    }
    public bool IsHitHead()
    {
        // 現在位置を反映
        Vector3 currentOnePosition = transform.position;
        Vector3 currentTwoPosition = transform.position;

        // ずらす
        if (Mathf.Abs(rocketVector.x) > 0f)
        {
            currentOnePosition.y -= halfSize;
            currentTwoPosition.y += halfSize;
        }
        else
        {
            currentOnePosition.x -= halfSize;
            currentTwoPosition.x += halfSize;
        }

        // Rayの生成
        RaycastHit2D leftHit = Physics2D.Raycast(currentOnePosition, rocketVector.normalized, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentTwoPosition, rocketVector.normalized, 0.45f, groundLayer);

        // RayがgroundLayerに衝突していたら接地判定はtrueを返す
        if (leftHit.collider != null || rightHit.collider != null)
        {
            return true;
        }
        return false;
    }

    // Getter
    public bool GetIsRocketMoving() { return isRocketMoving; }
}
