using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTear : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // 他コンポーネント
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    private UndoManager undoManager;

    // フラグ類
    private bool isActive;
    private bool isReleaseStick;

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

    void Start()
    {
        controller = GetComponent<PlayerController>();

        // 他コンポーネントを取得
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // 破り、開始
        if (!isActive && controller.IsGrounded() && !controller.GetIsRocketMoving() && Input.GetButtonDown("Special"))
        {
            if (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f)
            {
                isReleaseStick = false;
            }
            targetIntensity = maxIntensity;
            isActive = true;
        }
        // 破り、終了
        else if (isActive && Input.GetButtonDown("Special"))
        {
            targetIntensity = 0f;

            // 親を元に戻す
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.transform.parent = objectParent1; }
            isDivision = false;

            isActive = false;
            divisionLineObj.SetActive(false);
        }

        // 指を一度離させる処理
        if (isActive && !isReleaseStick && Input.GetAxisRaw("Horizontal") == 0f) { isReleaseStick = true; }

        // ロケット移動をしておらず、地面に接地している時に分断可能
        if (isActive && isReleaseStick && (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f))
        {
            // 移動前に保存
            undoManager.SaveState();

            // まだ分断していなかったら、初分断フラグをtrueにする
            if (!isDivision) { isDivision = true; }
            // 分断座標は整数丸めをしたプレイヤー座標
            if (Input.GetAxisRaw("Horizontal") < 0f) { divisionPosition = new Vector2(Mathf.FloorToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }
            if (Input.GetAxisRaw("Horizontal") > 0f) { divisionPosition = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }

            // 分断線の再表示
            if (!divisionLineObj.activeSelf)
            {
                divisionLineObj.transform.parent = null;
                divisionLineObj.SetActive(true);
            }
            // 分断線の位置を修正
            divisionLineObj.transform.position = new Vector3(divisionPosition.x, 6f, 0f);
            // 分断線に情報を与える
            divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL);

            // 分断処理
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                // 左側
                if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                // 右側
                else { fieldObject.transform.parent = objectParent2; }
            }

            targetIntensity = 0f;
            isActive = false;
        }

        // Global Volume
        vignette.intensity.value += (targetIntensity - vignette.intensity.value) * (fadePower * Time.deltaTime);
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

    // Setter
    public void SetDivisionPosition(Vector2 _divisionPosition) { divisionPosition = _divisionPosition; }
    public void SetIsDivision(bool _isDivision) { isDivision = _isDivision; }
}
