using UnityEngine;

public class PlayerTear : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;

    // ���R���|�[�l���g
    [SerializeField] private Transform objectParent1;
    [SerializeField] private Transform objectParent2;
    [SerializeField] private GameObject divisionLineObj;
    private UndoManager undoManager;

    // ���f���W
    private Vector2 divisionPosition;
    // ���f�t���O
    private bool isDivision;

    void Start()
    {
        controller = GetComponent<PlayerController>();

        // ���R���|�[�l���g���擾
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
    }

    public void ManualUpdate()
    {
        // ���P�b�g�ړ������Ă��炸�A�n�ʂɐڒn���Ă��鎞�ɕ��f�\
        if (Input.GetButtonDown("Special") && !controller.GetIsRocketMoving() && controller.IsGrounded())
        {
            // �ړ��O�ɕۑ�
            undoManager.SaveState();

            // �܂����f���Ă��Ȃ�������A�����f�t���O��true�ɂ���
            if (!isDivision) { isDivision = true; }
            // ���f���W�͐����ۂ߂������v���C���[���W
            divisionPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

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
                if (fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x)) { fieldObject.transform.parent = objectParent1; }
                // �E��
                else { fieldObject.transform.parent = objectParent2; }
            }
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

    // Setter
    public void SetDivisionPosition(Vector2 _divisionPosition) { divisionPosition = _divisionPosition; }
    public void SetIsDivision(bool _isDivision) { isDivision = _isDivision; }
}
