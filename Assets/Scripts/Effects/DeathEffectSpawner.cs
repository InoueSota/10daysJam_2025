using UnityEngine;

public class DeathEffectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab; // �~��Prefab�iSpriteRenderer�t���j
    [SerializeField] private int circleCount = 8;     // �o����
    [SerializeField] private float radius = 0.5f;     // �ŏ��̔��a
    [SerializeField] private float expandSpeed = 1f;  // �L����X�s�[�h
    [SerializeField] private float fadeDuration = 1f; // ������܂ł̎���

    public void SpawnEffect(Vector3 position)
    {
        for (int i = 0; i < circleCount; i++)
        {
            float angle = i * (360f / circleCount);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

            GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity);

            // �A�j���[�V�����p�ɃR���[�`���J�n
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

            // �O���ɍL����
            obj.transform.position = startPos + dir * (t * expandSpeed + radius);

            // �t�F�[�h�A�E�g
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = c;

            yield return null;
        }

        Destroy(obj);
    }
}
