using UnityEngine;

public class GoalLineManager : MonoBehaviour
{
    private Transform pointA; // �n�_
    private Transform pointB; // �I�_
    private LineRenderer lineRenderer;

    public void Initialize(Transform _pointA, Transform _pointB)
    {
        // LineRenderer��ǉ�
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // ���̑���
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // �}�e���A���i�f�t�H���g���ƌ����ɂ����̂Łj
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new(0.99f, 0.42f, 0.41f, 1f);
        lineRenderer.endColor = new(0.99f, 0.42f, 0.41f, 1f);

        // ���_����2
        lineRenderer.positionCount = 2;

        // 2�_�̐ݒ�
        pointA = _pointA;
        pointB = _pointB;

        // 2�_�Ԃ�ݒ�
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);
    }
}
