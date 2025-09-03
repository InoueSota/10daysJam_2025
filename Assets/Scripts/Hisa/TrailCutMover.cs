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
        var tf = transform; // Transform���L���b�V���i�j����̃A�N�Z�X�������j

        // ������
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

        // �@������~
        if (tr) tr.emitting = false;

        // �A1�t���[���҂iEditor���Q�Ƃ��X�V����P�\�j
        yield return new WaitForEndOfFrame();

        // �BTrail��������܂� �g��\�����������h �őҋ@�iInspector���G��Ȃ��悤�ɂ���j
        if (tr)
        {
            // �R���|�[�l���g�������iSceneView�̃n���h���`��̑Ώۂ���O���j
            tr.enabled = false;
        }
        // GO���̂���A�N�e�B�u���i����Ɉ��S�j
        gameObject.SetActive(false);

        // TrailRenderer�̎c�����ԂԂ�ҋ@�i���A���^�C���B�Q�[����~�ɂ������j
        float wait = tr ? Mathf.Max(0f, tr.time) : 0f;
        float end = Time.realtimeSinceStartup + wait;
        while (Time.realtimeSinceStartup < end) yield return null;

        // �C���S�ɔj��
        if (this) Destroy(gameObject);
    }
}
