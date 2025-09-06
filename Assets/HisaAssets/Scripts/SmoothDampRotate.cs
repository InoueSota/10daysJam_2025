using UnityEngine;

public class SmoothDampRotate : MonoBehaviour
{
    [SerializeField] private float targetAngle = 90f; // �ڕW�p�x�iY���j
    [SerializeField] private float smoothTime = 0.5f; // ���B�܂ł̂����悻�̎���

    private float currentVelocity; // SmoothDamp�p�̊p���x
    private bool isRotating;

    void Update()
    {
        if (isRotating)
        {
            float currentAngle = transform.eulerAngles.y;

            // Y�����ɃX���[�Y��]
            float newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentVelocity, smoothTime);

            transform.rotation = Quaternion.Euler(0, newAngle, 0);

            // �ڕW�t�߂܂ŗ������~
            if (Mathf.Abs(Mathf.DeltaAngle(newAngle, targetAngle)) < 0.1f)
            {
                transform.rotation = Quaternion.Euler(0, targetAngle, 0);
                isRotating = false;
            }
        }

       
    }

    public void StartRotation(float angle)
    {
        targetAngle = angle;
        currentVelocity = 0f;
        isRotating = true;
    }
}
