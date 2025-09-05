using UnityEngine;

public class DeathEffectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab; // 円のPrefab（SpriteRenderer付き）
    [SerializeField] private int circleCount = 8;     // 出す数
    [SerializeField] private float radius = 0.5f;     // 最初の半径
    [SerializeField] private float expandSpeed = 1f;  // 広がるスピード
    [SerializeField] private float fadeDuration = 1f; // 消えるまでの時間

    public void SpawnEffect(Vector3 position)
    {
        for (int i = 0; i < circleCount; i++)
        {
            float angle = i * (360f / circleCount);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

            GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity);

            // アニメーション用にコルーチン開始
            StartCoroutine(ExpandAndFade(circle, dir));
        }
    }

    private System.Collections.IEnumerator ExpandAndFade(GameObject obj, Vector3 dir)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Color c = sr.color;

        float t = 0f;
        Vector3 startPos = obj.transform.position;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            // 外側に広がる
            obj.transform.position = startPos + dir * (t * expandSpeed + radius);

            // フェードアウト
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = c;

            yield return null;
        }

        Destroy(obj);
    }
}
