using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public bool once = false;
    Vector3 randomAxis;
    private void Start()
    {
        // ƒ‰ƒ“ƒ_ƒ€‚È‰ñ“]²i-1`1‚Ì”ÍˆÍj
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

        // ‰ñ“]
        transform.Rotate(randomAxis * rotationSpeed * Time.deltaTime);
    }
}
