using DG.Tweening;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Position
    private Vector3 originPosition;

    [Header("�J�����ړ����x")]
    [SerializeField] private float floatRange;
    [SerializeField] private float addRotateValue;
    private float rotateValue;

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
        FloatCamera();
    }

    void FloatCamera()
    {
        rotateValue += addRotateValue * Time.deltaTime;

        Vector3 floatPosition = originPosition;
        floatPosition.x += Mathf.Cos(rotateValue) * floatRange;
        floatPosition.y += Mathf.Sin(rotateValue * 2f) * floatRange;
        transform.position = floatPosition;
    }

    // Setter
    public void ShakeCamera() { transform.DOShakePosition(shakeTime, shakePower, shakeCount); }
}
