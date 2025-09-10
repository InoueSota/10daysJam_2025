using UnityEngine;

public class TitlePlayer : MonoBehaviour
{


    [Header("インターバル")]
    [SerializeField] private int maxIntervalCount;
    [SerializeField] private int minIntervalCount;
    [SerializeField] private float intervalPower;
    private float intervalTimer;

    void Start()
    {
        
    }

    void Update()
    {

        intervalTimer -= Time.deltaTime;

        if (intervalTimer <= 0f)
        {

        }
    }

    void IntervalIntialize()
    {
        intervalTimer = Random.Range(minIntervalCount, maxIntervalCount) * intervalPower;
    }
}
