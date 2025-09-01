using UnityEngine;

public class GoalLineManager : MonoBehaviour
{
    private Transform pointA; // 始点
    private Transform pointB; // 終点
    private LineRenderer lineRenderer;

    public void Initialize(Transform _pointA, Transform _pointB)
    {
        // LineRendererを追加
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // 線の太さ
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // マテリアル（デフォルトだと見えにくいので）
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new(0.99f, 0.42f, 0.41f, 1f);
        lineRenderer.endColor = new(0.99f, 0.42f, 0.41f, 1f);

        // 頂点数は2
        lineRenderer.positionCount = 2;

        // 2点の設定
        pointA = _pointA;
        pointB = _pointB;

        // 2点間を設定
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);
    }
}
