using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerCut : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;
    private PlayerAnimationScript anim;

    // ���R���|�[�l���g
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    private UndoManager undoManager;

    // �t���O��
    private bool isActive;
    private bool isReleaseStick;
    [Header("�X�^�[�g�����番�f���𐶐������邩")]
    [SerializeField] private bool isCreateLineStart;

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

    //�A�j���[�V�����֘A
    int direction = 0;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        anim = GetComponent<PlayerAnimationScript>();

        // ���R���|�[�l���g���擾
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();

        // �ŏ����番�f�����z�u����Ă���Ȃ�A���̏����擾����
        if (isCreateLineStart)
        {
            divisionLineObj.transform.parent = null;

            // ���f���̃��[�h��ݒ�
            if (divisionLineObj.transform.rotation.z == 0f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL); }
            else                                            { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.HORIZONTAL); }

            // ���f���W�̐ݒ�
            divisionPosition = divisionLineObj.transform.position;

            // ���f����
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                // ���f�̉e�����󂯂Ȃ�����
                if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                if (divisionLineObj.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                {
                    // ����
                    if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                    // �E��
                    else { fieldObject.transform.parent = objectParent2; }
                }
                else if (divisionLineObj.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                {
                    // �㑤
                    if (fieldObject.transform.position.y > divisionPosition.y) { fieldObject.transform.parent = objectParent1; }
                    // ����
                    else { fieldObject.transform.parent = objectParent2; }
                }
            }

            // ���f���̔z�u�t���O��ݒ�
            isDivision = isCreateLineStart;
        }

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // �ŏ����番�f�����z�u����Ă���Ƃ��͕��f���̑���͕s�\�ɂ���
        if (!isCreateLineStart)
        {
            // ���f���̍폜
            if (Input.GetButtonDown("Cancel") || (isActive && Input.GetButtonDown("Special")))
            {
                targetIntensity = 0f;

                // �e�����ɖ߂�
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    // ���f�̉e�����󂯂Ȃ�����
                    if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                    fieldObject.transform.parent = objectParent1;
                }
                isDivision = false;

                isActive = false;
                divisionLineObj.SetActive(false);
            }
            // ���f���̐���
            else if (!isActive && controller.IsGrounded() && !controller.GetIsRocketMoving() && Input.GetButtonDown("Special"))
            {
                if (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f || Input.GetAxisRaw("Vertical") < 0f || Input.GetAxisRaw("Vertical") > 0f)
                {
                    isReleaseStick = false;
                }
                targetIntensity = maxIntensity;
                isActive = true;
            }

            // �w����x�������鏈��
            if (isActive && !isReleaseStick && Input.GetAxisRaw("Horizontal") == 0f) { isReleaseStick = true; }

            // ���P�b�g�ړ������Ă��炸�A�n�ʂɐڒn���Ă��鎞�ɕ��f�\
            if (isActive && isReleaseStick && (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f || Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f))
            {
                // �ړ��O�ɕۑ�
                undoManager.SaveState();

                // �܂����f���Ă��Ȃ�������A�����f�t���O��true�ɂ���
                if (!isDivision) { isDivision = true; }
                // ���f���W�͐����ۂ߂������v���C���[���W
                if (Input.GetAxisRaw("Horizontal") < -0.3f) { divisionPosition = new Vector2(Mathf.FloorToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); direction = 2; }
                if (Input.GetAxisRaw("Horizontal") > 0.3f) { divisionPosition = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); direction = 0; }
                if (Input.GetAxisRaw("Vertical") < -0.3f) { divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) - 0.5f); direction = 3; }
                if (Input.GetAxisRaw("Vertical") > 0.3f) { divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) + 0.5f); direction = 1; }

                // ���f���̍ĕ\��
                if (!divisionLineObj.activeSelf)
                {
                    divisionLineObj.transform.parent = null;
                    divisionLineObj.SetActive(true);
                }
                // ���f���̉�]���C��
                if (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f) { divisionLineObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f)); }
                if (Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f) { divisionLineObj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f)); }
                // ���f���̈ʒu���C��
                divisionLineObj.transform.position = new Vector3(divisionPosition.x, divisionPosition.y, 0f);
                // ���f���ɏ���^����
                if (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.VERTICAL); }
                if (Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f) { divisionLineObj.GetComponent<DivisionLineManager>().Initialize(DivisionLineManager.DivisionMode.HORIZONTAL); }

                // ���f����
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    // ���f�̉e�����󂯂Ȃ�����
                    if (fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.NAIL) { continue; }

                    if (Input.GetAxisRaw("Horizontal") < -0.3f || Input.GetAxisRaw("Horizontal") > 0.3f)
                    {
                        // ����
                        if (fieldObject.transform.position.x < divisionPosition.x) { fieldObject.transform.parent = objectParent1; }
                        // �E��
                        else { fieldObject.transform.parent = objectParent2; }
                    }
                    else if (Input.GetAxisRaw("Vertical") < -0.3f || Input.GetAxisRaw("Vertical") > 0.3f)
                    {
                        // �㑤
                        if (fieldObject.transform.position.y > divisionPosition.y) { fieldObject.transform.parent = objectParent1; }
                        // ����
                        else { fieldObject.transform.parent = objectParent2; }
                    }
                }

                targetIntensity = 0f;
                isActive = false;
                //�A�j���[�V�����g���K�[
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
