using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerCut : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // 他コンポーネント
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    [SerializeField] private PlayerAnimationScript animationScript;

    // フラグ類
    [SerializeField] private bool isActive;
    [SerializeField] private bool isReleaseStick;
    [Header("スタート時から分断線を生成させるか")]
    [SerializeField] private bool isCreateLineStart;

    // 分断座標
    private Vector2 divisionPosition;
    // 分断フラグ
    [SerializeField] private bool isDivision;
    // 分断決定フラグ
    private bool isDecision;
    private Vector2 decisionValue;

    // Global Volume
    [SerializeField] private float fadePower;
    [SerializeField] private Volume postEffectVolume;
    private Vignette vignette;
    private float maxIntensity = 0.45f;
    private float targetIntensity = 0f;

    // アニメーション関連
    int direction = 0;
    bool divisionFlag = false;
    bool divisionDeleteFlag = false;

    void Awake()
    {
        // 分断線の配置フラグを設定
        isDivision = isCreateLineStart;

        // 最初から分断線が配置されているなら、その情報を取得する
        if (isCreateLineStart)
        {
            // 分断線のモードを設定
            if (divisionLineObj.transform.rotation.z == 0f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL); }
            else { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.HORIZONTAL); }

            // 分断座標の設定
            divisionPosition = divisionLineObj.transform.position;
        }
    }

    void Start()
    {
        controller = GetComponent<PlayerController>();

        // 最初から分断線が配置されているなら、その情報を取得する
        if (isCreateLineStart)
        {
            divisionLineObj.transform.parent = null;

            // 分断処理
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                // 分断の影響を受けないもの
                if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                if (divisionLineObj.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                {
                    // 左側
                    if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                    // 右側
                    else { fieldObject.transform.parent = objectParent2; }
                }
                else if (divisionLineObj.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                {
                    // 上側
                    if (fieldObject.transform.position.y > divisionPosition.y) { fieldObject.transform.parent = objectParent1; }
                    // 下側
                    else { fieldObject.transform.parent = objectParent2; }
                }
            }
        }

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // 最初から分断線が配置されているときは分断線の操作は不可能にする
        if (!isCreateLineStart)
        {
            Debug.Log("PlayerCutUpdate");
            // 分断線の削除
            if (Input.GetButtonDown("Cancel") || (isActive && Input.GetButtonDown("Special")))
            {
                targetIntensity = 0f;

                // 親を元に戻す
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    // 分断の影響を受けないもの
                    if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                    fieldObject.transform.parent = objectParent1;
                }

                //アニメーションフラグ
                if (isDivision == true) divisionDeleteFlag = true;

                isDivision = false;
                isDecision = false;

                isActive = false;
                divisionLineObj.SetActive(false);

            }
            // 分断線の生成
            else if (!isActive && controller.IsGrounded() && !controller.GetIsRocketMoving() && Input.GetButtonDown("Special"))
            {
                if (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f || Input.GetAxisRaw("Vertical") < 0f || Input.GetAxisRaw("Vertical") > 0f)
                {
                    isReleaseStick = false;
                }
                targetIntensity = maxIntensity;
                isActive = true;
            }

            // 指を一度離させる処理
            if (isActive && !isReleaseStick && Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f) { isReleaseStick = true; }

            // 分断方向の決定
            if (isActive && isReleaseStick && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.3f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.3f))
            {
                if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.3f)
                {
                    decisionValue.x = Input.GetAxisRaw("Horizontal");
                    decisionValue.y = 0f;

                    // フラグの更新
                    isDecision = true;
                }
                else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.3f)
                {
                    decisionValue.x = 0f;
                    decisionValue.y = Input.GetAxisRaw("Vertical");

                    // フラグの更新
                    isDecision = true;
                }
            }

            // ロケット移動をしておらず、地面に接地している時に分断可能
            if (isActive && isReleaseStick && isDecision && Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.3f && Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.3f)
            {
                // まだ分断していなかったら、初分断フラグをtrueにする
                if (!isDivision) { isDivision = true; }
                // 分断座標は整数丸めをしたプレイヤー座標
                if (decisionValue.x < -0.3f) { divisionPosition = new Vector2(Mathf.FloorToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); direction = 2; }
                if (decisionValue.x > 0.3f)  { divisionPosition = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); direction = 0; }
                if (decisionValue.y < -0.3f) { divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) - 0.5f); direction = 3; }
                if (decisionValue.y > 0.3f)  { divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) + 0.5f); direction = 1; }

                // 分断線の再表示
                if (!divisionLineObj.activeSelf)
                {
                    divisionLineObj.transform.parent = null;
                    divisionLineObj.SetActive(true);
                }
                // 分断線の回転を修正
                if (Mathf.Abs(decisionValue.x) > 0.3f) { divisionLineObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f)); }
                if (Mathf.Abs(decisionValue.y) > 0.3f) { divisionLineObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f)); }
                // 分断線の位置を修正
                divisionLineObj.transform.position = new Vector3(divisionPosition.x, divisionPosition.y, 0f);
                // 分断線に情報を与える
                if (Mathf.Abs(decisionValue.x) > 0.3f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL); }
                if (Mathf.Abs(decisionValue.y) > 0.3f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.HORIZONTAL); }

                // 分断処理
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    // 分断の影響を受けないもの
                    if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                    if (Mathf.Abs(decisionValue.x) > 0.3f)
                    {
                        // 左側
                        if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                        // 右側
                        else { fieldObject.transform.parent = objectParent2; }
                    }
                    else if (Mathf.Abs(decisionValue.y) > 0.3f)
                    {
                        // 上側
                        if (fieldObject.transform.position.y > divisionPosition.y) { fieldObject.transform.parent = objectParent1; }
                        // 下側
                        else { fieldObject.transform.parent = objectParent2; }
                    }
                }

                targetIntensity = 0f;
                isActive = false;
                //アニメーショントリガー
                divisionFlag = true;
                animationScript.StartCut();

            }

            // Global Volume
            vignette.intensity.value += (targetIntensity - vignette.intensity.value) * (fadePower * Time.deltaTime);
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
    public DivisionLineManager GetDivisionLineManager() { return divisionLineObj.GetComponent<DivisionLineManager>(); }
    public bool GetIsCreateLineStart() { return isCreateLineStart; }
    public bool GetIsActive() { return isActive; }
    public int GetDirection() { return direction; }
    
    public bool GetDivisionFlag() { if (!divisionFlag) return false; divisionFlag = false; return true; }
    public bool GetDivisionDeleteFlag() { if (!divisionDeleteFlag) return false; divisionDeleteFlag = false; return true; }

    // Setter
    public void SetDivisionPosition(Vector2 _divisionPosition) { divisionPosition = _divisionPosition; }
    public void SetIsDivision(bool _isDivision) { isDivision = _isDivision; }
    public void SetDirection(int direction_) { direction = direction_; }
}
