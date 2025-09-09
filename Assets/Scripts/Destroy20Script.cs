using UnityEngine;

public class Destroy20Script : MonoBehaviour
{
    float destroyTime = 20;

    void Update()
    {
        destroyTime -= Time.deltaTime;
        if (destroyTime < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
