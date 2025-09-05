using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerCut : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;
    private PlayerAnimationScript anim;

    // 他コンポーネント
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    private UndoManager undoManager;

    // フラグ類
    private bool isActive;
    private bool isReleaseStick;
    [Header("スタート時から分断線を生成させるか")]
    [SerializeField] private bool isCreateLineStart;

    // 分断座標
    private Vector2 divisionPosition;
    // 分断フラグ
    private bool isDivision;

    // Global Volume
    [SerializeField] private float fadePower;
    [SerializeField] private Volume postEffectVolume;
    private Vignette vignette;
    private float maxIntensity = 0.5f;
    private float targetIntensity = 0f;

    //アニメーション関連
    int direction = 0;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        anim = GetComponent<PlayerAnimationScript>();

        // 他コンポーネントを取得
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();

        // 最初から分断線が配置されているなら、その情報を取得する
        if (isCreateLineStart)
        {
            divisionLineObj.transform.parent = null;

            // 分断線のモードを設定
            if (divisionLineObj.transform.rotation.z == 0f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL); }
            else                                            { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.HORIZONTAL); }

            // 分断座標の設定
            divisionPosition = divisionLineObj.transform.position;

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

            // 分断線の配置フラグを設定
            isDivision = isCreateLineStart;
        }

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // 最初から分断線が配置されているときは分断線の操作は不可能にする
        if (!isCreateLineStart)
        {
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
                isDivision = false;

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
            if (isActive && !isReleaseStick && Input.GetAxisRaw("Horizontal") == 0f) { isReleaseStick = true; }

            // ロケット移動をしておらず、地面に接地している時に分断可能
            if (isActive && isReleaseStick && (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f || Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f))
            {
                // 移動前に保存
                undoManager.SaveState();

                // まだ分断していなかったら、初分断フラグをtrueにする
                if (!isDivision) { isDivision = true; }
                // 分断座標は整数丸めをしたプレイヤー座標
                if (Input.GetAxisRaw("Horizontal") < -0.3f) { divisionPosition = new Vector2(Mathf.FloorToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); direction = 2; }
                if (Input.GetAxisRaw("Horizontal") > 0.3f) { divisionPosition = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); direction = 0; }
                if (Input.GetAxisRaw("Vertical") < -0.3f) { divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) - 0.5f); direction = 3; }
                if (Input.GetAxisRaw("Vertical") > 0.3f) { divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) + 0.5f); direction = 1; }

                // 分断線の再表示
                if (!divisionLineObj.activeSelf)
                {
                    divisionLineObj.transform.parent = null;
                    divisionLineObj.SetActive(true);
                }
                // 分断線の回転を修正
                if (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f) { divisionLineObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f)); }
                if (Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f) { divisionLineObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f)); }
                // 分断線の位置を修正
                divisionLineObj.transform.position = new Vector3(divisionPosition.x, divisionPosition.y, 0f);
                // 分断線に情報を与える
                if (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL); }
                if (Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.HORIZONTAL); }

                // 分断処理
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    // 分断の影響を受けないもの
                    if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                    if (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f)
                    {
                        // 左側
                        if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                        // 右側
                        else { fieldObject.transform.parent = objectParent2; }
                    }
                    else if (Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f)
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
                anim.StartCut();
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

    // Setter
    public void SetDivisionPosition(Vector2 _divisionPosition) { divisionPosition = _divisionPosition; }
    public void SetIsDivision(bool _isDivision) { isDivision = _isDivision; }
    public void SetDirection(int direction_) { direction = direction_; }
}
