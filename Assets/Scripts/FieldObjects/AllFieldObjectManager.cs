using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{
    // �Y������ObjectType
    public enum ObjectType
    {
        GROUND,
        GOAL,
        BLOCK,
        SPONGE,
        FRAGILE,
        WARP,
        GLASS,
        NAIL
    }
    [SerializeField] private ObjectType objectType;

    // ���W�Q
    private Vector3 prePosition;
    private Vector3 currentPosition;

    [Header("Hit Layer")]
    [SerializeField] private LayerMask groundLayer;

    void Start()
    {
        currentPosition = transform.position;

        switch (objectType)
        {
            case ObjectType.NAIL:

                transform.parent = null;

                break;
        }
    }

    /// <summary>
    /// �������ꂽ���Ƃ̏���
    /// </summary>
    public void AfterHeadbutt(bool _horizontalHeadbutt, Vector3 _rocketVector, Transform _movingParent)
    {
        // �O�t���[�����W�̕ۑ�
        prePosition = transform.position;
        // ���W�̍X�V
        currentPosition = transform.position + _rocketVector;

        // ���f���̎擾
        GameObject divisionLine = GameObject.FindGameObjectWithTag("DivisionLine");

        // �ړ����ׂ��I�u�W�F�N�g�����f����
        if (transform.parent == _movingParent)
        {
            switch (objectType)
            {
                case ObjectType.GROUND:
                case ObjectType.GOAL:
                case ObjectType.BLOCK:
                case ObjectType.SPONGE:
                case ObjectType.FRAGILE:
                case ObjectType.WARP:
                case ObjectType.GLASS:

                    // ����������̓��˂�
                    if (_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                    {
                        if ((prePosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= currentPosition.x) ||
                            (currentPosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= prePosition.x))
                        {
                            if (objectType == ObjectType.GOAL) { GetComponent<GoalManager>().SetIsCreateLine(false); }

                            gameObject.SetActive(false);
                        }
                    }
                    // �c��������̓��˂�
                    else if (!_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                    {
                        if ((prePosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= currentPosition.y) ||
                            (currentPosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= prePosition.y))
                        {
                            if (objectType == ObjectType.GOAL) { GetComponent<GoalManager>().SetIsCreateLine(false); }

                            gameObject.SetActive(false);
                        }
                    }

                    break;
            }

            // �B�u���b�N�ɓ�����������ł���
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, _rocketVector, 0.4f, groundLayer);
            if (objectType != ObjectType.NAIL && hit.collider != null && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() == ObjectType.NAIL) { gameObject.SetActive(false); }
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
    public Vector3 GetPrePosition() { return prePosition; }
    public Vector3 GetCurrentPosition() { return currentPosition; }

    // Setter
    public void SetPrePosition(Vector3 _prePosition) { prePosition = _prePosition; }
    public void SetCurrentPosition(Vector3 _currentPosition) { currentPosition = _currentPosition; }
}
