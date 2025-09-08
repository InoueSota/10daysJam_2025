using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class LaserLineManager : MonoBehaviour
{
    private Transform pointA; // 始点
    private Transform pointB; // 終点
    private LineRenderer lineRenderer;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask characterLayer;

    public void Initialize(Transform _pointA, Transform _pointB, float alpha)
    {
        // LineRendererを追加
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // 線の太さ
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // マテリアル（デフォルトだと見えにくいので）
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new(0.32f, 0.54f, 1f, 1f);
        lineRenderer.endColor = new(0.32f, 0.54f, 1f, 1f);

        // 頂点数は2
        lineRenderer.positionCount = 2;

        // 2点の設定
        pointA = _pointA;
        pointB = _pointB;

        //レイヤー指定
        lineRenderer.sortingOrder = 40;

        // 透明度の設定
        SetAlpha(alpha);
    }
    public void SetAlpha(float alpha)
    {
        // 現在の色を取得
        Color start = lineRenderer.startColor;
        Color end = lineRenderer.endColor;

        // alphaを変更
        start.a = alpha;
        end.a = alpha;

        // 設定
        lineRenderer.startColor = start;
        lineRenderer.endColor = end;
    }

    void Update()
    {
        if (!pointA.gameObject.activeSelf || !pointB.gameObject.activeSelf) { Destroy(gameObject); }

        // 2点間を設定
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);

        foreach (RaycastHit2D hit in Physics2D.LinecastAll(pointA.position, pointB.position, groundLayer))
        {
            // TagがFieldObjectなら
            if (hit && hit.collider.gameObject.CompareTag("FieldObject") && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.LASER)
            {
                hit.collider.gameObject.SetActive(false);
            }
        }

        // プレイヤーが触れたか判定
        RaycastHit2D hitP = Physics2D.Linecast(pointA.position, pointB.position, characterLayer);

        if (hitP.collider != null)
        {
            hitP.collider.GetComponent<PlayerManager>().SetDeath(hitP.point, true);
        }
    }
}
