using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 自コンポーネント
    private PlayerCut cut;
    private Rigidbody2D rbody2D;
    private BoxCollider2D boxCollider2D;
    private Animator animator;

    // 他コンポーネント
    private CameraManager cameraManager;
    private UndoManager undoManager;
    private DivisionLineManager divisionLineManager;
    [SerializeField] private PlayerAnimationScript animationScript;

    [Header("基本的なパラメータ")]
    [SerializeField] private float halfSize;

    [Header("ロケットパラメータ")]
    [SerializeField] private float toMaxSpeedTime;
    [SerializeField] private float rocketMaxSpeed;
    private float rocketSpeed;
    private Vector3 rocketVector;
    private bool isRocketMoving;
    private AllFieldObjectManager hitAllFieldObjectManager;

    // ワープパラメータ
    private Vector3 warpPosition;
    private bool isWarping;

    [Header("当たり判定を行うレイヤー")]
    [SerializeField] private LayerMask groundLayer;

    [Header("ステージオブジェクトが動く速度")]
    [SerializeField] private float mapMoveTime;

    // フラグ
    private bool isMoving;
    private bool isStacking;
    private bool definitelyStack;

    // ワープ
    private GameObject warpObj;

    // Animation系
    private int direction = 0;

    void Start()
    {
        // 自コンポーネントを取得
        cut = GetComponent<PlayerCut>();
        rbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        // 他コンポーネントを取得
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
        divisionLineManager = cut.GetDivisionLineManager();
    }

    public void ManualUpdate()
    {
        if (!isStacking && !definitelyStack)
        {
            // 左右移動処理
            MoveUpdate();
            // 頭突き処理
            HeadbuttUpdate();
        }

        // 確定スタックじゃないときに判定を取る
        if (!definitelyStack)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 0.1f, groundLayer);
            if (hit.collider != null && hit.transform.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.WARP) { isStacking = true; }
            else { isStacking = false; }
        }
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    void MoveUpdate()
    {
        if (!isRocketMoving) { rbody2D.linearVelocity = new Vector2(0f, rbody2D.linearVelocity.y); }

        if (!isRocketMoving && !isMoving && IsGrounded() && !cut.GetIsActive() && Input.GetButtonDown("Jump") &&
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f))
        {
            // 座標を丸める
            transform.position = new Vector3(SnapToNearestHalf(transform.position.x), Mathf.Round(transform.position.y));
            // 移動前に保存
            undoManager.SaveState();

            // 移動ベクトルの初期化
            rocketVector = Vector2.zero;
            // 重力を無くす
            rbody2D.gravityScale = 0f;

            // 右方向に入力
            if (Input.GetAxisRaw("Horizontal") > 0.5f) { rocketVector = Vector3.right; direction = 0; }
            // 左方向に入力
            else if (Input.GetAxisRaw("Horizontal") < -0.5f) { rocketVector = Vector3.left; direction = 2; }
            // 上方向に入力
            else if (Input.GetAxisRaw("Vertical") > 0.5f) { rocketVector = Vector3.up; direction = 1; }
            // 下方向に入力
            else if (Input.GetAxisRaw("Vertical") < -0.5f) { rocketVector = Vector3.down; direction = 3; }

            // ロケットの移動速度を変える
            DOVirtual.Float(0f, rocketMaxSpeed, toMaxSpeedTime, value => { rocketSpeed = value; }).SetEase(Ease.Linear);

            // ワープ対象オブジェクトの情報を初期化する
            warpObj = null;

            // フラグの変更
            isMoving = true;
            isRocketMoving = true;

            //アニメーショントリガー
            animationScript.StartRocket();
        }
    }

    /// <summary>
    /// 頭突き処理
    /// </summary>
    void HeadbuttUpdate()
    {
        // 頭突き処理
        if (isRocketMoving && !isWarping && IsHeadbutt())
        {
            if (hitAllFieldObjectManager.GetObjectType() != AllFieldObjectManager.ObjectType.SPONGE)
            {
                Vector3 beforeHeadbuttPosition = transform.position;

                // 動いている親オブジェクト
                Transform movingParent = null;

                // 分断されている場合
                if (cut.GetIsDivision())
                {
                    // 左側 || 上側
                    if ((transform.position.x < cut.GetDivisionPosition().x && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL) ||
                        (transform.position.y > cut.GetDivisionPosition().y && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL))
                    {
                        MoveObjectTransform(1, ref movingParent);
                    }
                    // 右側 || 下側
                    else if ((transform.position.x >= cut.GetDivisionPosition().x && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL) ||
                             (transform.position.y <= cut.GetDivisionPosition().y && divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL))
                    {
                        MoveObjectTransform(2, ref movingParent);
                    }
                }
                // 分断されていない場合
                else { MoveObjectTransform(1, ref movingParent); }

                // 進行方向に不動オブジェクトがあるかどうか判定
                RaycastHit2D forwardHit = Physics2D.Raycast(beforeHeadbuttPosition, rocketVector, 0.8f, groundLayer);
                // 逆進行方向に可動オブジェクトがあるかどうか判定
                RaycastHit2D backHit = Physics2D.Raycast(beforeHeadbuttPosition, -rocketVector, 0.8f, groundLayer);
                // 進行方向に不動オブジェクトがあり、逆進行方向に可動オブジェクトがあるとき確実にスタックする
                if (forwardHit.collider && backHit.collider && (forwardHit.transform.parent != movingParent || forwardHit.transform.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL))
                {
                    // 重力をなくす
                    rbody2D.gravityScale = 0f;
                    // 当たり判定を無くす
                    boxCollider2D.enabled = false;

                    // フラグの設定
                    isStacking = true;
                    definitelyStack = true;
                }

                // 座標を丸める
                transform.position = new Vector3(SnapToNearestHalf(beforeHeadbuttPosition.x), Mathf.Round(beforeHeadbuttPosition.y));

                // 分断処理
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.GetComponent<AllFieldObjectManager>().AfterHeadbutt(IsHorizontalHeadbutt(), rocketVector.normalized, movingParent); }

                // カメラシェイクをする
                cameraManager.ShakeCamera();

                //アニメーションフラグ
                animationScript.StartHit();
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
            // 座標を丸める
            transform.position = _hitWarpObj.transform.position;

            // ワープ情報の取得
            _hitWarpObj.GetComponent<WarpManager>().SetWarpPosition(ref _warpPosition, ref _warpObj);

            // ワープ演出の開始
            animator.SetTrigger("InWarp");

            // 移動関係
            rbody2D.linearVelocity = Vector2.zero;
            rbody2D.gravityScale = 0f;

            // フラグ
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
        // ロケット移動をしている時のみRigidbody2Dに反映
        if (isRocketMoving && !isWarping) { rbody2D.linearVelocity = rocketVector * rocketSpeed; }
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
        AdjustRayPosition(ref currentOnePosition, true);
        AdjustRayPosition(ref currentTwoPosition, false);

        // Rayの生成
        RaycastHit2D leftHit = Physics2D.Raycast(currentOnePosition, rocketVector.normalized, 0.45f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(currentTwoPosition, rocketVector.normalized, 0.45f, groundLayer);

        return HeadbuttChecker(leftHit, rightHit);
    }
    void AdjustRayPosition(ref Vector3 _position, bool _isOne)
    {
        // ずらす
        if (Mathf.Abs(rocketVector.x) > 0f && _isOne) { _position.y -= halfSize; }
        else if (Mathf.Abs(rocketVector.x) > 0f && !_isOne) { _position.y -= halfSize; }
        else if (Mathf.Abs(rocketVector.y) > 0f && _isOne) { _position.x -= halfSize; }
        else if (Mathf.Abs(rocketVector.y) > 0f && !_isOne) { _position.x += halfSize; }
    }
    bool HeadbuttChecker(RaycastHit2D _leftHit, RaycastHit2D _rightHit)
    {
        // RayがgroundLayerに衝突していたら接地判定はtrueを返す
        if (_leftHit.collider != null || _rightHit.collider != null)
        {
            GameObject hitObj = null;

            if (_leftHit.collider != null) { hitAllFieldObjectManager = _leftHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = _leftHit.collider.gameObject; }
            if (_rightHit.collider != null) { hitAllFieldObjectManager = _rightHit.collider.GetComponent<AllFieldObjectManager>(); hitObj = _rightHit.collider.gameObject; }

            // 当たったブロック単体に起こす処理
            if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.FRAGILE)
            {
                hitAllFieldObjectManager.gameObject.SetActive(false);
            }
            else if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                // Warpがステージに２つ以上あれば判定を行う
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

                    // 現在位置を反映
                    Vector3 currentOnePosition = transform.position + (rocketVector * 0.8f);
                    Vector3 currentTwoPosition = transform.position + (rocketVector * 0.8f);
                    AdjustRayPosition(ref currentOnePosition, true);
                    AdjustRayPosition(ref currentTwoPosition, false);

                    // Rayの生成
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
        // 移動を無くす
        rbody2D.linearVelocity = Vector2.zero;
        // 確定スタックでないとき重力を受けるように戻す
        if (!definitelyStack) { rbody2D.gravityScale = 1f; }

        // フラグの変更
        isRocketMoving = false;
        if (hitAllFieldObjectManager && hitAllFieldObjectManager.GetObjectType() == AllFieldObjectManager.ObjectType.SPONGE) { isMoving = false; }
    }
    public void FlagInitialize()
    {
        // 重力を戻す
        rbody2D.gravityScale = 1f;
        // 当たり判定を戻す
        boxCollider2D.enabled = true;

        // 移動中オブジェクトを止める
        DOTween.KillAll();

        // フラグの初期化
        isMoving = false;
        isStacking = false;
        definitelyStack = false;
    }
    public void SetDirection(int direction_) { direction = direction_; }
    public void SetDeathFreeze(Vector3 _viewPortPos)
    {
        // ロケット移動をしていない判定に
        isRocketMoving = false;
        // 移動を無くす
        rbody2D.linearVelocity = Vector2.zero;
        // 座標を調整
        // 左
        if (_viewPortPos.x < 0) { transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + halfSize, transform.position.y, 0f); }
        // 右
        if (_viewPortPos.x > 1) { transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - halfSize, transform.position.y, 0f); }
        // 下
        if (_viewPortPos.y < 0) { transform.position = new Vector3(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + halfSize, 0f); }
        // 上
        if (_viewPortPos.y > 1) { transform.position = new Vector3(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - halfSize, 0f); }
        // 重力をなくす
        rbody2D.gravityScale = 0f;
        // 当たり判定を無くす
        boxCollider2D.enabled = false;
    }
    public void SetWarpObj(GameObject _warpObj) { warpObj = _warpObj; }

    // Getter
    public bool GetIsRocketMoving() { return isRocketMoving; }
    public bool GetIsStacking() { return isStacking; }
    public int GetDirection() { return direction; }
    public GameObject GetWarpObj() { return warpObj; }

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
