using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerController : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerTear tear;
    private Rigidbody2D rbody2D;

    [Header("��{�p�����[�^")]
    [SerializeField] private float halfSize;
    [Header("�ړ��p�����[�^")]
    [SerializeField] private float moveSpeed;
    private float xSpeed;
    private Vector3 prePosition;
    private Vector3 currentPosition;
    [Header("�W�����v�p�����[�^")]
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    private bool isHitHead;
    private bool canJump;

    void Start()
    {
        // ���R���|�[�l���g���擾
        tear = GetComponent<PlayerTear>();
        rbody2D = GetComponent<Rigidbody2D>();

        currentPosition = transform.position;
    }

    public void ManualUpdate()
    {
        // ���E�ړ�����
        MoveUpdate();
        // �W�����v����
        JumpUpdate();
    }

    /// <summary>
    /// ���E�ړ�����
    /// </summary>
    void MoveUpdate()
    {
        // �E�����ɓ���
        if (Input.GetAxisRaw("Horizontal") > 0.5f) { xSpeed = moveSpeed; }
        // �������ɓ���
        else if (Input.GetAxisRaw("Horizontal") < -0.5f) { xSpeed = -moveSpeed; }
        // ������
        else { xSpeed = 0f; }
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    void JumpUpdate()
    {
        // ���˂��\�ɍĐݒ�
        if (isHitHead && IsGrounded()) { isHitHead = false; }

        // �O��t���[�����W�̕ۑ�
        prePosition = currentPosition;
        currentPosition = transform.position;
        // �W�����v�\������
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
        // �W�����v�J�n
        if (Input.GetButtonDown("Jump") && canJump) { rbody2D.linearVelocity = new Vector2(rbody2D.linearVelocity.x, jumpPower); canJump = false; }

        // ���˂�����
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
        // ���݂̒l���擾
        Vector2 velocity = rbody2D.linearVelocity;
        // X�����̈ړ����x����
        velocity.x = xSpeed;
        // Rigidbody2D�ɔ��f
        rbody2D.linearVelocity = velocity;
    }

    // �ڒn����Q
    public bool IsGrounded()
    {
        // ���݈ʒu�𔽉f
        Vector3 currentLeftPosition = transform.position;
        Vector3 currentRightPosition = transform.position;

        // ���炷
        currentLeftPosition.x -= halfSize;
        currentRightPosition.x += halfSize;

        // Ray�̐���
        RaycastHit2D leftHit = Physics2D.Raycast(currentLeftPosition, Vector2.down, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentRightPosition, Vector2.down, 0.45f, groundLayer);

        // Ray��groundLayer�ɏՓ˂��Ă�����ڒn�����true��Ԃ�
        if (leftHit.collider != null || rightHit.collider != null)
        {
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
