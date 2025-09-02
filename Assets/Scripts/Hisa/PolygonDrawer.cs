using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonDrawer : MonoBehaviour
{
    [Tooltip("ポリゴンの頂点 (時計回りまたは反時計回り)")]
    public Vector3[] vertices;

    private void Start()
    {
        DrawPolygon(vertices);
    }

    void DrawPolygon(Vector3[] verts)
    {
        if (verts.Length < 3)
        {
            Debug.LogWarning("ポリゴンを描画するには3つ以上の頂点が必要です");
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "PolygonMesh";

        // 頂点をセット
        mesh.vertices = verts;

        // 三角形インデックスを作成（簡易的にEar Clippingを使わない正多角形向け）
        int[] triangles = new int[(verts.Length - 2) * 3];
        for (int i = 0; i < verts.Length - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.triangles = triangles;

        // 法線とUVを自動計算
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // MeshFilterに適用
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;
    }
}
