using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public bool once = false;
    Vector3 randomAxis;
    private void Start()
    {
        // �����_���ȉ�]���i-1�`1�͈̔́j
        randomAxis = new Vector3(
           Random.Range(-1f, 1f),
           Random.Range(-1f, 1f),
           Random.Range(-1f, 1f)
       ).normalized;
    }

    void Update()
    {
        if (!once)
        {
            randomAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        }

        // ��]
        transform.Rotate(randomAxis * rotationSpeed * Time.deltaTime);
    }
}
