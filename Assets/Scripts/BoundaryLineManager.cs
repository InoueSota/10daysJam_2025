using UnityEngine;

public class BoundaryLineManager : MonoBehaviour
{
    // Player
    private Transform playerTransform;

    [Header("Speed Value")]
    [SerializeField] private float chasePower;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        CalcPosition();
    }

    void CalcPosition()
    {
        float targetPosition = Mathf.RoundToInt(playerTransform.position.x);
        float currentPosition = (targetPosition - transform.position.x) * (chasePower * Time.deltaTime);
        transform.position = new(transform.position.x + currentPosition, transform.position.y);
    }
}
