using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 左クリック: 縦切り（x=const） / 右クリック: 横切り（y=const）
/// カメラ映像を四角メッシュに貼り、何度でも分割可能。クリック or ←/→ で残す側を選択。
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenAxisSlicer_AllInOne : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Input visuals (optional)")]
    public LineRenderer lineRenderer;             // 切断線の可視化（任意）
    public float lineZ = 0f;

    [Header("Plane & Material")]
    public float zPlane = 0f;                     // 2D平面（Z）
    public bool doubleSided = true;               // 両面描画（メッシュ複製）
    public Color pieceTint = Color.white;         // マテリアル色（半透明可）

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
    Material sharedMat;

    // 現在の多角形（ローカル・CCW）
    List<Vector2> currentPoly = new List<Vector2>();

    // 選択待ち
    bool awaitingChoice = false;
    GameObject tempA, tempB;
    List<Vector2> polyA, polyB; // ローカル
    Vector2 lastCutDirLocal = Vector2.right; // ←/→キー用

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        // RenderTexture
        rt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        targetCamera.targetTexture = rt;

        // Unlit/Texture（両面はジオメトリ複製で実現）
        sharedMat = new Material(Shader.Find("Unlit/Texture"));
        sharedMat.mainTexture = rt;
        sharedMat.color = pieceTint;
        mr.sharedMaterial = sharedMat;

        // 初期四角形
        BuildInitialQuadAsPolygon();
        EnsureCCW(currentPoly);
        RebuildMainMesh(currentPoly);

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }

        // 紙吹雪デフォルト
        if (confettiColorOverLifetime == null || confettiColorOverLifetime.colorKeys.Length == 0)
        {
            var g = new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f,0.9f,0.6f), 1f)
            };
            var a = new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
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
            // ←/→ キーで選択（切断方向に依存せず、線の“右側”を→、左側を←として判定）
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

            // クリック選択（左クリック）
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
                    // 線上で両方ヒット → 重心に近い方
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

        // ★ 切断トリガー：左クリック = 縦切り、右クリック = 横切り
        if (Input.GetMouseButtonDown(0)) // 縦
        {
            Vector3 w = ScreenToWorldOnPlane(Input.mousePosition, zPlane);
            TrySliceVerticalAtWorldX(w);
        }
        else if (Input.GetMouseButtonDown(1)) // 横
        {
            Vector3 w = ScreenToWorldOnPlane(Input.mousePosition, zPlane);
            TrySliceHorizontalAtWorldY(w);
        }
    }

    // ========= 初期形状 / メインメッシュ =========

    void BuildInitialQuadAsPolygon()
    {
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

    // ========= カット：縦 / 横 =========

    void TrySliceVerticalAtWorldX(Vector3 worldPoint)
    {
        Vector2 local = transform.InverseTransformPoint(worldPoint);
        float x = local.x;

        var hits = IntersectionsWithVerticalLine(x, currentPoly);
        if (hits.Count < 2) return;

        // 一番離れた2交点を採用（凹でも安定して2分割）
        ChooseFarthestPair(hits, true, out var A, out var B);

        // 可視ライン（任意）
        if (lineRenderer)
        {
            var p0 = transform.TransformPoint(new Vector3(A.point.x, A.point.y, zPlane));
            var p1 = transform.TransformPoint(new Vector3(B.point.x, B.point.y, zPlane));
            lineRenderer.positionCount = 2;
            p0.z = lineZ; p1.z = lineZ;
            lineRenderer.SetPosition(0, p0);
            lineRenderer.SetPosition(1, p1);
            // すぐ消す
            Invoke(nameof(ClearLine), 0.05f);
        }

        lastCutDirLocal = Vector2.up; // 縦切りの線方向

        BuildPiecesFromTwoIntersections(A, B);
    }

    void TrySliceHorizontalAtWorldY(Vector3 worldPoint)
    {
        Vector2 local = transform.InverseTransformPoint(worldPoint);
        float y = local.y;

        var hits = IntersectionsWithHorizontalLine(y, currentPoly);
        if (hits.Count < 2) return;

        ChooseFarthestPair(hits, false, out var A, out var B);

        if (lineRenderer)
        {
            var p0 = transform.TransformPoint(new Vector3(A.point.x, A.point.y, zPlane));
            var p1 = transform.TransformPoint(new Vector3(B.point.x, B.point.y, zPlane));
            lineRenderer.positionCount = 2;
            p0.z = lineZ; p1.z = lineZ;
            lineRenderer.SetPosition(0, p0);
            lineRenderer.SetPosition(1, p1);
            Invoke(nameof(ClearLine), 0.05f);
        }

        lastCutDirLocal = Vector2.right; // 横切りの線方向

        BuildPiecesFromTwoIntersections(A, B);
    }

    void ClearLine()
    {
        if (lineRenderer) lineRenderer.positionCount = 0;
    }

    void BuildPiecesFromTwoIntersections(Intersection A, Intersection B)
    {
        // 弧（A→B / B→A）＋直線で2ポリゴン
        var arcAB = BuildPolyArc(currentPoly, A.edgeIndex, B.edgeIndex, A.point, B.point, true);
        var arcBA = BuildPolyArc(currentPoly, A.edgeIndex, B.edgeIndex, A.point, B.point, false);

        var lineAB = new List<Vector2> { A.point, B.point };
        var lineBA = new List<Vector2>(lineAB); lineBA.Reverse();

        polyA = new List<Vector2>(arcAB.Count + lineBA.Count);
        polyA.AddRange(arcAB); polyA.AddRange(lineBA); EnsureCCW(polyA);

        polyB = new List<Vector2>(arcBA.Count + lineAB.Count);
        polyB.AddRange(arcBA); polyB.AddRange(lineAB); EnsureCCW(polyB);

        // 一時ピース表示（半透明 & Trigger Collider）
        (tempA, tempB) = (CreateTempPiece("Choice_A", polyA), CreateTempPiece("Choice_B", polyB));

        // 親は一旦非表示（確定時に currentPoly から再構築）
        mr.enabled = false;
        mf.mesh = null;

        awaitingChoice = true;
    }

    // ========= 一時ピース・確定 =========

    void ChooseAndFinalize(GameObject keepGO, List<Vector2> keepPoly, GameObject dropGO)
    {
        currentPoly = new List<Vector2>(keepPoly);
        RebuildMainMesh(currentPoly);

        // 落とす側
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

        Destroy(keepGO);
        tempA = tempB = null;
        awaitingChoice = false;
    }

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
        pc.isTrigger = true; // OverlapPoint用
        pc.enabled = true;

        return go;
    }

    // ========= 交点（軸専用で頑丈） =========

    struct Intersection
    {
        public Vector2 point;     // 交点（ローカル）
        public int edgeIndex;     // 辺インデックス i（頂点 i→i+1）
    }

    // 垂直線 x=c と多角形の全交点
    static List<Intersection> IntersectionsWithVerticalLine(float c, List<Vector2> poly)
    {
        const float EPS = 1e-7f;
        var hits = new List<Intersection>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p = poly[i];
            Vector2 q = poly[(i + 1) % poly.Count];

            // 端点がちょうど乗る場合の多重カウント防止
            bool pOn = Mathf.Abs(p.x - c) < EPS;
            bool qOn = Mathf.Abs(q.x - c) < EPS;

            // 完全に同一直線（縦辺 x==c）はスキップ（曖昧さ回避）
            if (pOn && qOn) continue;

            // 符号が異なる or 片側がちょうど乗る → 交差候補
            if ((p.x - c) * (q.x - c) <= 0f)
            {
                // 補正：端点一致は片側だけ数える
                if (pOn || qOn)
                {
                    // 頂点を共有する隣の辺で重複するので、上側（yが大きい）に属する方のみ採用
                    if (pOn && qOn) continue;
                }

                float dx = q.x - p.x;
                if (Mathf.Abs(dx) < EPS) continue; // 縦辺は除外済み

                float t = (c - p.x) / dx;
                if (t < -EPS || t > 1f + EPS) continue;

                float y = Mathf.Lerp(p.y, q.y, Mathf.Clamp01(t));
                hits.Add(new Intersection { point = new Vector2(c, y), edgeIndex = i });
            }
        }
        return hits;
    }

    // 水平線 y=c と多角形の全交点
    static List<Intersection> IntersectionsWithHorizontalLine(float c, List<Vector2> poly)
    {
        const float EPS = 1e-7f;
        var hits = new List<Intersection>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p = poly[i];
            Vector2 q = poly[(i + 1) % poly.Count];

            bool pOn = Mathf.Abs(p.y - c) < EPS;
            bool qOn = Mathf.Abs(q.y - c) < EPS;

            // 横辺 y==c はスキップ
            if (pOn && qOn) continue;

            if ((p.y - c) * (q.y - c) <= 0f)
            {
                if (pOn || qOn)
                {
                    if (pOn && qOn) continue;
                }

                float dy = q.y - p.y;
                if (Mathf.Abs(dy) < EPS) continue; // 横辺は除外済み

                float t = (c - p.y) / dy;
                if (t < -EPS || t > 1f + EPS) continue;

                float x = Mathf.Lerp(p.x, q.x, Mathf.Clamp01(t));
                hits.Add(new Intersection { point = new Vector2(x, c), edgeIndex = i });
            }
        }
        return hits;
    }

    // 交点群から「最も離れた2点」を選ぶ（縦: y距離 / 横: x距離）
    static void ChooseFarthestPair(List<Intersection> hits, bool vertical, out Intersection A, out Intersection B)
    {
        A = hits[0]; B = hits[1];
        float maxD = -1f;
        for (int i = 0; i < hits.Count; i++)
            for (int j = i + 1; j < hits.Count; j++)
            {
                float d = vertical
                    ? Mathf.Abs(hits[i].point.y - hits[j].point.y)
                    : Mathf.Abs(hits[i].point.x - hits[j].point.x);
                if (d > maxD) { maxD = d; A = hits[i]; B = hits[j]; }
            }
    }

    // ========= 幾何ヘルパー =========

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

        for (int i = 0; i < vertCount; i++) { newVerts[i] = verts[i]; newUVs[i] = uvs[i]; }
        for (int i = 0; i < tris.Length; i++) newTris[i] = tris[i];

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

    // ========= 可視化 / 紙吹雪 =========

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

    // ========= 座標変換 =========

    Vector3 ScreenToWorldOnPlane(Vector3 screen, float z)
    {
        float dist = Mathf.Abs(targetCamera.transform.position.z - z);
        var w = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = z;
        return w;
    }
}