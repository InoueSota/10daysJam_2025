using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerCut cut;
    private Rigidbody2D rbody2D;
    private Animator animator;
    private PlayerSpriteScript playerSpriteScript;

    // ���R���|�[�l���g
    private UndoManager undoManager;
    private DivisionLineManager divisionLineManager;

    [Header("Basic Parameter")]
    [SerializeField] private float halfSize;
    [Header("Rocket Parameter")]
    [SerializeField] private float rocketSpeed;
    private Vector3 rocketVector;
    private bool isRocketMoving;
    private AllFieldObjectManager hitAllFieldObjectManager;
    [Header("Ground Judgement")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Map Move Parameter")]
    [SerializeField] private float mapMoveTime;

    // �t���O
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isStacking;
    private bool wasUndo;

    // ���[�v
    private GameObject warpObj;

    //Animation�n
    private int direction= 0;

    void Start()
    {
        // ���R���|�[�l���g���擾
        cut = GetComponent<PlayerCut>();
        rbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerSpriteScript = GetComponent<PlayerSpriteScript>();

        // ���R���|�[�l���g���擾
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
        divisionLineManager = cut.GetDivisionLineManager();
    }

    public void ManualUpdate()
    {
        if (!isStacking)
        {
            // ���E�ړ�����
            MoveUpdate();
            // ���˂�����
            HeadbuttUpdate();

            if (!wasUndo)
            {
                // �X�^�b�N���Ă��邩����
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 0.1f, groundLayer);
                if (hit.collider != null) { isStacking = true; }
            }
            else { wasUndo = false; }
        }

        animator.SetBool("isDash", isRocketMoving);
        playerSpriteScript.SetDirection(direction);

        // Undo
        if (Input.GetButtonDown("Undo")) { undoManager.Undo(); }
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    void MoveUpdate()
    {
        if (!isRocketMoving) { rbody2D.linearVelocity = new Vector2(0f, rbody2D.linearVelocity.y); }

        if (!isRocketMoving && !isMoving && IsGrounded() && Input.GetButtonDown("Jump") &&
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            // �ړ��O�ɕۑ�
            undoManager.SaveState();

            // �ړ��x�N�g���̏�����
            rocketVector = Vector2.zero;
            // �d�͂𖳂���
            rbody2D.gravityScale = 0f;

            // �E�����ɓ���
            if (Input.GetAxisRaw("Horizontal") > 0.5f) { rocketVector.x = rocketSpeed; direction = 0; }
            // �������ɓ���
            else if (Input.GetAxisRaw("Horizontal") < -0.5f) { rocketVector.x = -rocketSpeed; direction = 2; }
            // ������ɓ���
            else if (Input.GetAxisRaw("Vertical") > 0.5f) { rocketVector.y = rocketSpeed; direction = 1; }
            // �������ɓ���
            else if (Input.GetAxisRaw("Vertical") < -0.5f) { rocketVector.y = -rocketSpeed; direction = 3; }

            // ���[�v�ΏۃI�u�W�F�N�g�̏�������������
            warpObj = null;

            // �t���O�̕ύX
            isMoving = true;
            isRocketMoving = true;
        }
    }

    /// <summary>
    /// ���˂�����
    /// </summary>
    void HeadbuttUpdate()
    {
        // ���˂�����
        if (isRocketMoving && IsHeadbutt())
        {
            if (hitAllFieldObjectManager.GetObjectType() != AllFieldObjectManager.ObjectType.SPONGE)
            {
                Vector3 beforeHeadbuttPosition = transform.position;

                // �����Ă���e�I�u�W�F�N�g
                Transform movingParent = null;

                // ���f����Ă���ꍇ
                if (cut.GetIsDivision())
                {
                    // �㉺��
                    if (divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                    {
                        // ����
                        if (transform.position.x < cut.GetDivisionPosition().x)
                        {
                            cut.GetObjectTransform(1).transform.DOMove(cut.GetObjectTransform(1).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(1);
                        }
                        // �E��
                        else
                        {
                            cut.GetObjectTransform(2).transform.DOMove(cut.GetObjectTransform(2).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(2);
                        }
                    }
                    // ���E��
                    else if (divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                    {
                        // �㑤
                        if (transform.position.y > cut.GetDivisionPosition().y)
                        {
                            cut.GetObjectTransform(1).transform.DOMove(cut.GetObjectTransform(1).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(1);
                        }
                        // ����
                        else
                        {
                            cut.GetObjectTransform(2).transform.DOMove(cut.GetObjectTransform(2).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(2);
                        }
                    }
                }
                // ���f����Ă��Ȃ��ꍇ
                else { cut.GetObjectTransform(1).transform.DOMove(cut.GetObjectTransform(1).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove); }

                // ���f����
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.GetComponent<AllFieldObjectManager>().AfterHeadbutt(IsHorizontalHeadbutt(), rocketVector.normalized, movingParent); }

                // �v���C���[�����炵�ɂ���Ė������ꍇ�݂̂P�}�X�O�ɓ�����
                RaycastHit2D hit = Physics2D.Raycast(beforeHeadbuttPosition, -rocketVector.normalized, 0.8f, groundLayer);
                if (hit.collider != null) { transform.DOMove(beforeHeadbuttPosition + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine); }
            }

            // �ϐ��̏�����
            RocketInitialize();
        }
    }
    bool IsHorizontalHeadbutt()
    {
        if (Mathf.Abs(rocketVector.x) > 0.1f) { return true; }
        return false;
    }
    void FinishMapMove() { isMoving = false; }

    void FixedUpdate()
    {
        // ���P�b�g�ړ������Ă��鎞�̂�Rigidbody2D�ɔ��f
        if (isRocketMoving) { rbody2D.linearVelocity = rocketVector; }
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
    public bool IsHeadbutt()
    {
        // ���݈ʒu�𔽉f
        Vector3 currentOnePosition = transform.position;
        Vector3 currentTwoPosition = transform.position;

        // ���炷
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

        // Ray�̐���
        RaycastHit2D leftHit = Physics2D.Raycast(currentOnePosition, rocketVector.normalized, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentTwoPosition, rocketVector.normalized, 0.45f, groundLayer);

        // Ray��groundLayer�ɏՓ˂��Ă�����ڒn�����true��Ԃ�
        if (leftHit.collider != null || rightHit.collider != null)
        {
            GameObject hitObj = null;

            if (leftHit.collider != null) { hitAllFieldObjectManager = leftHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = leftHit.collider.gameObject; }
            if (rightHit.collider != null) { hitAllFieldObjectManager = rightHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = rightHit.collider.gameObject; }

            // ���������u���b�N�P�̂ɋN��������
            if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.FRAGILE)
            {
                hitAllFieldObjectManager.gameObject.SetActive(false);
            }
            else if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                if (hitObj != warpObj)
                {
                    hitObj.GetComponent<WarpManager>().DoWarp(transform, ref warpObj);
                }
                return false;
            }
            else if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.GLASS)
            {
                hitAllFieldObjectManager.gameObject.SetActive(false);
                return false;
            }

            // ���˂��ɐ��������烏�[�vObj������������
            warpObj = null;
            return true;
        }
        return false;
    }

    // Setter
    public void RocketInitialize()
    {
        // �ړ��𖳂���
        rbody2D.linearVelocity = Vector2.zero;
        // �d�͂��󂯂�悤�ɖ߂�
        rbody2D.gravityScale = 1f;

        // �t���O�̕ύX
        isRocketMoving = false;
    }
    public void FlagInitialize()
    {
        DOTween.KillAll();

        isMoving = false;
        isStacking = false;
        wasUndo = true;
    }

    // Getter
    public bool GetIsRocketMoving() { return isRocketMoving; }

    /// <summary>
    /// �����蔻��Q
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision) { OnTrigger2D(collision); }
    void OnTriggerStay2D(Collider2D collision) { OnTrigger2D(collision); }
    void OnTrigger2D(Collider2D collision)
    {
        if (collision.CompareTag("FieldObject"))
        {
            if (collision.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                if (collision.gameObject != warpObj)
                {
                    collision.GetComponent<WarpManager>().DoWarp(transform, ref warpObj);
                    rbody2D.linearVelocity = Vector2.zero;
                }
            }
        }
    }
}
