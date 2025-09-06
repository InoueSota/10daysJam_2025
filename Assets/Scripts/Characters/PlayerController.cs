using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerCut cut;
    private Rigidbody2D rbody2D;
    private BoxCollider2D boxCollider2D;
    private Animator animator;

    // ���R���|�[�l���g
    private CameraManager cameraManager;
    private UndoManager undoManager;
    private DivisionLineManager divisionLineManager;
    [SerializeField] private PlayerAnimationScript animationScript;

    [Header("��{�I�ȃp�����[�^")]
    [SerializeField] private float halfSize;

    [Header("���P�b�g�p�����[�^")]
    [SerializeField] private float toMaxSpeedTime;
    [SerializeField] private float rocketMaxSpeed;
    private float rocketSpeed;
    private Vector3 rocketVector;
    private bool isRocketMoving;
    private AllFieldObjectManager hitAllFieldObjectManager;

    // ���[�v�p�����[�^
    private Vector3 warpPosition;
    private bool isWarping;

    [Header("�����蔻����s�����C���[")]
    [SerializeField] private LayerMask groundLayer;

    [Header("�X�e�[�W�I�u�W�F�N�g���������x")]
    [SerializeField] private float mapMoveTime;

    // �t���O
    private bool isMoving;
    private bool isStacking;
    private bool definitelyStack;

    // ���[�v
    private GameObject warpObj;

    // Animation�n
    private int direction = 0;

    void Start()
    {
        // ���R���|�[�l���g���擾
        cut = GetComponent<PlayerCut>();
        rbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        // ���R���|�[�l���g���擾
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
        divisionLineManager = cut.GetDivisionLineManager();
    }

    public void ManualUpdate()
    {
        if (!isStacking && !definitelyStack)
        {
            // ���E�ړ�����
            MoveUpdate();
            // ���˂�����
            HeadbuttUpdate();
        }

        // �m��X�^�b�N����Ȃ��Ƃ��ɔ�������
        if (!definitelyStack)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 0.1f, groundLayer);
            if (hit.collider != null && hit.transform.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.WARP) { isStacking = true; }
            else { isStacking = false; }
        }
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    void MoveUpdate()
    {
        if (!isRocketMoving) { rbody2D.linearVelocity = new Vector2(0f, rbody2D.linearVelocity.y); }

        if (!isRocketMoving && !isMoving && IsGrounded() && !cut.GetIsActive() && Input.GetButtonDown("Jump") &&
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            // ���W���ۂ߂�
            transform.position = new Vector3(SnapToNearestHalf(transform.position.x), Mathf.Round(transform.position.y));
            // �ړ��O�ɕۑ�
            undoManager.SaveState();

            // �ړ��x�N�g���̏�����
            rocketVector = Vector2.zero;
            // �d�͂𖳂���
            rbody2D.gravityScale = 0f;

            // �E�����ɓ���
            if (Input.GetAxisRaw("Horizontal") > 0.5f) { rocketVector = Vector3.right; direction = 0; }
            // �������ɓ���
            else if (Input.GetAxisRaw("Horizontal") < -0.5f) { rocketVector = Vector3.left; direction = 2; }
            // ������ɓ���
            else if (Input.GetAxisRaw("Vertical") > 0.5f) { rocketVector = Vector3.up; direction = 1; }
            // �������ɓ���
            else if (Input.GetAxisRaw("Vertical") < -0.5f) { rocketVector = Vector3.down; direction = 3; }

            // ���P�b�g�̈ړ����x��ς���
            DOVirtual.Float(0f, rocketMaxSpeed, toMaxSpeedTime, value => { rocketSpeed = value; }).SetEase(Ease.Linear);

            // ���[�v�ΏۃI�u�W�F�N�g�̏�������������
            warpObj = null;

            // �t���O�̕ύX
            isMoving = true;
            isRocketMoving = true;

            //�A�j���[�V�����g���K�[
            animationScript.StartRocket();
        }
    }

    /// <summary>
    /// ���˂�����
    /// </summary>
    void HeadbuttUpdate()
    {
        // ���˂�����
        if (isRocketMoving && !isWarping && IsHeadbutt())
        {
            if (hitAllFieldObjectManager.GetObjectType() != AllFieldObjectManager.ObjectType.SPONGE)
            {
                Vector3 beforeHeadbuttPosition = transform.position;

                // �����Ă���e�I�u�W�F�N�g
                Transform movingParent = null;

                // ���f����Ă���ꍇ
                if (cut.GetIsDivision())
                {
                    // ���� || �㑤
                    if ((transform.position.x < cut.GetDivisionPosition().x && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL) ||
                        (transform.position.y > cut.GetDivisionPosition().y && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL))
                    {
                        MoveObjectTransform(1, ref movingParent);
                    }
                    // �E�� || ����
                    else if ((transform.position.x >= cut.GetDivisionPosition().x && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL) ||
                             (transform.position.y <= cut.GetDivisionPosition().y && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL))
                    {
                        MoveObjectTransform(2, ref movingParent);
                    }
                }
                // ���f����Ă��Ȃ��ꍇ
                else { MoveObjectTransform(1, ref movingParent); }

                // �i�s�����ɕs���I�u�W�F�N�g�����邩�ǂ�������
                RaycastHit2D forwardHit = Physics2D.Raycast(beforeHeadbuttPosition, rocketVector, 0.8f, groundLayer);
                // �t�i�s�����ɉ��I�u�W�F�N�g�����邩�ǂ�������
                RaycastHit2D backHit = Physics2D.Raycast(beforeHeadbuttPosition, -rocketVector, 0.8f, groundLayer);
                // �i�s�����ɕs���I�u�W�F�N�g������A�t�i�s�����ɉ��I�u�W�F�N�g������Ƃ��m���ɃX�^�b�N����
                if (forwardHit.collider && backHit.collider && (forwardHit.transform.parent != movingParent || forwardHit.transform.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL))
                {
                    // �d�͂��Ȃ���
                    rbody2D.gravityScale = 0f;
                    // �����蔻��𖳂���
                    boxCollider2D.enabled = false;

                    // �t���O�̐ݒ�
                    isStacking = true;
                    definitelyStack = true;
                }

                // ���W���ۂ߂�
                transform.position = new Vector3(SnapToNearestHalf(beforeHeadbuttPosition.x), Mathf.Round(beforeHeadbuttPosition.y));

                // ���f����
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.GetComponent<AllFieldObjectManager>().AfterHeadbutt(IsHorizontalHeadbutt(), rocketVector.normalized, movingParent); }

                // �J�����V�F�C�N������
                cameraManager.ShakeCamera();

                //�A�j���[�V�����t���O
                animationScript.StartHit();
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
    void MoveObjectTransform(int _parentObjectNumber, ref Transform _movingParent)
    {
        cut.GetObjectTransform(_parentObjectNumber).transform.DOMove(cut.GetObjectTransform(_parentObjectNumber).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
        _movingParent = cut.GetObjectTransform(_parentObjectNumber);
    }
    void FinishMapMove() { isMoving = false; definitelyStack = false; }
    float SnapToNearestHalf(float _value) { return Mathf.Round(_value - 0.5f) + 0.5f; }


    void WarpInitialize(GameObject _hitWarpObj, ref Vector3 _warpPosition, ref GameObject _warpObj)
    {
        if (!isWarping)
        {
            // ���W���ۂ߂�
            transform.position = _hitWarpObj.transform.position;

            // ���[�v���̎擾
            _hitWarpObj.GetComponent<WarpManager>().SetWarpPosition(ref _warpPosition, ref _warpObj);

            // ���[�v���o�̊J�n
            animator.SetTrigger("InWarp");

            // �ړ��֌W
            rbody2D.linearVelocity = Vector2.zero;
            rbody2D.gravityScale = 0f;

            // �t���O
            isWarping = true;
        }
    }
    public void DoWarp()
    {
        transform.position = warpPosition;
    }
    public void FinishWarp()
    {
        rbody2D.gravityScale = 1f;
        isWarping = false;
    }

    void FixedUpdate()
    {
        // ���P�b�g�ړ������Ă��鎞�̂�Rigidbody2D�ɔ��f
        if (isRocketMoving && !isWarping) { rbody2D.linearVelocity = rocketVector * rocketSpeed; }
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
        AdjustRayPosition(ref currentOnePosition, true);
        AdjustRayPosition(ref currentTwoPosition, false);

        // Ray�̐���
        RaycastHit2D leftHit = Physics2D.Raycast(currentOnePosition, rocketVector.normalized, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentTwoPosition, rocketVector.normalized, 0.45f, groundLayer);

        return HeadbuttChecker(leftHit, rightHit);
    }
    void AdjustRayPosition(ref Vector3 _position, bool _isOne)
    {
        // ���炷
        if (Mathf.Abs(rocketVector.x) > 0f && _isOne) { _position.y -= halfSize; }
        else if (Mathf.Abs(rocketVector.x) > 0f && !_isOne) { _position.y -= halfSize; }
        else if (Mathf.Abs(rocketVector.y) > 0f && _isOne) { _position.x -= halfSize; }
        else if (Mathf.Abs(rocketVector.y) > 0f && !_isOne) { _position.x += halfSize; }
    }
    bool HeadbuttChecker(RaycastHit2D _leftHit, RaycastHit2D _rightHit)
    {
        // Ray��groundLayer�ɏՓ˂��Ă�����ڒn�����true��Ԃ�
        if (_leftHit.collider != null || _rightHit.collider != null)
        {
            GameObject hitObj = null;

            if (_leftHit.collider != null) { hitAllFieldObjectManager = _leftHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = _leftHit.collider.gameObject; }
            if (_rightHit.collider != null) { hitAllFieldObjectManager = _rightHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = _rightHit.collider.gameObject; }

            // ���������u���b�N�P�̂ɋN��������
            if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.FRAGILE)
            {
                hitAllFieldObjectManager.gameObject.SetActive(false);
            }
            else if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                // Warp���X�e�[�W�ɂQ�ȏ゠��Δ�����s��
                int warpCount = 0;

                foreach (GameObject warp in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    if (warp.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.WARP) { warpCount++; }
                }

                if (1 < warpCount)
                {
                    if (hitObj != warpObj)
                    {
                        WarpInitialize(hitObj, ref warpPosition, ref warpObj);
                        return false;
                    }

                    // ���݈ʒu�𔽉f
                    Vector3 currentOnePosition = transform.position + (rocketVector * 0.8f);
                    Vector3 currentTwoPosition = transform.position + (rocketVector * 0.8f);
                    AdjustRayPosition(ref currentOnePosition, true);
                    AdjustRayPosition(ref currentTwoPosition, false);

                    // Ray�̐���
                    RaycastHit2D leftHit = Physics2D.Raycast(currentOnePosition, rocketVector.normalized, 0.2f, groundLayer);
                    RaycastHit2D rightHit = Physics2D.Raycast(currentTwoPosition, rocketVector.normalized, 0.2f, groundLayer);

                    return HeadbuttChecker(leftHit, rightHit);
                }
                else
                {
                    return false;
                }
            }
            else if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.GLASS)
            {
                hitAllFieldObjectManager.gameObject.SetActive(false);
                return false;
            }
            return true;
        }
        return false;
    }

    // Setter
    public void RocketInitialize()
    {
        // �ړ��𖳂���
        rbody2D.linearVelocity = Vector2.zero;
        // �m��X�^�b�N�łȂ��Ƃ��d�͂��󂯂�悤�ɖ߂�
        if (!definitelyStack) { rbody2D.gravityScale = 1f; }

        // �t���O�̕ύX
        isRocketMoving = false;
        if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.SPONGE) { isMoving = false; }
    }
    public void FlagInitialize()
    {
        // �d�͂�߂�
        rbody2D.gravityScale = 1f;
        // �����蔻���߂�
        boxCollider2D.enabled = true;

        // �ړ����I�u�W�F�N�g���~�߂�
        DOTween.KillAll();

        // �t���O�̏�����
        isMoving = false;
        isStacking = false;
        definitelyStack = false;
    }
    public void SetDirection(int direction_) { direction = direction_; }
    public void SetDeathFreeze(Vector3 _viewPortPos)
    {
        // ���P�b�g�ړ������Ă��Ȃ������
        isRocketMoving = false;
        // �ړ��𖳂���
        rbody2D.linearVelocity = Vector2.zero;
        // ���W�𒲐�
        // ��
        if (_viewPortPos.x < 0) { transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + halfSize, transform.position.y, 0f); }
        // �E
        if (_viewPortPos.x > 1) { transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - halfSize, transform.position.y, 0f); }
        // ��
        if (_viewPortPos.y < 0) { transform.position = new Vector3(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + halfSize, 0f); }
        // ��
        if (_viewPortPos.y > 1) { transform.position = new Vector3(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - halfSize, 0f); }
        // �d�͂��Ȃ���
        rbody2D.gravityScale = 0f;
        // �����蔻��𖳂���
        boxCollider2D.enabled = false;
    }
    public void SetWarpObj(GameObject _warpObj) { warpObj = _warpObj; }

    // Getter
    public bool GetIsRocketMoving() { return isRocketMoving; }
    public bool GetIsStacking() { return isStacking; }
    public int GetDirection() { return direction; }
    public GameObject GetWarpObj() { return warpObj; }

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
                if (!isRocketMoving && collision.gameObject != warpObj)
                {
                    WarpInitialize(collision.gameObject, ref warpPosition, ref warpObj);
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("FieldObject"))
        {
            if (collision.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                if (collision.gameObject == warpObj)
                {
                    warpObj = null;
                }
            }
        }
    }
}
