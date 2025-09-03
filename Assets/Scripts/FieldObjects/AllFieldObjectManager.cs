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
        FRAGILE
    }
    [SerializeField] private ObjectType objectType;

    // ���W�Q
    private Vector3 prePosition;
    private Vector3 currentPosition;

    void Start()
    {
        currentPosition = transform.position;
    }

    /// <summary>
    /// �������ꂽ���Ƃ̏���
    /// </summary>
    public void AfterHeadbutt(bool _horizontalHeadbutt)
    {
        // �O�t���[�����W�̕ۑ�
        prePosition = currentPosition;
        // ���W�̍X�V
        currentPosition = transform.position;

        // ���f���̎擾
        GameObject divisionLine = GameObject.FindGameObjectWithTag("DivisionLine");

        switch (objectType)
        {
            case ObjectType.GROUND:



                break;
            case ObjectType.GOAL:
            case ObjectType.BLOCK:
            case ObjectType.SPONGE:

                // ����������̓��˂�
                if (_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
                {
                    if ((prePosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= currentPosition.x) ||
                        (currentPosition.x < divisionLine.transform.position.x && divisionLine.transform.position.x <= prePosition.x))
                    {
                        gameObject.SetActive(false);
                    }
                }
                // �c��������̓��˂�
                else if (!_horizontalHeadbutt && divisionLine && divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
                {
                    if ((prePosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= currentPosition.y) ||
                        (currentPosition.y < divisionLine.transform.position.y && divisionLine.transform.position.y <= prePosition.y))
                    {
                        gameObject.SetActive(false);
                    }
                }

                break;

            case ObjectType.FRAGILE:



                break;
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
}
