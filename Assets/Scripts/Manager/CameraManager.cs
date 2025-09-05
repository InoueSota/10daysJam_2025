using DG.Tweening;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // ���_
    private Vector3 originPosition;

    // �`�����Ă��邩
    private bool isPeek;

    [Header("�J�����ړ����x")]
    [SerializeField] private float floatRange;
    [SerializeField] private float addRotateValue;
    private float rotateValue;

    [Header("�J�����`�����x")]
    [SerializeField] private float peekPower;
    [SerializeField] private float peekRange;

    [Header("�J�����V�F�C�N")]
    [SerializeField] private float shakeTime;
    [SerializeField] private float shakePower;
    [SerializeField] private int shakeCount;

    void Start()
    {
        originPosition = transform.position;
    }

    void Update()
    {
        // �J�����X�V����
        CameraUpdate();
    }

    /// <summary>
    /// �J�����X�V����
    /// </summary>
    void CameraUpdate()
    {
        rotateValue += addRotateValue * Time.deltaTime;

        Vector3 floatPosition = Vector3.zero;

        if (Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f)
        {
            floatPosition = originPosition;
        }
        floatPosition.x += Mathf.Cos(rotateValue) * floatRange;
        floatPosition.y += Mathf.Sin(rotateValue * 2f) * floatRange;

        if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
        {
            Vector3 peekPosition = originPosition;
            peekPosition.x += Input.GetAxisRaw("Horizontal") * peekRange;
            peekPosition.y += Input.GetAxisRaw("Vertical") * peekRange;

            transform.position = transform.position + (peekPosition + floatPosition - transform.position) * (peekPower * Time.deltaTime);
        }
        else
        {
            transform.position = transform.position + (floatPosition - transform.position) * (peekPower * Time.deltaTime);
        }
    }

    // Setter
    public void ShakeCamera() { transform.DOShakePosition(shakeTime, shakePower, shakeCount); }
}
