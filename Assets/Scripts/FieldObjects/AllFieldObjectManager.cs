using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{

    //���C���[���Ƃ̐F
    [SerializeField] Color[] layerColor = new Color[3];

    // �Y������ObjectType
    public enum ObjectType
    {
        GROUND,
        GOAL,
        BLOCK
    }
    [SerializeField] private ObjectType objectType;

    // �Y������\�����
    public enum Status
    {
        FIRST,
        SECOND,
        THIRD
    }
    private Status status = Status.FIRST;

    // ���R���|�[�l���g
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;

    // �y�[�W�̃��C���[�ԍ�
    private int page1Layer;
    private int page2Layer;
    private int page3Layer;

    void Start()
    {
        // ���R���|�[�l���g�̎擾
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // �y�[�W�̃��C���[�ԍ���ݒ�
        page1Layer = 6;
        page2Layer = 7;
        page3Layer = 8;

        // ObjectType�ɂ���ď�����ύX
        AfterChangeLayer();
    }

    /// <summary>
    /// �j��ꂽ���Ƃ̏���
    /// </summary>
    public void HitTear()
    {
        switch (objectType)
        {
            case ObjectType.GROUND:

                switch (status)
                {
                    // �őO�ʂɂ���
                    case Status.SECOND:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;
                        break;

                    // �P�O�ʂɂ���
                    case Status.THIRD:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                        status = Status.SECOND;
                        break;
                }

                break;
            case ObjectType.GOAL:

                switch (status)
                {
                    // �őO�ʂ̂Ƃ��͏�������
                    case Status.FIRST:
                        Destroy(gameObject);
                        break;

                    // �őO�ʂɂ���
                    case Status.SECOND:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;
                        break;

                    // �P�O�ʂɂ���
                    case Status.THIRD:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                        status = Status.SECOND;
                        break;
                }

                break;
            case ObjectType.BLOCK:

                switch (status)
                {
                    // �őO�ʂ̂Ƃ��͏�������
                    case Status.FIRST:
                        Destroy(gameObject);
                        break;

                    // �őO�ʂɂ���
                    case Status.SECOND:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;
                        break;

                    // �P�O�ʂɂ���
                    case Status.THIRD:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                        status = Status.SECOND;
                        break;
                }

                break;
        }
    }

    /// <summary>
    /// ���C���[��؂�ւ����Ƃ��̏���
    /// </summary>]
    public void AfterChangeLayer()
    {
        switch (objectType)
        {
            // GROUND�̓y�[�W���ɂ���ē����蔻��̗L���Ƃ���ɔ����\����ύX����
            case ObjectType.GROUND:

                // �ǂ̃y�[�W�ł��F���܂��߂�
                spriteRenderer.color = Color.black;

                // �y�[�W1�̂Ƃ�
                if (transform.parent.gameObject.layer == page1Layer)
                {
                    status = Status.FIRST;
                }
                // �y�[�W2�̂Ƃ�
                else if (transform.parent.gameObject.layer == page2Layer)
                {
                    if (transform.parent.parent.GetChild(transform.parent.GetSiblingIndex() - 1).GetComponent<PageManager>())
                    {
                        
                    }

                    status = Status.SECOND;
                    // �������ɂ���
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    // �����蔻����ꎞ�I�ɖ�����
                    boxCollider2D.enabled = false;
                }
                // �y�[�W3�̂Ƃ�
                else if (transform.parent.gameObject.layer == page3Layer)
                {
                    status = Status.SECOND;
                    // �������ɂ���
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.02f);
                    // �����蔻����ꎞ�I�ɖ�����
                    boxCollider2D.enabled = false;
                }

                break;

            // GOAL�̓y�[�W���ɂ���ē����蔻��̗L���Ƃ���ɔ����\����ύX����
            case ObjectType.GOAL:

                // �ǂ̃y�[�W�ł��F���܂��߂�
                spriteRenderer.color = Color.white;

                // �y�[�W1�̂Ƃ�
                if (transform.parent.gameObject.layer == page1Layer)
                {
                    status = Status.FIRST;
                    // �����蔻����ꎞ�I�ɂ���
                    boxCollider2D.enabled = true;

                    foreach (GameObject goalLine in GameObject.FindGameObjectsWithTag("GoalLine")) { goalLine.GetComponent<GoalLineManager>().SetAlpha(1f); }
                }
                // �y�[�W2�̂Ƃ�
                else if (transform.parent.gameObject.layer == page2Layer)
                {
                    status = Status.SECOND;
                    // �������ɂ���
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    // �����蔻����ꎞ�I�ɖ�����
                    boxCollider2D.enabled = false;

                    foreach (GameObject goalLine in GameObject.FindGameObjectsWithTag("GoalLine")) { goalLine.GetComponent<GoalLineManager>().SetAlpha(0.2f); }
                }
                // �y�[�W3�̂Ƃ�
                else if (transform.parent.gameObject.layer == page3Layer)
                {
                    status = Status.THIRD;
                    // �������ɂ���
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.02f);
                    // �����蔻����ꎞ�I�ɖ�����
                    boxCollider2D.enabled = false;

                    foreach (GameObject goalLine in GameObject.FindGameObjectsWithTag("GoalLine")) { goalLine.GetComponent<GoalLineManager>().SetAlpha(0.02f); }
                }

                break;

            // BLOCK�̓y�[�W���ɂ���ē����蔻��̗L���Ƃ���ɔ����\����ύX����
            case ObjectType.BLOCK:

                // �ǂ̃y�[�W�ł��F���܂��߂�
                spriteRenderer.color = layerColor[0];

                // �y�[�W1�̂Ƃ�
                if (transform.parent.gameObject.layer == page1Layer)
                {
                    status = Status.FIRST;
                    // �����蔻����ꎞ�I�ɂ���
                    boxCollider2D.enabled = true;
                }
                // �y�[�W2�̂Ƃ�
                else if (transform.parent.gameObject.layer == page2Layer)
                {
                    status = Status.SECOND;
                    // �������ɂ���
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    // �����蔻����ꎞ�I�ɖ�����
                    boxCollider2D.enabled = false;
                }
                // �y�[�W3�̂Ƃ�
                else if (transform.parent.gameObject.layer == page3Layer)
                {
                    status = Status.THIRD;
                    // �������ɂ���
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.02f);
                    // �����蔻����ꎞ�I�ɖ�����
                    boxCollider2D.enabled = false;
                }

                break;
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
    public Status GetStatus() { return status; }
}
