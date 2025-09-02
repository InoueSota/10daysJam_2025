using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラの映像を四角メッシュに貼り、ドラッグの「直線」で四角を2分割する。
/// ・対象はオルソカメラ前提（横幅=2*size*aspect / 縦=2*size）
/// ・直線が四角境界を2回横切るときのみ分割
/// ・分割後はUVを維持した2つのMeshを生成
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenMeshStraightSlicer : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Optional visuals")]
    public LineRenderer lineRenderer;       // 切る線の見た目（任意）
    public float lineZ = 0f;

    [Header("Quad plane")]
    public float zPlane = 0f;               // 四角メッシュのZ（2D平面）
    public bool hideOriginalAfterSlice = true;

    RenderTexture rt;
    MeshFilter mf;
    MeshRenderer mr;

    // スクリーン四角（ローカル座標・CCW）: 左下→右下→右上→左上
    Vector2[] rectLocal = new Vector2[4];

    // 入力
    bool dragging;
    Vector3 dragStartW, dragEndW;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        // RenderTexture セット
        rt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        targetCamera.targetTexture = rt;

        // マテリアル（Unlit/Texture）
        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = rt;
        mr.sharedMaterial = mat;

        // 四角メッシュを作成
        BuildQuadMesh();

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }
    }

    void OnDestroy()
    {
        if (targetCamera) targetCamera.targetTexture = null;
        if (rt) rt.Release();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            dragStartW = ScreenToWorldOnPlane(Input.mousePosition, zPlane);
            if (lineRenderer)
            {
                lineRenderer.positionCount = 2;
                var p = dragStartW; p.z = lineZ;
                lineRenderer.SetPosition(0, p);
                lineRenderer.SetPosition(1, p);
            }
        }
        if (dragging && Input.GetMouseButton(0))
        {
            dragEndW = ScreenToWorldOnPlane(Input.mousePosition, zPlane);
            if (lineRenderer)
            {
                var p0 = dragStartW; p0.z = lineZ;
                var p1 = dragEndW; p1.z = lineZ;
                lineRenderer.SetPosition(0, p0);
                lineRenderer.SetPosition(1, p1);
            }
        }
        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (lineRenderer) lineRenderer.positionCount = 0;

            TrySliceWithStraightLine(dragStartW, ScreenToWorldOnPlane(Input.mousePosition, zPlane));
        }
    }

    // カメラ座標→zPlane上ワールド
    Vector3 ScreenToWorldOnPlane(Vector3 screen, float z)
    {
        float dist = Mathf.Abs(targetCamera.transform.position.z - z);
        var w = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = z;
        return w;
    }

    void BuildQuadMesh()
    {
        // オルソサイズから矩形サイズを決定
        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;

        rectLocal[0] = new Vector2(-w / 2f, -h / 2f);
        rectLocal[1] = new Vector2(+w / 2f, -h / 2f);
        rectLocal[2] = new Vector2(+w / 2f, +h / 2f);
        rectLocal[3] = new Vector2(-w / 2f, +h / 2f);

        var verts = new Vector3[]
        {
            new Vector3(rectLocal[0].x, rectLocal[0].y, zPlane),
            new Vector3(rectLocal[1].x, rectLocal[1].y, zPlane),
            new Vector3(rectLocal[2].x, rectLocal[2].y, zPlane),
            new Vector3(rectLocal[3].x, rectLocal[3].y, zPlane),
        };
        var uvs = new Vector2[]
        {
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
        };
        var tris = new int[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new Mesh();
        mesh.name = "ScreenQuad";
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;
    }

    // 直線で矩形を切る → 2つの多角形に分割
    void TrySliceWithStraightLine(Vector3 w0, Vector3 w1)
    {
        // ローカルへ変換
        Vector2 a = transform.InverseTransformPoint(w0);
        Vector2 b = transform.InverseTransformPoint(w1);

        // 無限直線で矩形との交点を探す（必ず2つ返すよう拡張）
        if (!LineRectFullIntersections(a, b, rectLocal, out var hitA, out var hitB))
        {
            Debug.LogWarning("交点を2つ見つけられませんでした");
            return;
        }

        // --- 以降は同じ処理（arcAB / arcBA 作成 → メッシュ生成） ---
        var arcAB = BuildRectArc(rectLocal, hitA.edge, hitB.edge, hitA.point, hitB.point, true);
        var arcBA = BuildRectArc(rectLocal, hitA.edge, hitB.edge, hitA.point, hitB.point, false);

        var lineAB = new List<Vector2> { hitA.point, hitB.point };
        var lineBA = new List<Vector2>(lineAB); lineBA.Reverse();

        var poly1 = new List<Vector2>(); poly1.AddRange(arcAB); poly1.AddRange(lineBA);
        var poly2 = new List<Vector2>(); poly2.AddRange(arcBA); poly2.AddRange(lineAB);

        CreatePieceMesh("SlicePiece_A", poly1);
        CreatePieceMesh("SlicePiece_B", poly2);

        if (hideOriginalAfterSlice)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshFilter>().mesh = null;
        }
    }
    bool LineRectFullIntersections(Vector2 a, Vector2 b, Vector2[] rect, out Hit h0, out Hit h1)
    {
        h0 = default; h1 = default;
        var hits = new List<Hit>();

        for (int e = 0; e < 4; e++)
        {
            Vector2 p = rect[e];
            Vector2 q = rect[(e + 1) % 4];

            if (SegmentLineIntersection(p, q, a, b, out Vector2 ip))
            {
                hits.Add(new Hit { point = ip, edge = e });
            }
        }

        if (hits.Count >= 2)
        {
            h0 = hits[0];
            h1 = hits[1];
            return true;
        }
        return false;
    }

    struct Hit
    {
        public Vector2 point;   // 交点（ローカル）
        public int edge;        // 交差した辺のインデックス（0..3）: 0 L->R bottom, 1 right, 2 top, 3 left
    }

    // 無限直線 a-b と矩形辺の交差を前後順で2つ取得
    bool LineRectIntersections(Vector2 a, Vector2 b, Vector2[] rect, out Hit h0, out Hit h1)
    {
        h0 = default; h1 = default;
        var hits = new List<Hit>();

        for (int e = 0; e < 4; e++)
        {
            Vector2 p = rect[e];
            Vector2 q = rect[(e + 1) % 4];
            if (SegmentLineIntersection(p, q, a, b, out Vector2 ip))
            {
                hits.Add(new Hit { point = ip, edge = e });
                if (hits.Count == 2) { h0 = hits[0]; h1 = hits[1]; return true; }
            }
        }
        return false;
    }

    // 矩形辺(線分pq) と 無限直線ab の交差
    static bool SegmentLineIntersection(Vector2 p, Vector2 q, Vector2 a, Vector2 b, out Vector2 ip)
    {
        ip = default;
        Vector2 r = q - p;
        Vector2 s = b - a;
        float rxs = Cross(r, s);
        if (Mathf.Abs(rxs) < 1e-7f) return false; // 平行

        float t = Cross(a - p, s) / rxs;     // p + t r
        float u = Cross(a - p, r) / rxs;     // a + u s

        if (t >= 0f && t <= 1f) // 辺の範囲内
        {
            ip = p + t * r;
            return true;
        }
        return false;
    }

    static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    // 矩形の境界弧（交点A→B）: forwardCCW=true でCCW方向、falseでCW
    List<Vector2> BuildRectArc(Vector2[] rect, int edgeA, int edgeB, Vector2 ptA, Vector2 ptB, bool forwardCCW)
    {
        var arc = new List<Vector2>();
        arc.Add(ptA);
        int i = edgeA;

        while (true)
        {
            i = forwardCCW ? (i + 1) % 4 : (i - 1 + 4) % 4;
            int v = forwardCCW ? i : (i + 1) % 4;
            arc.Add(rect[v]);
            if (i == edgeB) break;
        }
        arc.Add(ptB);
        return arc;
    }

    // 凸多角形の三角化（中心点からの扇形）
    void CreatePieceMesh(string name, List<Vector2> polygon)
    {
        if (polygon.Count < 3) return;

        // UVは元四角の [0..1] に射影
        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;
        float minX = -w / 2f, minY = -h / 2f;

        // 頂点/UV
        int n = polygon.Count;
        var verts = new Vector3[n];
        var uvs = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            var p = polygon[i];
            verts[i] = new Vector3(p.x, p.y, zPlane);
            uvs[i] = new Vector2((p.x - minX) / w, (p.y - minY) / h);
        }

        // 扇形トライアングル
        List<int> tris = new List<int>((n - 2) * 3);
        for (int i = 1; i < n - 1; i++)
        {
            tris.Add(0); tris.Add(i); tris.Add(i + 1);
        }

        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var mf2 = go.AddComponent<MeshFilter>();
        var mr2 = go.AddComponent<MeshRenderer>();
        mf2.sharedMesh = new Mesh { name = name + "_Mesh" };
        mf2.sharedMesh.SetVertices(verts);
        mf2.sharedMesh.SetUVs(0, uvs);
        mf2.sharedMesh.SetTriangles(tris, 0);
        mf2.sharedMesh.RecalculateBounds();
        mf2.sharedMesh.RecalculateNormals();

        // 同じマテリアル（RenderTexture）
        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = rt;
        mr2.sharedMaterial = mat;
    }
}
