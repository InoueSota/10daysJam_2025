using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerController : MonoBehaviour
{
    // 自コンポーネント
    private PlayerTear tear;
    private Rigidbody2D rbody2D;

    [Header("基本パラメータ")]
    [SerializeField] private float halfSize;
    [Header("移動パラメータ")]
    [SerializeField] private float moveSpeed;
    private float xSpeed;
    private Vector3 prePosition;
    private Vector3 currentPosition;
    [Header("ジャンプパラメータ")]
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    private bool isHitHead;
    private bool canJump;

    void Start()
    {
        // 自コンポーネントを取得
        tear = GetComponent<PlayerTear>();
        rbody2D = GetComponent<Rigidbody2D>();

        currentPosition = transform.position;
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
        if (Input.GetAxisRaw("Horizontal") > 0.5f) { xSpeed = moveSpeed; }
        // 左方向に入力
        else if (Input.GetAxisRaw("Horizontal") < -0.5f) { xSpeed = -moveSpeed; }
        // 未入力
        else { xSpeed = 0f; }
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    void JumpUpdate()
    {
        // 頭突き可能に再設定
        if (isHitHead && IsGrounded()) { isHitHead = false; }

        // 前回フレーム座標の保存
        prePosition = currentPosition;
        currentPosition = transform.position;
        // ジャンプ可能か判定
        if ((IsGrounded() && rbody2D.linearVelocity.y <= 0f) || 
            (tear.GetIsDivision() && prePosition.x < tear.GetDivisionPosition().x && tear.GetDivisionPosition().x <= currentPosition.x) || 
            (tear.GetIsDivision() && prePosition.x > tear.GetDivisionPosition().x && tear.GetDivisionPosition().x >= currentPosition.x)) 
        {
            if (isHitHead) { isHitHead = false; }
            canJump = true;
        }

<<<<<<< HEAD
        Debug.Log(IsGrounded());

=======
>>>>>>> 327de5e2b89bcd090b15a2338625ecd1ef5b0fcb
        // ジャンプ開始
        if (Input.GetButtonDown("Jump") && canJump) { rbody2D.linearVelocity = new Vector2(rbody2D.linearVelocity.x, jumpPower); canJump = false; }

        // 頭突き処理
        if (!isHitHead && IsHitHead())
        {
            if (tear.GetIsDivision())
            {
                if (transform.position.x < tear.GetDivisionPosition().x) { tear.GetObjectTransform(1).transform.position = tear.GetObjectTransform(1).transform.position + Vector3.up; }
                else { tear.GetObjectTransform(2).transform.position = tear.GetObjectTransform(2).transform.position + Vector3.up; }
            }
            else
            {
                tear.GetObjectTransform(1).transform.position = tear.GetObjectTransform(1).transform.position + Vector3.up;
            }
            rbody2D.linearVelocity = new Vector2(rbody2D.linearVelocity.x, 0f);
            isHitHead = true;
        }
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
        Vector3 currentLeftPosition = transform.position;
        Vector3 currentRightPosition = transform.position;

        // ずらす
        currentLeftPosition.x -= halfSize;
        currentRightPosition.x += halfSize;

        // Rayの生成
        RaycastHit2D leftHit = Physics2D.Raycast(currentLeftPosition, Vector2.up, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentRightPosition, Vector2.up, 0.45f, groundLayer);

        // RayがgroundLayerに衝突していたら接地判定はtrueを返す
        if (leftHit.collider != null || rightHit.collider != null)
        {
            return true;
        }
        return false;
    }

    // Setter
    public void SetDefault()
    {
        xSpeed = 0f;
        rbody2D.gravityScale = 0f;
    }
    public void SetBackToNormal()
    {
        rbody2D.gravityScale = 1f;
    }
}
