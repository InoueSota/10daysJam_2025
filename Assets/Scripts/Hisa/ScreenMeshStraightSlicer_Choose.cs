using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラ映像を四角メッシュに貼り、ドラッグした直線で何度でも分割。
/// 毎回、クリック or ←/→ で残す側を選択。落とす側はRigidbody2Dで“ひらり”＋紙吹雪。
/// - 直線は自動で境界まで延長（端→端）
/// - 復活バグ対策：currentPoly を常に更新して再描画
/// - クリック判定は Collider2D.OverlapPoint を使用
/// - CCW 正規化で表裏の向きを安定化
/// - 両面描画 option: メッシュを表裏複製（doubleSided）
///
/// 使い方：空のGameObjectにアタッチ。TargetCamera をセット（その映像が貼られる）
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenMeshStraightSlicer_AllInOne : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Input visuals (optional)")]
    public LineRenderer lineRenderer;      // 切る線の可視化（任意）
    public float lineZ = 0f;

    [Header("Plane & Material")]
    public float zPlane = 0f;              // 2D平面（Z）
    public bool doubleSided = true;        // ★両面描画（メッシュ複製）するか
    public Color pieceTint = Color.white;  // ピースの色（半透明にしたいならA<1）

    [Header("Drop effect")]
    public float dropTorque = 3f;
    public Vector2 dropImpulse = new Vector2(0.5f, 0.8f);
    public float dropDestroyAfter = 5f;

    [Header("Confetti")]
    public bool confettiOnDrop = true;
    public Gradient confettiColorOverLifetime;
    public Vector2 confettiCountRange = new Vector2(30, 50);
    public float confettiLife = 1.8f;
    public float confettiGravity = 0.6f;

    // 内部
    RenderTexture rt;
    MeshFilter mf;
    MeshRenderer mr;

    // 現在の多角形（ローカル座標 / CCW）
    List<Vector2> currentPoly = new List<Vector2>();

    // 入力管理
    bool dragging;
    Vector3 dragStartW, dragEndW;
    Vector2 lastCutDirLocal;

    // 選択待ち
    bool awaitingChoice = false;
    GameObject tempA, tempB;
    List<Vector2> polyA, polyB; // ローカル
    Material sharedMat;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        // RenderTexture 準備
        rt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        targetCamera.targetTexture = rt;

        // Unlit/Texture を使用（両面描画はメッシュ複製で実現）
        sharedMat = new Material(Shader.Find("Unlit/Texture"));
        sharedMat.mainTexture = rt;
        sharedMat.color = pieceTint;
        mr.sharedMaterial = sharedMat;

        // 初期四角形（オルソカメラ基準）
        BuildInitialQuadAsPolygon();
        EnsureCCW(currentPoly);
        RebuildMainMesh(currentPoly);

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }

        // デフォルトの紙吹雪用グラデ
        if (confettiColorOverLifetime == null || confettiColorOverLifetime.colorKeys.Length == 0)
        {
            var g = new GradientColorKey[] {
                new GradientColorKey(new Color(1f,1f,1f), 0f),
                new GradientColorKey(new Color(1f,0.9f,0.6f), 1f)
            };
            var a = new GradientAlphaKey[] {
                new GradientAlphaKey(1f,0f),
                new GradientAlphaKey(0f,1f)
            };
            confettiColorOverLifetime = new Gradient();
            confettiColorOverLifetime.SetKeys(g, a);
        }
    }

    void OnDestroy()
    {
        if (targetCamera) targetCamera.targetTexture = null;
        if (rt) rt.Release();
    }

    void Update()
    {
        if (awaitingChoice)
        {
            // ← / → キー選択
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                var ca = Centroid(polyA);
                var cb = Centroid(polyB);
                Vector2 n = new Vector2(-lastCutDirLocal.y, lastCutDirLocal.x); // 右法線
                float sideA = Vector2.Dot(ca, n);
                float sideB = Vector2.Dot(cb, n);
                bool rightIsA = sideA > sideB;

                bool chooseRight = Input.GetKeyDown(KeyCode.RightArrow);
                bool chooseA = (chooseRight && rightIsA) || (!chooseRight && !rightIsA);
                ChooseAndFinalize(chooseA ? tempA : tempB, chooseA ? polyA : polyB,
                                  chooseA ? tempB : tempA);
                return;
            }

            // クリック選択（OverlapPoint）
            if (Input.GetMouseButtonDown(0))
            {
                var w = ScreenToWorldOnPlane(Input.mousePosition, zPlane);
                var colA = tempA.GetComponent<Collider2D>();
                var colB = tempB.GetComponent<Collider2D>();

                bool hitA = colA && colA.OverlapPoint(w);
                bool hitB = colB && colB.OverlapPoint(w);

                if (hitA ^ hitB)
                {
                    ChooseAndFinalize(hitA ? tempA : tempB, hitA ? polyA : polyB,
                                      hitA ? tempB : tempA);
                }
                else if (hitA && hitB)
                {
                    // 線上などで両方ヒット → 重心に近い方
                    Vector2 la = transform.InverseTransformPoint(w);
                    float da = (la - Centroid(polyA)).sqrMagnitude;
                    float db = (la - Centroid(polyB)).sqrMagnitude;
                    bool chooseA = da <= db;
                    ChooseAndFinalize(chooseA ? tempA : tempB, chooseA ? polyA : polyB,
                                      chooseA ? tempB : tempA);
                }
            }
            return;
        }

        // 通常ドラッグ
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

    // ========= 初期形状 / メインメッシュ =========

    void BuildInitialQuadAsPolygon()
    {
        // オルソサイズから矩形サイズ
        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;

        currentPoly.Clear();
        // 左下→右下→右上→左上（CCW）
        currentPoly.Add(new Vector2(-w / 2f, -h / 2f));
        currentPoly.Add(new Vector2(+w / 2f, -h / 2f));
        currentPoly.Add(new Vector2(+w / 2f, +h / 2f));
        currentPoly.Add(new Vector2(-w / 2f, +h / 2f));
    }

    void RebuildMainMesh(List<Vector2> poly)
    {
        EnsureCCW(poly);

        var (verts, uvs, tris) = BuildMeshDataFromPoly(poly);

        var mesh = new Mesh();
        mesh.name = "MainPiece_Mesh";
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        if (doubleSided) MakeDoubleSidedMesh(mesh);

        mf.sharedMesh = mesh;
        mr.sharedMaterial = sharedMat;
        mr.enabled = true;
    }

    // ========= カット処理 =========

    void TrySliceWithStraightLine(Vector3 w0, Vector3 w1)
    {
        Vector2 a = transform.InverseTransformPoint(w0);
        Vector2 b = transform.InverseTransformPoint(w1);

        if (!LinePolygonIntersections(a, b, currentPoly, out var hitA, out var hitB))
            return; // 交差しなければ無視

        lastCutDirLocal = (hitB.point - hitA.point).normalized;

        // 弧＋直線で2多角形
        var arcAB = BuildPolyArc(currentPoly, hitA.edgeIndex, hitB.edgeIndex, hitA.point, hitB.point, true);
        var arcBA = BuildPolyArc(currentPoly, hitA.edgeIndex, hitB.edgeIndex, hitA.point, hitB.point, false);

        var lineAB = new List<Vector2> { hitA.point, hitB.point };
        var lineBA = new List<Vector2>(lineAB); lineBA.Reverse();
        Debug.DrawLine(hitA.point, hitB.point, Color.red, 2f);
        polyA = new List<Vector2>(arcAB.Count + lineBA.Count);
        polyA.AddRange(arcAB); polyA.AddRange(lineBA);
        EnsureCCW(polyA);

        polyB = new List<Vector2>(arcBA.Count + lineAB.Count);
        polyB.AddRange(arcBA); polyB.AddRange(lineAB);
        EnsureCCW(polyB);

        // 一時ピース表示（半透明）
        (tempA, tempB) = (CreateTempPiece("Choice_A", polyA), CreateTempPiece("Choice_B", polyB));

        // 親の表示を消す（復活防止：確定時に必ずcurrentPolyから再生成）
        mr.enabled = false;
        mf.mesh = null;

        awaitingChoice = true;
    }

    void ChooseAndFinalize(GameObject keepGO, List<Vector2> keepPoly, GameObject dropGO)
    {
        // 1) 残す側に確定 → currentPoly 更新＆再描画
        currentPoly = new List<Vector2>(keepPoly);
        RebuildMainMesh(currentPoly);

        // 2) 落とす側“ひらり”
        dropGO.transform.SetParent(null, true);
        var col = dropGO.GetComponent<PolygonCollider2D>();
        if (!col) col = dropGO.AddComponent<PolygonCollider2D>();
        col.isTrigger = false;

        var rb = dropGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.AddForce(new Vector2(Random.Range(-dropImpulse.x, dropImpulse.x), dropImpulse.y), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-dropTorque, dropTorque), ForceMode2D.Impulse);

        if (confettiOnDrop) AddConfetti(dropGO);
        if (dropDestroyAfter > 0f) Destroy(dropGO, dropDestroyAfter);

        // 3) 一時ピース片方は不要
        Destroy(keepGO);
        tempA = tempB = null;
        awaitingChoice = false;
    }

    // ========= 一時ピース =========

    GameObject CreateTempPiece(string name, List<Vector2> poly)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var mf2 = go.AddComponent<MeshFilter>();
        var mr2 = go.AddComponent<MeshRenderer>();

        var (verts, uvs, tris) = BuildMeshDataFromPoly(poly);
        var mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        if (doubleSided) MakeDoubleSidedMesh(mesh);
        mf2.sharedMesh = mesh;

        var mat = new Material(sharedMat);
        var c = pieceTint; c.a = 0.8f; // 半透明
        mat.color = c;
        mr2.sharedMaterial = mat;

        var pc = go.AddComponent<PolygonCollider2D>();
        pc.SetPath(0, poly.ToArray());
        pc.isTrigger = true; // OverlapPoint 用
        pc.enabled = true;

        return go;
    }

    // ========= 可視化/演出 =========

    void AddConfetti(GameObject host)
    {
        var psObj = new GameObject("Confetti");
        psObj.transform.SetParent(host.transform, false);
        var ps = psObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = confettiLife;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.gravityModifier = confettiGravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, (short)Random.Range((int)confettiCountRange.x, (int)confettiCountRange.y))
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = confettiColorOverLifetime;

        ps.Play();
        Destroy(psObj, confettiLife + 1.0f);
    }

    // ========= 幾何ヘルパー =========

    struct Intersection
    {
        public Vector2 point;     // 交点（ローカル）
        public int edgeIndex;     // 辺インデックス i（頂点 i→i+1）
    }

    // 直線 a-b と多角形の交点を2つ取得（直線は無限延長）
    static bool LinePolygonIntersections(Vector2 a, Vector2 b, List<Vector2> polygon,
        out Intersection A, out Intersection B)
    {
        A = default; B = default;
        var hits = new List<Intersection>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 p = polygon[i];
            Vector2 q = polygon[(i + 1) % polygon.Count];
            if (SegmentLineIntersection(p, q, a, b, out Vector2 ip))
                hits.Add(new Intersection { point = ip, edgeIndex = i });
        }
        if (hits.Count < 2) return false;
        A = hits[0]; B = hits[1];
        return true;
    }

    static bool SegmentLineIntersection(Vector2 p, Vector2 q, Vector2 a, Vector2 b, out Vector2 ip)
    {
        ip = default;
        Vector2 r = q - p;
        Vector2 s = b - a;
        float rxs = Cross(r, s);
        float qpxs = Cross(a - p, r);

        // ★イプシロンを緩めて水平/垂直に耐性をつける
        if (Mathf.Abs(rxs) < 1e-8f) return false;

        float t = Cross(a - p, s) / rxs; // p + t*r
        float u = qpxs / rxs;            // a + u*s

        if (t >= -1e-6f && t <= 1f + 1e-6f) // 辺の範囲に入っている
        {
            ip = p + t * r;
            return true;
        }
        return false;
    }

    static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    // 多角形の境界弧（交点A→B）: ccw=trueでCCW方向、falseでCW
    static List<Vector2> BuildPolyArc(List<Vector2> polygon, int edgeA, int edgeB, Vector2 ptA, Vector2 ptB, bool ccw)
    {
        var arc = new List<Vector2>();
        arc.Add(ptA);

        int i = edgeA;
        while (true)
        {
            i = ccw ? (i + 1) % polygon.Count : (i - 1 + polygon.Count) % polygon.Count;
            int v = ccw ? i : (i + 1) % polygon.Count;
            arc.Add(polygon[v]);
            if (i == edgeB) break;
        }
        arc.Add(ptB);
        return arc;
    }

    static float SignedArea(List<Vector2> poly)
    {
        float a = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            var p = poly[i];
            var q = poly[(i + 1) % poly.Count];
            a += p.x * q.y - q.x * p.y;
        }
        return 0.5f * a;
    }

    static void EnsureCCW(List<Vector2> poly)
    {
        if (SignedArea(poly) < 0f) poly.Reverse();
    }

    static Vector2 Centroid(List<Vector2> poly)
    {
        float a = 0f, cx = 0f, cy = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            var p = poly[i];
            var q = poly[(i + 1) % poly.Count];
            float cross = p.x * q.y - q.x * p.y;
            a += cross;
            cx += (p.x + q.x) * cross;
            cy += (p.y + q.y) * cross;
        }
        if (Mathf.Abs(a) < 1e-8f)
        {
            Vector2 avg = Vector2.zero;
            foreach (var v in poly) avg += v;
            return avg / Mathf.Max(1, poly.Count);
        }
        a *= 0.5f;
        return new Vector2(cx / (6f * a), cy / (6f * a));
    }

    (Vector3[] verts, Vector2[] uvs, int[] tris) BuildMeshDataFromPoly(List<Vector2> poly)
    {
        int n = poly.Count;
        var verts = new Vector3[n];
        var uvs = new Vector2[n];

        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;
        float minX = -w / 2f, minY = -h / 2f;

        for (int i = 0; i < n; i++)
        {
            var p = poly[i];
            verts[i] = new Vector3(p.x, p.y, zPlane);
            uvs[i] = new Vector2((p.x - minX) / w, (p.y - minY) / h);
        }

        // 扇形トライアングル
        var tris = FanTriangulate(n);
        return (verts, uvs, tris);
    }

    static int[] FanTriangulate(int n)
    {
        var tris = new List<int>((n - 2) * 3);
        for (int i = 1; i < n - 1; i++)
        {
            tris.Add(0); tris.Add(i); tris.Add(i + 1);
        }
        return tris.ToArray();
    }

    void MakeDoubleSidedMesh(Mesh mesh)
    {
        int vertCount = mesh.vertexCount;
        var verts = mesh.vertices;
        var uvs = mesh.uv;
        var tris = mesh.triangles;

        var newVerts = new Vector3[vertCount * 2];
        var newUVs = new Vector2[vertCount * 2];
        var newTris = new int[tris.Length * 2];

        // 表
        for (int i = 0; i < vertCount; i++) { newVerts[i] = verts[i]; newUVs[i] = uvs[i]; }
        for (int i = 0; i < tris.Length; i++) newTris[i] = tris[i];

        // 裏（反転）
        for (int i = 0; i < vertCount; i++) { newVerts[i + vertCount] = verts[i]; newUVs[i + vertCount] = uvs[i]; }
        for (int i = 0; i < tris.Length; i += 3)
        {
            newTris[tris.Length + i + 0] = tris[i + 0] + vertCount;
            newTris[tris.Length + i + 1] = tris[i + 2] + vertCount;
            newTris[tris.Length + i + 2] = tris[i + 1] + vertCount;
        }

        mesh.vertices = newVerts;
        mesh.uv = newUVs;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // ========= 座標変換 =========

    Vector3 ScreenToWorldOnPlane(Vector3 screen, float z)
    {
        float dist = Mathf.Abs(targetCamera.transform.position.z - z);
        var w = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = z;
        return w;
    }
}
