using UnityEngine;

public class SmoothDampRotate : MonoBehaviour
{
    [SerializeField] private float targetAngle = 90f; // 目標角度（Y軸）
    [SerializeField] private float smoothTime = 0.5f; // 到達までのおおよその時間

    private float currentVelocity; // SmoothDamp用の角速度
    private bool isRotating;

    void Update()
    {
        if (isRotating)
        {
            float currentAngle = transform.eulerAngles.y;

            // Y軸を例にスムーズ回転
            float newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentVelocity, smoothTime);

            transform.rotation = Quaternion.Euler(0, newAngle, 0);

            // 目標付近まで来たら停止
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
