using UnityEngine;

public class ChangeLayerManager : MonoBehaviour
{
    private enum Status
    {
        DEFAULT,
        ACTIVE
    }
    private Status status = Status.DEFAULT;

    // Default Parameter
    private Vector3 defaultPosition;
    private Vector3 defaultRotation;

    [Header("Camera Parameter")]
    [SerializeField] private Vector3 changePosition;
    [SerializeField] private Vector3 changeRotation;
    private Camera thisCamera;

    [Header("Select Parameter")]
    [SerializeField] private GameObject selectLineObj;
    [SerializeField] private int selectNum;
    private LineRenderer selectLineRenderer;
    private bool isSelect;

    // Choice Parameter
    private Transform choiseTransform;
    private bool isChoice;

    [Header("Layer Parameter")]
    [SerializeField] private PlayerController controller;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private float diffValue;
    private Transform[] pagesTransform;

    void Start()
    {
        thisCamera = GetComponent<Camera>();

        selectLineRenderer = selectLineObj.GetComponent<LineRenderer>();

        pagesTransform = new Transform[gridTransform.childCount];

        defaultPosition = transform.position;
        defaultRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        // ���C���[�ύX����̊J�n�^�I��
        ChangeLayerActive();
        switch (status)
        {
            case Status.ACTIVE:

                // ���C���[�I�𑀍�
                SelectLayer();
                // ���C���[�ύX����
                ChangeLayer();

                break;
        }
    }
    void ChangeLayerActive()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            selectNum = 1;

            switch (status)
            {
                case Status.DEFAULT:

                    selectLineObj.SetActive(true);

                    for (int i = 0; i < pagesTransform.Length; i++) { pagesTransform[i] = gridTransform.GetChild(i).transform; }

                    selectLineObj.transform.position = new(0f, 0f, pagesTransform[selectNum].transform.position.z);
                    selectLineRenderer.SetPosition(0, new(-10f, 13.5f, pagesTransform[selectNum].transform.position.z));
                    selectLineRenderer.SetPosition(1, new(10f, 13.5f, pagesTransform[selectNum].transform.position.z));
                    selectLineRenderer.SetPosition(2, new(10f, -1.5f, pagesTransform[selectNum].transform.position.z));
                    selectLineRenderer.SetPosition(3, new(-10f, -1.5f, pagesTransform[selectNum].transform.position.z));

                    // Camera�̋���
                    transform.position = changePosition;
                    transform.rotation = Quaternion.Euler(changeRotation);
                    thisCamera.orthographicSize = 12f;

                    // ���C���[�̋���
                    for (int i = 0; i < pagesTransform.Length; i++)
                    {
                        pagesTransform[i].transform.localPosition = new(pagesTransform[i].transform.localPosition.x, pagesTransform[i].transform.localPosition.y, pagesTransform[i].transform.localPosition.z + diffValue * (i - pagesTransform.Length * 0.5f));
                    }

                    // �v���C���[�̋���
                    controller.SetDefault();

                    // Status�J��
                    status = Status.ACTIVE;

                    break;
                case Status.ACTIVE:

                    selectLineObj.SetActive(false);
                    selectLineObj.transform.position = new(0f, 0f, 0f);
                    selectLineRenderer.SetPosition(0, new(-10f, 13.5f, 0f));
                    selectLineRenderer.SetPosition(1, new(10f, 13.5f, 0f));
                    selectLineRenderer.SetPosition(2, new(10f, -1.5f, 0f));
                    selectLineRenderer.SetPosition(3, new(-10f, -1.5f, 0f));

                    // Camera�̋���
                    transform.position = defaultPosition;
                    transform.rotation = Quaternion.Euler(defaultRotation);
                    thisCamera.orthographicSize = 7.5f;

                    // ���C���[�̋���
                    for (int i = 0; i < pagesTransform.Length; i++)
                    {
                        pagesTransform[i].transform.localPosition = Vector3.zero;
                    }

                    // �v���C���[�̋���
                    controller.SetDefault();

                    // Status�J��
                    status = Status.DEFAULT;

                    break;
            }
        }
    }
    void SelectLayer()
    {
        if (!isSelect && !isChoice)
        {
            if (Input.GetAxisRaw("Horizontal2") < 0f)
            {
                selectNum--;
                selectNum = Mathf.Clamp(selectNum, 1, selectNum);
            }
            else if (Input.GetAxisRaw("Horizontal2") > 0f)
            {
                selectNum++;
                selectNum = Mathf.Clamp(selectNum, selectNum, 2);
            }
            selectLineObj.transform.position = new(0f, 0f, pagesTransform[selectNum].transform.position.z);
            selectLineRenderer.SetPosition(0, new(-10f, 13.5f, pagesTransform[selectNum].transform.position.z));
            selectLineRenderer.SetPosition(1, new(10f, 13.5f, pagesTransform[selectNum].transform.position.z));
            selectLineRenderer.SetPosition(2, new(10f, -1.5f, pagesTransform[selectNum].transform.position.z));
            selectLineRenderer.SetPosition(3, new(-10f, -1.5f, pagesTransform[selectNum].transform.position.z));

            isSelect = true;
        }

        if (isSelect && !isChoice && Input.GetAxisRaw("Horizontal2") == 0f) { isSelect = false; }
    }
    void ChangeLayer()
    {
        if (isChoice)
        {
            if (!isSelect)
            {
                if (Input.GetAxisRaw("Horizontal2") < 0f)
                {
                    if (selectNum > 1)
                    {
                        selectNum--;
                        selectNum = Mathf.Clamp(selectNum, 1, selectNum);

                        // ���C���[����ւ�
                        choiseTransform.parent.gameObject.layer--;
                        pagesTransform[selectNum].transform.parent.gameObject.layer++;

                        // �ꏊ����ւ�
                        Vector3 preChoisePosition = choiseTransform.position;
                        choiseTransform.position = pagesTransform[selectNum].transform.position;
                        pagesTransform[selectNum].transform.position = preChoisePosition;

                        // Transform����ւ�
                        Transform tmpTransform = pagesTransform[selectNum + 1];
                        pagesTransform[selectNum + 1] = pagesTransform[selectNum];
                        pagesTransform[selectNum] = tmpTransform;
                    }
                }
                else if (Input.GetAxisRaw("Horizontal2") > 0f)
                {
                    if (selectNum < 2)
                    {
                        selectNum++;
                        selectNum = Mathf.Clamp(selectNum, selectNum, 2);

                        // ���C���[����ւ�
                        choiseTransform.parent.gameObject.layer++;
                        pagesTransform[selectNum].transform.parent.gameObject.layer--;

                        // �ꏊ����ւ�
                        Vector3 preChoisePosition = choiseTransform.position;
                        choiseTransform.position = pagesTransform[selectNum].transform.position;
                        pagesTransform[selectNum].transform.position = preChoisePosition;

                        // Transform����ւ�
                        Transform tmpTransform = pagesTransform[selectNum - 1];
                        pagesTransform[selectNum - 1] = pagesTransform[selectNum];
                        pagesTransform[selectNum] = tmpTransform;
                    }
                }
                isSelect = true;
            }

            if (isSelect && Input.GetAxisRaw("Horizontal2") == 0f) { isSelect = false; }

            if (Input.GetButtonDown("Jump"))
            {
                isChoice = false;
            }
        }

        if (!isChoice && Input.GetButtonDown("Jump"))
        {
            choiseTransform = pagesTransform[selectNum];
            isChoice = true;
        }
    }

    // Getter
    public bool GetIsActive()
    {
        switch (status)
        {
            case Status.DEFAULT:
                return false;

            case Status.ACTIVE:
                return true;
        }
        return false;
    }
}
