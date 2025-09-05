using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    [SerializeField, Header("ランダムを初期化時のみに")]
    bool once = false;

    [SerializeField, Header("ランダムの種類")]
    RandomMode randomMode = RandomMode.Range; // ← 新しく追加

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

        // 回転
        transform.Rotate(rotate * Time.deltaTime);
    }

    /// <summary>
    /// ランダム軸を取得
    /// </summary>
    private Vector3 GetRandomAxis()
    {
        if (randomMode == RandomMode.Range)
        {
            // -1～1 の範囲
            return new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
        }
        else
        {
            // -1 or 1 のみ
            return new Vector3(
                Random.value < 0.5f ? -1f : 1f,
                Random.value < 0.5f ? -1f : 1f,
                Random.value < 0.5f ? -1f : 1f
            ).normalized;
        }
    }

    // ランダムの種類
    private enum RandomMode
    {
        Range,   // -1 ~ 1
        Binary   // -1 or 1
    }
}
