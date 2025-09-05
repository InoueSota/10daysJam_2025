using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    [SerializeField, Header("�����_�������������݂̂�")]
    bool once = false;

    [SerializeField, Header("�����_���̎��")]
    RandomMode randomMode = RandomMode.Range; // �� �V�����ǉ�

    Vector3 randomAxis;
    public Vector3 rotateSpeed;
    public Vector3 rotate;

    private void Start()
    {
        randomAxis = GetRandomAxis();
    }

    void Update()
    {
        if (!once)
        {
            randomAxis = GetRandomAxis();
        }

        rotate = randomAxis.normalized;
        rotate.x *= rotateSpeed.x;
        rotate.y *= rotateSpeed.y;
        rotate.z *= rotateSpeed.z;

        // ��]
        transform.Rotate(rotate * Time.deltaTime);
    }

    /// <summary>
    /// �����_�������擾
    /// </summary>
    private Vector3 GetRandomAxis()
    {
        if (randomMode == RandomMode.Range)
        {
            // -1�`1 �͈̔�
            return new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
        }
        else
        {
            // -1 or 1 �̂�
            return new Vector3(
                Random.value < 0.5f ? -1f : 1f,
                Random.value < 0.5f ? -1f : 1f,
                Random.value < 0.5f ? -1f : 1f
            ).normalized;
        }
    }

    // �����_���̎��
    private enum RandomMode
    {
        Range,   // -1 ~ 1
        Binary   // -1 or 1
    }
}
