using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �N������ targetCamera �̉f���� 1 �񂾂� Texture2D �ɃX�i�b�v�V���b�g�B
/// ���̐Î~�e�N�X�`�����l�p���b�V���ɓ\��A
/// ���N���b�N=�c�؂�ix=���j�A�E�N���b�N=���؂�iy=���j�ŉ��x�ł������B
/// ���� �N���b�N or ��/�� �Ŏc������I���B���Ƃ�����Rigidbody2D�Łg�Ђ��h�{������B
/// 
/// �y�|�C���g�z
/// - �L���v�`���͏��������̂݁i�Ȍ�� RenderTexture ���g��Ȃ��j
/// - currentPoly ���X�V���ĕ����o�O�Ȃ�
/// - ��_�������Ă��u�ł����ꂽ 2 �_�v�ŏ�� 2 �s�[�X��
/// - �N���b�N����� Collider2D.OverlapPoint�i���W�덷�ɋ����j
/// - CCW ���K���A�K�v�Ȃ痼�ʕ`��idoubleSided�j
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenAxisSlicer_Snapshot : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Capture (one-shot)")]
    public int captureWidth = 0;     // 0 �Ȃ�J�����̃s�N�Z���T�C�Y���g�p
    public int captureHeight = 0;

    [Header("Input visuals (optional)")]
    public LineRenderer lineRenderer; // �O�Ղ̌����ځi�C�Ӂj
    public float lineZ = 0f;

    [Header("Plane & Material")]
    public float zPlane = 0f;        // 2D���ʂ�Z
    public bool doubleSided = true;  // ���ʕ`��i�W�I���g�������j
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

    // ����
    MeshFilter mf;
    MeshRenderer mr;
    Material sharedMat;
    Texture2D snapshotTex;

    // ���݂̑��p�`�i���[�J���ECCW�j
    List<Vector2> currentPoly = new List<Vector2>();

    // �I��҂�
    bool awaitingChoice = false;
    GameObject tempA, tempB;
    List<Vector2> polyA, polyB;
    Vector2 lastCutDirLocal = Vector2.right;

    //���o�p�ɐ؂����ӂ̍��W��ۑ�
    public Vector3 tmpP0;
    public Vector3 tmpP1;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        // �� ���������̂�: �J������ RenderTexture �ɕ`���� Texture2D �֋z���o��
        snapshotTex = CaptureCameraOnceToTexture2D(targetCamera, captureWidth, captureHeight);

        // �}�e���A���iUnlit/Texture�j
        sharedMat = new Material(Shader.Find("Unlit/Texture"));
        sharedMat.mainTexture = snapshotTex;
        sharedMat.color = pieceTint;
        mr.sharedMaterial = sharedMat;

        // �����l�p�`�i�I���\�J�����̌������ɍ��킹��j
        BuildInitialQuadAsPolygon();
        EnsureCCW(currentPoly);
        RebuildMainMesh(currentPoly);

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }

        // �������ŃX�N���[���T�C�Y�Ƀt�B�b�g�i�ʒu���킹�{���b�V���č\�z�j
        FitToScreenAtStart();

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
        }

        // ������̃f�t�H���g
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

    // --- 1�񂾂��X�i�b�v�V���b�g���B�� ---
    Texture2D CaptureCameraOnceToTexture2D(Camera cam, int w, int h)
    {
        // �����FURP�ł� Base �J�����ōs���iOverlay �� Render() �ł��܂���j
        int pw = (w > 0) ? w : Mathf.Max(1, cam.pixelWidth);
        int ph = (h > 0) ? h : Mathf.Max(1, cam.pixelHeight);
        var rt = new RenderTexture(pw, ph, 24, RenderTextureFormat.ARGB32);
        rt.name = "SliceSnapshotRT";
        var prevTarget = cam.targetTexture;
        var prevActive = RenderTexture.active;

        try
        {
            cam.targetTexture = rt;
            cam.Render(); // ��������1�t���[�����������_�����O
            RenderTexture.active = rt;

            // Texture2D �쐬�isRGB�͎����ϊ��ɔC����j
            var tex = new Texture2D(pw, ph, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, pw, ph), 0, 0);
            tex.Apply(false, false);
            return tex;
        }
        finally
        {
            // ��n���FDisplay1�֕`����悤 targetTexture ���O��
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
            // ��/���L�[
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                var ca = Centroid(polyA);
                var cb = Centroid(polyB);
                Vector2 n = new Vector2(-lastCutDirLocal.y, lastCutDirLocal.x); // �E�@��
                float sideA = Vector2.Dot(ca, n);
                float sideB = Vector2.Dot(cb, n);
                bool rightIsA = sideA > sideB;

                bool chooseRight = Input.GetKeyDown(KeyCode.RightArrow);
                bool chooseA = (chooseRight && rightIsA) || (!chooseRight && !rightIsA);
                ChooseAndFinalize(chooseA ? tempA : tempB, chooseA ? polyA : polyB,
                                  chooseA ? tempB : tempA);
                return;
            }

            // �N���b�N�I��
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

        // ���N���b�N=�c�؂�A�E�N���b�N=���؂�
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

    // ========= �����`�� / ���C�����b�V�� =========

    void BuildInitialQuadAsPolygon()
    {
        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;

        currentPoly.Clear();
        // �������E�����E�と����iCCW�j
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

    // ========= �J�b�g�F�c / �� =========

    void TrySliceVerticalAtWorldX(Vector3 worldPoint)
    {
        Vector2 local = transform.InverseTransformPoint(worldPoint);
        float x = local.x;

        var hits = IntersectionsWithVerticalLine(x, currentPoly);
        if (hits.Count < 2) return;

        ChooseFarthestPair(hits, true, out var A, out var B);

        // ���C���\���i�C�Ӂj
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

    // ========= �ꎞ�s�[�X�E�m�� =========

    void ChooseAndFinalize(GameObject keepGO, List<Vector2> keepPoly, GameObject dropGO)
    {
        currentPoly = new List<Vector2>(keepPoly);
        RebuildMainMesh(currentPoly);

        // �����鑤
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
        var c = pieceTint; c.a = 0.8f; // ������
        mat.color = c;
        mr2.sharedMaterial = mat;

        var pc = go.AddComponent<PolygonCollider2D>();
        pc.SetPath(0, poly.ToArray());
        pc.isTrigger = true;
        pc.enabled = true;

        return go;
    }

    // ========= ��_�i����p�Ŋ��j =========

    struct Intersection
    {
        public Vector2 point;     // ��_�i���[�J���j
        public int edgeIndex;     // �ӃC���f�b�N�X i�i���_ i��i+1�j
    }

    static List<Intersection> IntersectionsWithVerticalLine(float c, List<Vector2> poly)
    {
        const float EPS = 1e-7f;
        var hits = new List<Intersection>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p = poly[i], q = poly[(i + 1) % poly.Count];
            bool pOn = Mathf.Abs(p.x - c) < EPS, qOn = Mathf.Abs(q.x - c) < EPS;
            if (pOn && qOn) continue; // �������ۂ��Əd�Ȃ�̂̓X�L�b�v

            if ((p.x - c) * (q.x - c) <= 0f)
            {
                if (Mathf.Abs(q.x - p.x) < EPS) continue; // �c�ӂ͏��O�ς�
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
            if (pOn && qOn) continue; // ���ӂ��ۂ��Əd�Ȃ�

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

    // ========= �􉽃w���p�[ =========

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

    // ========= ���W�ϊ� =========

    Vector3 ScreenToWorldOnPlane(Vector3 screen, float z)
    {
        float dist = Mathf.Abs(targetCamera.transform.position.z - z);
        var w = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = z;
        return w;
    }

    // �����鑤�̃s�[�X�Ɏ�������o��
    void AddConfetti(GameObject host)
    {
        // �����ʒu�F������s�[�X�̃��[�J�����S
        var psObj = new GameObject("Confetti");
        psObj.transform.SetParent(host.transform, false);

        var ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = confettiLife;                            // �؋󎞊�
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);  // �΂炯���x
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.gravityModifier = confettiGravity;                       // ������
        main.simulationSpace = ParticleSystemSimulationSpace.World;   // ���[���h�ŕ���

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
        new ParticleSystem.Burst(0.0f, (short)Random.Range((int)confettiCountRange.x, (int)confettiCountRange.y))
    });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        // �J���[�ω��i�O���f���w��Ȃ� Awake �Ŋ�������Ă����z��j
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = confettiColorOverLifetime;

        // �i�C�Ӂj�����_���ݒ�F�ޗ��������Ă������܂����AURP/Built-in�Ō����ڂ����肳�������Ȃ�
        var pr = ps.GetComponent<ParticleSystemRenderer>();
        // �\�Ȃ�URP��Unlit�p�[�e�B�N���A�������Built-in��Standard Unlit�Ƀt�H�[���o�b�N
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
        Destroy(psObj, confettiLife + 1.0f); // �����N���[���A�b�v
    }

    // �� �ǉ�: �N�����ɃX�N���[���T�C�Y�փt�B�b�g
    void FitToScreenAtStart()
    {
        if (!targetCamera) targetCamera = Camera.main;

        // �J�������S��XY�ɍ��킹�AZ�� zPlane �ɌŒ�
        var cam = targetCamera;
        var pos = cam.transform.position;
        transform.position = new Vector3(pos.x, pos.y, zPlane);

        // ��]�E�X�P�[���͑f����
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // ��ʃT�C�Y���Z�o�i�I�[�\/�p�[�X�Ή��j
        float halfW, halfH;
        if (cam.orthographic)
        {
            halfH = cam.orthographicSize;
            halfW = halfH * cam.aspect;
        }
        else
        {
            // �p�[�X��: �J�������� zPlane �܂ł̋����ŉ��T�C�Y���v�Z
            float dist = Mathf.Abs(cam.transform.position.z - zPlane);
            halfH = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;
            halfW = halfH * cam.aspect;
        }

        // ���̃T�C�Y�ŏ����|���S�������
        BuildInitialQuadAsPolygon(halfW * 2f, halfH * 2f);
        EnsureCCW(currentPoly);
        RebuildMainMesh(currentPoly);
    }

    // �� �ύX: �����t���̏����l�p�`�r���h�i��w, ����h�j
    void BuildInitialQuadAsPolygon(float w, float h)
    {
        currentPoly.Clear();
        // �������E�����E�と����iCCW�j
        currentPoly.Add(new Vector2(-w * 0.5f, -h * 0.5f));
        currentPoly.Add(new Vector2(+w * 0.5f, -h * 0.5f));
        currentPoly.Add(new Vector2(+w * 0.5f, +h * 0.5f));
        currentPoly.Add(new Vector2(-w * 0.5f, +h * 0.5f));
    }
}
