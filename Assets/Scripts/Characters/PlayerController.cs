using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 自コンポーネント
    private Rigidbody2D rbody2D;

    [Header("基本パラメータ")]
    [SerializeField] private float halfSize;
    [Header("移動パラメータ")]
    [SerializeField] private float moveSpeed;
    private float xSpeed;
    [Header("ジャンプパラメータ")]
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [Header("頭打ちパラメータ")]
    [SerializeField] private float hoveringTime;

    void Start()
    {
        // 自コンポーネントを取得
        rbody2D = GetComponent<Rigidbody2D>();
    }

    public void ManualUpdate()
    {
        // 左右移動処理
        MoveUpdate();
        // ジャンプ処理
        JumpUpdate();
    }

    /// <summary>
    /// 左右移動処理
    /// </summary>
    void MoveUpdate()
    {
        // 右方向に入力
        if (Input.GetAxisRaw("Horizontal") > 0f) { xSpeed = moveSpeed; }
        // 左方向に入力
        else if (Input.GetAxisRaw("Horizontal") < 0f) { xSpeed = -moveSpeed; }
        // 未入力
        else { xSpeed = 0f; }
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    void JumpUpdate()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded()) { rbody2D.linearVelocity = new Vector2(rbody2D.linearVelocity.x, jumpPower); }
    }

    void FixedUpdate()
    {
        // 現在の値を取得
        Vector2 velocity = rbody2D.linearVelocity;
        // X方向の移動速度を代入
        velocity.x = xSpeed;
        // Rigidbody2Dに反映
        rbody2D.linearVelocity = velocity;
    }

    public bool IsGrounded()
    {
        // 現在位置を反映
        Vector3 currentLeftPosition  = transform.position;
        Vector3 currentRightPosition = transform.position;

        // ずらす
        currentLeftPosition.x  -= halfSize;
        currentRightPosition.x += halfSize;

        // Rayの生成
        RaycastHit2D leftHit  = Physics2D.Raycast(currentLeftPosition,  Vector2.down, 0.6f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentRightPosition, Vector2.down, 0.6f, groundLayer);

        // RayがgroundLayerに衝突していたら接地判定はtrueを返す
        if (leftHit.collider != null ||  rightHit.collider != null)
        {
            return true;
        }
        return false;
    }

    // Setter
    public void SetDefault()
    {
        xSpeed = 0f;
    }
}
