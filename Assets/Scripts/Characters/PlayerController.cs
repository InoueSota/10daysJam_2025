using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���R���|�[�l���g
    private Rigidbody2D rbody2D;

    [Header("��{�p�����[�^")]
    [SerializeField] private float halfSize;
    [Header("�ړ��p�����[�^")]
    [SerializeField] private float moveSpeed;
    private float xSpeed;
    [Header("�W�����v�p�����[�^")]
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [Header("���ł��p�����[�^")]
    [SerializeField] private float hoveringTime;
    private float hoveringTimer;
    private bool isHovering;
    private bool canHovering;

    void Start()
    {
        // ���R���|�[�l���g���擾
        rbody2D = GetComponent<Rigidbody2D>();
    }

    public void ManualUpdate()
    {
        // ���E�ړ�����
        MoveUpdate();
        // �W�����v����
        JumpUpdate();
        // �z�o�[����
        HoverUpdate();
    }

    /// <summary>
    /// ���E�ړ�����
    /// </summary>
    void MoveUpdate()
    {
        // �E�����ɓ���
        if (Input.GetAxisRaw("Horizontal") > 0f) { xSpeed = moveSpeed; }
        // �������ɓ���
        else if (Input.GetAxisRaw("Horizontal") < 0f) { xSpeed = -moveSpeed; }
        // ������
        else { xSpeed = 0f; }
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    void JumpUpdate()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded()) { rbody2D.linearVelocity = new Vector2(rbody2D.linearVelocity.x, jumpPower); }
    }

    /// <summary>
    /// �z�o�[����
    /// </summary>
    void HoverUpdate()
    {
        if (!isHovering && canHovering && IsHitHead())
        {
            rbody2D.linearVelocity = new(rbody2D.linearVelocity.x, 0f);
            hoveringTimer = hoveringTime;
            isHovering = true;
        }
        else if (isHovering)
        {
            rbody2D.gravityScale = 0f;

            hoveringTimer -= Time.deltaTime;
            if (hoveringTimer <= 0f)
            {
                rbody2D.gravityScale = 1f;
                canHovering = false;
                isHovering = false;
            }
        }
    }

    void FixedUpdate()
    {
        // ���݂̒l���擾
        Vector2 velocity = rbody2D.linearVelocity;
        // X�����̈ړ����x����
        velocity.x = xSpeed;
        // Rigidbody2D�ɔ��f
        rbody2D.linearVelocity = velocity;
    }

    public bool IsGrounded()
    {
        // ���݈ʒu�𔽉f
        Vector3 currentLeftPosition = transform.position;
        Vector3 currentRightPosition = transform.position;

        // ���炷
        currentLeftPosition.x -= halfSize;
        currentRightPosition.x += halfSize;

        // Ray�̐���
        RaycastHit2D leftHit = Physics2D.Raycast(currentLeftPosition, Vector2.down, 0.6f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentRightPosition, Vector2.down, 0.6f, groundLayer);

        // Ray��groundLayer�ɏՓ˂��Ă�����ڒn�����true��Ԃ�
        if (leftHit.collider != null || rightHit.collider != null)
        {
            canHovering = true;
            return true;
        }
        return false;
    }
    public bool IsHitHead()
    {
        // ���݈ʒu�𔽉f
        Vector3 currentLeftPosition = transform.position;
        Vector3 currentRightPosition = transform.position;

        // ���炷
        currentLeftPosition.x -= halfSize;
        currentRightPosition.x += halfSize;

        // Ray�̐���
        RaycastHit2D leftHit = Physics2D.Raycast(currentLeftPosition, Vector2.up, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentRightPosition, Vector2.up, 0.45f, groundLayer);

        // Ray��groundLayer�ɏՓ˂��Ă�����ڒn�����true��Ԃ�
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
