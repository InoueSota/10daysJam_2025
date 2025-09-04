using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 自コンポーネント
    private PlayerCut cut;
    private Rigidbody2D rbody2D;
    private Animator animator;
    private PlayerSpriteScript playerSpriteScript;

    // 他コンポーネント
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

    // フラグ
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isStacking;
    private bool wasUndo;

    // ワープ
    private GameObject warpObj;

    //Animation系
    private int direction= 0;

    void Start()
    {
        // 自コンポーネントを取得
        cut = GetComponent<PlayerCut>();
        rbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerSpriteScript = GetComponent<PlayerSpriteScript>();

        // 他コンポーネントを取得
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
        divisionLineManager = cut.GetDivisionLineManager();
    }

    public void ManualUpdate()
    {
        if (!isStacking)
        {
            // 左右移動処理
            MoveUpdate();
            // 頭突き処理
            HeadbuttUpdate();

            if (!wasUndo)
            {
                // スタックしているか判定
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
    /// 移動処理
    /// </summary>
    void MoveUpdate()
    {
        if (!isRocketMoving) { rbody2D.linearVelocity = new Vector2(0f, rbody2D.linearVelocity.y); }

        if (!isRocketMoving && !isMoving && IsGrounded() && Input.GetButtonDown("Jump") &&
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            // 移動前に保存
            undoManager.SaveState();

            // 移動ベクトルの初期化
            rocketVector = Vector2.zero;
            // 重力を無くす
            rbody2D.gravityScale = 0f;

            // 右方向に入力
            if (Input.GetAxisRaw("Horizontal") > 0.5f) { rocketVector.x = rocketSpeed; direction = 0; }
            // 左方向に入力
            else if (Input.GetAxisRaw("Horizontal") < -0.5f) { rocketVector.x = -rocketSpeed; direction = 2; }
            // 上方向に入力
            else if (Input.GetAxisRaw("Vertical") > 0.5f) { rocketVector.y = rocketSpeed; direction = 1; }
            // 下方向に入力
            else if (Input.GetAxisRaw("Vertical") < -0.5f) { rocketVector.y = -rocketSpeed; direction = 3; }

            // ワープ対象オブジェクトの情報を初期化する
            warpObj = null;

            // フラグの変更
            isMoving = true;
            isRocketMoving = true;
        }
    }

    /// <summary>
    /// 頭突き処理
    /// </summary>
    void HeadbuttUpdate()
    {
        // 頭突き処理
        if (isRocketMoving && IsHeadbutt())
        {
            if (hitAllFieldObjectManager.GetObjectType() != AllFieldObjectManager.ObjectType.SPONGE)
            {
                Vector3 beforeHeadbuttPosition = transform.position;

                // 動いている親オブジェクト
                Transform movingParent = null;

                // 分断されている場合
                if (cut.GetIsDivision())
                {
                    // 上下線
                    if (divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                    {
                        // 左側
                        if (transform.position.x < cut.GetDivisionPosition().x)
                        {
                            cut.GetObjectTransform(1).transform.DOMove(cut.GetObjectTransform(1).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(1);
                        }
                        // 右側
                        else
                        {
                            cut.GetObjectTransform(2).transform.DOMove(cut.GetObjectTransform(2).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(2);
                        }
                    }
                    // 左右線
                    else if (divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                    {
                        // 上側
                        if (transform.position.y > cut.GetDivisionPosition().y)
                        {
                            cut.GetObjectTransform(1).transform.DOMove(cut.GetObjectTransform(1).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(1);
                        }
                        // 下側
                        else
                        {
                            cut.GetObjectTransform(2).transform.DOMove(cut.GetObjectTransform(2).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove);
                            movingParent = cut.GetObjectTransform(2);
                        }
                    }
                }
                // 分断されていない場合
                else { cut.GetObjectTransform(1).transform.DOMove(cut.GetObjectTransform(1).transform.position + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine).OnComplete(FinishMapMove); }

                // 分断処理
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.GetComponent<AllFieldObjectManager>().AfterHeadbutt(IsHorizontalHeadbutt(), rocketVector.normalized, movingParent); }

                // プレイヤーがずらしによって埋もれる場合のみ１マス前に動かす
                RaycastHit2D hit = Physics2D.Raycast(beforeHeadbuttPosition, -rocketVector.normalized, 0.8f, groundLayer);
                if (hit.collider != null) { transform.DOMove(beforeHeadbuttPosition + rocketVector.normalized, mapMoveTime).SetEase(Ease.OutSine); }
            }

            // 変数の初期化
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
    public bool IsHeadbutt()
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
            GameObject hitObj = null;

            if (leftHit.collider != null) { hitAllFieldObjectManager = leftHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = leftHit.collider.gameObject; }
            if (rightHit.collider != null) { hitAllFieldObjectManager = rightHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = rightHit.collider.gameObject; }

            // 当たったブロック単体に起こす処理
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

            // 頭突きに成功したらワープObjを初期化する
            warpObj = null;
            return true;
        }
        return false;
    }

    // Setter
    public void RocketInitialize()
    {
        // 移動を無くす
        rbody2D.linearVelocity = Vector2.zero;
        // 重力を受けるように戻す
        rbody2D.gravityScale = 1f;

        // フラグの変更
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
    /// 当たり判定群
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
