using DG.Tweening;
using UnityEngine;

public class TitlePlayer : MonoBehaviour
{
    // 自コンポーネント
    private SpriteRenderer spriteRenderer;

    // 大きさ
    private float size;
    // フラグ
    private bool isActivePlayer;
    // カウント
    [SerializeField] private bool leftToRight;

    [Header("移動速度")]
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float minMoveSpeed;

    [Header("インターバル")]
    [SerializeField] private int maxIntervalCount;
    [SerializeField] private int minIntervalCount;
    [SerializeField] private float intervalPower;
    private float intervalTimer;

    void Start()
    {
        // 自コンポーネントの取得
        spriteRenderer = GetComponent<SpriteRenderer>();

        size = transform.localScale.x;
        isActivePlayer = false;
    }

    void Update()
    {
        intervalTimer -= Time.deltaTime;

        if (!isActivePlayer && intervalTimer <= 0f)
        {
            Vector3 startPosition = Vector3.zero;
            Vector3 endPosition = Vector3.zero;

            if (leftToRight)
            {
                startPosition = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * Random.Range(0.1f, 0.9f), 0f));
                endPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * Random.Range(0.1f, 0.9f), 0f));
                startPosition.x -= size;
                endPosition.x += size;

                leftToRight = false;
            }
            else
            {
                startPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * Random.Range(0.1f, 0.9f), 0f));
                endPosition = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * Random.Range(0.1f, 0.9f), 0f));
                startPosition.x += size;
                endPosition.x -= size;

                leftToRight = true;
            }
            startPosition.z = 0f;
            endPosition.z = 0f;

            spriteRenderer.flipX = leftToRight;

            transform.position = startPosition;
            transform.DOMove(endPosition, Random.Range(minMoveSpeed, maxMoveSpeed)).SetEase(Ease.InSine).OnComplete(IntervalIntialize);
            isActivePlayer = true;
        }
    }

    void IntervalIntialize()
    {
        intervalTimer = Random.Range(minIntervalCount, maxIntervalCount) * intervalPower;
        isActivePlayer = false;
    }
}
