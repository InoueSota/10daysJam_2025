using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTear : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;

    // ���R���|�[�l���g
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    private UndoManager undoManager;

    // �t���O��
    private bool isActive;
    private bool isReleaseStick;

    // ���f���W
    private Vector2 divisionPosition;
    // ���f�t���O
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

        // ���R���|�[�l���g���擾
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // �j��A�J�n
        if (!isActive && controller.IsGrounded() && !controller.GetIsRocketMoving() && Input.GetButtonDown("Special"))
        {
            if (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f)
            {
                isReleaseStick = false;
            }
            targetIntensity = maxIntensity;
            isActive = true;
        }
        // �j��A�I��
        else if (isActive && Input.GetButtonDown("Special"))
        {
            targetIntensity = 0f;

            // �e�����ɖ߂�
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject")) { fieldObject.transform.parent = objectParent1; }
            isDivision = false;

            isActive = false;
            divisionLineObj.SetActive(false);
        }

        // �w����x�������鏈��
        if (isActive && !isReleaseStick && Input.GetAxisRaw("Horizontal") == 0f) { isReleaseStick = true; }

        // ���P�b�g�ړ������Ă��炸�A�n�ʂɐڒn���Ă��鎞�ɕ��f�\
        if (isActive && isReleaseStick && (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f))
        {
            // �ړ��O�ɕۑ�
            undoManager.SaveState();

            // �܂����f���Ă��Ȃ�������A�����f�t���O��true�ɂ���
            if (!isDivision) { isDivision = true; }
            // ���f���W�͐����ۂ߂������v���C���[���W
            if (Input.GetAxisRaw("Horizontal") < 0f) { divisionPosition = new Vector2(Mathf.FloorToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }
            if (Input.GetAxisRaw("Horizontal") > 0f) { divisionPosition = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }

            // ���f���̍ĕ\��
            if (!divisionLineObj.activeSelf)
            {
                divisionLineObj.transform.parent = null;
                divisionLineObj.SetActive(true);
            }
            // ���f���̈ʒu���C��
            divisionLineObj.transform.position = new Vector3(divisionPosition.x, 6f, 0f);
            // ���f���ɏ���^����
            divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL);

            // ���f����
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                // ����
                if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                // �E��
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
