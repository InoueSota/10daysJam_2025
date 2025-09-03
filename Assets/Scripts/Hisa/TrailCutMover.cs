using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailCutMover : MonoBehaviour
{
    public AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);

    public void Play(Vector3 from, Vector3 to, float dur)
    {
        StartCoroutine(PlayRoutine(from, to, Mathf.Max(0.0001f, dur)));
    }

    IEnumerator PlayRoutine(Vector3 from, Vector3 to, float dur)
    {
        var tr = GetComponent<TrailRenderer>();
        var tf = transform; // Transformをキャッシュ（破棄後のアクセスを避ける）

        // 初期化
        if (tr) { tr.Clear(); tr.emitting = true; }
        tf.position = from;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float u = ease.Evaluate(Mathf.Clamp01(t));
            tf.position = Vector3.LerpUnclamped(from, to, u);
            yield return null;
        }
        tf.position = to;

        // ①発光停止
        if (tr) tr.emitting = false;

        // ②1フレーム待つ（Editorが参照を更新する猶予）
        yield return new WaitForEndOfFrame();

        // ③Trailが消えるまで “非表示＆無効化” で待機（Inspectorが触らないようにする）
        if (tr)
        {
            // コンポーネント無効化（SceneViewのハンドル描画の対象から外す）
            tr.enabled = false;
        }
        // GO自体も非アクティブ化（さらに安全）
        gameObject.SetActive(false);

        // TrailRendererの残像時間ぶん待機（リアルタイム。ゲーム停止にも強い）
        float wait = tr ? Mathf.Max(0f, tr.time) : 0f;
        float end = Time.realtimeSinceStartup + wait;
        while (Time.realtimeSinceStartup < end) yield return null;

        // ④安全に破棄
        if (this) Destroy(gameObject);
    }
}
