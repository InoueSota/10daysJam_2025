using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 起動時に targetCamera の映像を 1 回だけ Texture2D にスナップショット。
/// その静止テクスチャを四角メッシュに貼り、
/// 左クリック=縦切り（x=一定）、右クリック=横切り（y=一定）で何度でも分割。
/// 毎回 クリック or ←/→ で残す側を選択。落とす側はRigidbody2Dで“ひらり”＋紙吹雪。
/// 
/// 【ポイント】
/// - キャプチャは初期化時のみ（以後は RenderTexture を使わない）
/// - currentPoly を更新して復活バグなし
/// - 交点が多くても「最も離れた 2 点」で常に 2 ピース化
/// - クリック判定は Collider2D.OverlapPoint（座標誤差に強い）
/// - CCW 正規化、必要なら両面描画（doubleSided）
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenAxisSlicer_Snapshot : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Capture (one-shot)")]
    public int captureWidth = 0;     // 0 ならカメラのピクセルサイズを使用
    public int captureHeight = 0;

    [Header("Input visuals (optional)")]
    public LineRenderer lineRenderer; // 軌跡の見た目（任意）
    public float lineZ = 0f;

    [Header("Plane & Material")]
    public float zPlane = 0f;        // 2D平面のZ
    public bool doubleSided = true;  // 両面描画（ジオメトリ複製）
    public Color pieceTint = Color.white;

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
    MeshFilter mf;
    MeshRenderer mr;
    Material sharedMat;
    Texture2D snapshotTex;

    // 現在の多角形（ローカル・CCW）
    List<Vector2> currentPoly = new List<Vector2>();

    // 選択待ち
    bool awaitingChoice = false;
    GameObject tempA, tempB;
    List<Vector2> polyA, polyB;
    Vector2 lastCutDirLocal = Vector2.right;

    //演出用に切った辺の座標を保存
    public Vector3 tmpP0;
    public Vector3 tmpP1;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        // ★ 初期化時のみ: カメラを RenderTexture に描いて Texture2D へ吸い出す
        snapshotTex = CaptureCameraOnceToTexture2D(targetCamera, captureWidth, captureHeight);

        // マテリアル（Unlit/Texture）
        sharedMat = new Material(Shader.Find("Unlit/Texture"));
        sharedMat.mainTexture = snapshotTex;
        sharedMat.color = pieceTint;
        mr.sharedMaterial = sharedMat;

        // 初期四角形（オルソカメラの見え幅に合わせる）
        BuildInitialQuadAsPolygon();
        EnsureCCW(currentPoly);
        RebuildMainMesh(currentPoly);

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }

        // ★ここでスクリーンサイズにフィット（位置合わせ＋メッシュ再構築）
        FitToScreenAtStart();

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }

        // 紙吹雪のデフォルト
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

    // --- 1回だけスナップショットを撮る ---
    Texture2D CaptureCameraOnceToTexture2D(Camera cam, int w, int h)
    {
        // 推奨：URPでは Base カメラで行う（Overlay は Render() できません）
        int pw = (w > 0) ? w : Mathf.Max(1, cam.pixelWidth);
        int ph = (h > 0) ? h : Mathf.Max(1, cam.pixelHeight);
        var rt = new RenderTexture(pw, ph, 24, RenderTextureFormat.ARGB32);
        rt.name = "SliceSnapshotRT";
        var prevTarget = cam.targetTexture;
        var prevActive = RenderTexture.active;

        try
        {
            cam.targetTexture = rt;
            cam.Render(); // ★ここで1フレームだけレンダリング
            RenderTexture.active = rt;

            // Texture2D 作成（sRGBは自動変換に任せる）
            var tex = new Texture2D(pw, ph, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, pw, ph), 0, 0);
            tex.Apply(false, false);
            return tex;
        }
        finally
        {
            // 後始末：Display1へ描けるよう targetTexture を外す
            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            rt.Release();
            Object.Destroy(rt);
        }
    }

    void Update()
    {
        if (awaitingChoice)
        {
            // ←/→キー
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

            // クリック選択
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

        // 左クリック=縦切り、右クリック=横切り
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 w = ScreenToWorldOnPlane(Input.mousePosition, zPlane);
            TrySliceVerticalAtWorldX(w);
        }
        else if (Input.GetMouseButtonDown(1))
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

        ChooseFarthestPair(hits, true, out var A, out var B);

        // ライン表示（任意）
        if (lineRenderer)
        {
            var p0 = transform.TransformPoint(new Vector3(A.point.x, A.point.y, zPlane));
            var p1 = transform.TransformPoint(new Vector3(B.point.x, B.point.y, zPlane));
            lineRenderer.positionCount = 2;
            p0.z = lineZ; p1.z = lineZ;
            lineRenderer.SetPosition(0, p0);
            lineRenderer.SetPosition(1, p1);
            Invoke(nameof(ClearLine), 0.05f);

            tmpP0 = p0;
            tmpP1 = p1;
        }

        lastCutDirLocal = Vector2.up;
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
            tmpP0 = p0;
            tmpP1 = p1;
        }

        lastCutDirLocal = Vector2.right;
        BuildPiecesFromTwoIntersections(A, B);
    }

    void ClearLine() { if (lineRenderer) lineRenderer.positionCount = 0; }

    void BuildPiecesFromTwoIntersections(Intersection A, Intersection B)
    {
        var arcAB = BuildPolyArc(currentPoly, A.edgeIndex, B.edgeIndex, A.point, B.point, true);
        var arcBA = BuildPolyArc(currentPoly, A.edgeIndex, B.edgeIndex, A.point, B.point, false);

        var lineAB = new List<Vector2> { A.point, B.point };
        var lineBA = new List<Vector2>(lineAB); lineBA.Reverse();

        polyA = new List<Vector2>(arcAB.Count + lineBA.Count);
        polyA.AddRange(arcAB); polyA.AddRange(lineBA); EnsureCCW(polyA);

        polyB = new List<Vector2>(arcBA.Count + lineAB.Count);
        polyB.AddRange(arcBA); polyB.AddRange(lineAB); EnsureCCW(polyB);

        (tempA, tempB) = (CreateTempPiece("Choice_A", polyA), CreateTempPiece("Choice_B", polyB));

        mr.enabled = false;
        mf.mesh = null;
        awaitingChoice = true;
    }

    // ========= 一時ピース・確定 =========

    void ChooseAndFinalize(GameObject keepGO, List<Vector2> keepPoly, GameObject dropGO)
    {
        currentPoly = new List<Vector2>(keepPoly);
        RebuildMainMesh(currentPoly);

        // 落ちる側
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
        pc.isTrigger = true;
        pc.enabled = true;

        return go;
    }

    // ========= 交点（軸専用で頑丈） =========

    struct Intersection
    {
        public Vector2 point;     // 交点（ローカル）
        public int edgeIndex;     // 辺インデックス i（頂点 i→i+1）
    }

    static List<Intersection> IntersectionsWithVerticalLine(float c, List<Vector2> poly)
    {
        const float EPS = 1e-7f;
        var hits = new List<Intersection>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p = poly[i], q = poly[(i + 1) % poly.Count];
            bool pOn = Mathf.Abs(p.x - c) < EPS, qOn = Mathf.Abs(q.x - c) < EPS;
            if (pOn && qOn) continue; // 線分が丸ごと重なるのはスキップ

            if ((p.x - c) * (q.x - c) <= 0f)
            {
                if (Mathf.Abs(q.x - p.x) < EPS) continue; // 縦辺は除外済み
                float t = (c - p.x) / (q.x - p.x);
                if (t < -EPS || t > 1f + EPS) continue;
                float y = Mathf.Lerp(p.y, q.y, Mathf.Clamp01(t));
                hits.Add(new Intersection { point = new Vector2(c, y), edgeIndex = i });
            }
        }
        return hits;
    }

    static List<Intersection> IntersectionsWithHorizontalLine(float c, List<Vector2> poly)
    {
        const float EPS = 1e-7f;
        var hits = new List<Intersection>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p = poly[i], q = poly[(i + 1) % poly.Count];
            bool pOn = Mathf.Abs(p.y - c) < EPS, qOn = Mathf.Abs(q.y - c) < EPS;
            if (pOn && qOn) continue; // 横辺が丸ごと重なる

            if ((p.y - c) * (q.y - c) <= 0f)
            {
                if (Mathf.Abs(q.y - p.y) < EPS) continue;
                float t = (c - p.y) / (q.y - p.y);
                if (t < -EPS || t > 1f + EPS) continue;
                float x = Mathf.Lerp(p.x, q.x, Mathf.Clamp01(t));
                hits.Add(new Intersection { point = new Vector2(x, c), edgeIndex = i });
            }
        }
        return hits;
    }

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

    // ========= 座標変換 =========

    Vector3 ScreenToWorldOnPlane(Vector3 screen, float z)
    {
        float dist = Mathf.Abs(targetCamera.transform.position.z - z);
        var w = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = z;
        return w;
    }

    // 落ちる側のピースに紙吹雪を出す
    void AddConfetti(GameObject host)
    {
        // 発生位置：落ちるピースのローカル中心
        var psObj = new GameObject("Confetti");
        psObj.transform.SetParent(host.transform, false);

        var ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = confettiLife;                            // 滞空時間
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);  // ばらけ速度
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.gravityModifier = confettiGravity;                       // 落下感
        main.simulationSpace = ParticleSystemSimulationSpace.World;   // ワールドで舞う

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
        new ParticleSystem.Burst(0.0f, (short)Random.Range((int)confettiCountRange.x, (int)confettiCountRange.y))
    });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        // カラー変化（グラデ未指定なら Awake で既定を入れておく想定）
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = confettiColorOverLifetime;

        // （任意）レンダラ設定：材料が無くても動きますが、URP/Built-inで見た目を安定させたいなら
        var pr = ps.GetComponent<ParticleSystemRenderer>();
        // 可能ならURPのUnlitパーティクル、無ければBuilt-inのStandard Unlitにフォールバック
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh == null) sh = Shader.Find("Particles/Standard Unlit");
        if (sh != null)
        {
            var mat = new Material(sh);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            pr.material = mat;
        }
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingFudge = 1f;

        ps.Play();
        Destroy(psObj, confettiLife + 1.0f); // 自動クリーンアップ
    }

    // ★ 追加: 起動時にスクリーンサイズへフィット
    void FitToScreenAtStart()
    {
        if (!targetCamera) targetCamera = Camera.main;

        // カメラ中心のXYに合わせ、Zは zPlane に固定
        var cam = targetCamera;
        var pos = cam.transform.position;
        transform.position = new Vector3(pos.x, pos.y, zPlane);

        // 回転・スケールは素直に
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // 画面サイズを算出（オーソ/パース対応）
        float halfW, halfH;
        if (cam.orthographic)
        {
            halfH = cam.orthographicSize;
            halfW = halfH * cam.aspect;
        }
        else
        {
            // パース時: カメラから zPlane までの距離で可視サイズを計算
            float dist = Mathf.Abs(cam.transform.position.z - zPlane);
            halfH = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;
            halfW = halfH * cam.aspect;
        }

        // そのサイズで初期ポリゴンを作る
        BuildInitialQuadAsPolygon(halfW * 2f, halfH * 2f);
        EnsureCCW(currentPoly);
        RebuildMainMesh(currentPoly);
    }

    // ★ 変更: 引数付きの初期四角形ビルド（幅w, 高さh）
    void BuildInitialQuadAsPolygon(float w, float h)
    {
        currentPoly.Clear();
        // 左下→右下→右上→左上（CCW）
        currentPoly.Add(new Vector2(-w * 0.5f, -h * 0.5f));
        currentPoly.Add(new Vector2(+w * 0.5f, -h * 0.5f));
        currentPoly.Add(new Vector2(+w * 0.5f, +h * 0.5f));
        currentPoly.Add(new Vector2(-w * 0.5f, +h * 0.5f));
    }
}
