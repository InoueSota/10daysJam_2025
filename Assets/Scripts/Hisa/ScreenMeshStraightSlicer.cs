using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �J�����̉f�����l�p���b�V���ɓ\��A�h���b�O�́u�����v�Ŏl�p��2��������B
/// �E�Ώۂ̓I���\�J�����O��i����=2*size*aspect / �c=2*size�j
/// �E�������l�p���E��2�񉡐؂�Ƃ��̂ݕ���
/// �E�������UV���ێ�����2��Mesh�𐶐�
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenMeshStraightSlicer : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Optional visuals")]
    public LineRenderer lineRenderer;       // �؂���̌����ځi�C�Ӂj
    public float lineZ = 0f;

    [Header("Quad plane")]
    public float zPlane = 0f;               // �l�p���b�V����Z�i2D���ʁj
    public bool hideOriginalAfterSlice = true;

    RenderTexture rt;
    MeshFilter mf;
    MeshRenderer mr;

    // �X�N���[���l�p�i���[�J�����W�ECCW�j: �������E�����E�と����
    Vector2[] rectLocal = new Vector2[4];

    // ����
    bool dragging;
    Vector3 dragStartW, dragEndW;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        // RenderTexture �Z�b�g
        rt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        targetCamera.targetTexture = rt;

        // �}�e���A���iUnlit/Texture�j
        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = rt;
        mr.sharedMaterial = mat;

        // �l�p���b�V�����쐬
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

    // �J�������W��zPlane�ハ�[���h
    Vector3 ScreenToWorldOnPlane(Vector3 screen, float z)
    {
        float dist = Mathf.Abs(targetCamera.transform.position.z - z);
        var w = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        w.z = z;
        return w;
    }

    void BuildQuadMesh()
    {
        // �I���\�T�C�Y�����`�T�C�Y������
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

    // �����ŋ�`��؂� �� 2�̑��p�`�ɕ���
    void TrySliceWithStraightLine(Vector3 w0, Vector3 w1)
    {
        // ���[�J���֕ϊ�
        Vector2 a = transform.InverseTransformPoint(w0);
        Vector2 b = transform.InverseTransformPoint(w1);

        // ���������ŋ�`�Ƃ̌�_��T���i�K��2�Ԃ��悤�g���j
        if (!LineRectFullIntersections(a, b, rectLocal, out var hitA, out var hitB))
        {
            Debug.LogWarning("��_��2�������܂���ł���");
            return;
        }

        // --- �ȍ~�͓��������iarcAB / arcBA �쐬 �� ���b�V�������j ---
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
        public Vector2 point;   // ��_�i���[�J���j
        public int edge;        // ���������ӂ̃C���f�b�N�X�i0..3�j: 0 L->R bottom, 1 right, 2 top, 3 left
    }

    // �������� a-b �Ƌ�`�ӂ̌�����O�㏇��2�擾
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

    // ��`��(����pq) �� ��������ab �̌���
    static bool SegmentLineIntersection(Vector2 p, Vector2 q, Vector2 a, Vector2 b, out Vector2 ip)
    {
        ip = default;
        Vector2 r = q - p;
        Vector2 s = b - a;
        float rxs = Cross(r, s);
        if (Mathf.Abs(rxs) < 1e-7f) return false; // ���s

        float t = Cross(a - p, s) / rxs;     // p + t r
        float u = Cross(a - p, r) / rxs;     // a + u s

        if (t >= 0f && t <= 1f) // �ӂ͈͓̔�
        {
            ip = p + t * r;
            return true;
        }
        return false;
    }

    static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    // ��`�̋��E�ʁi��_A��B�j: forwardCCW=true ��CCW�����Afalse��CW
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

    // �ʑ��p�`�̎O�p���i���S�_����̐�`�j
    void CreatePieceMesh(string name, List<Vector2> polygon)
    {
        if (polygon.Count < 3) return;

        // UV�͌��l�p�� [0..1] �Ɏˉe
        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;
        float minX = -w / 2f, minY = -h / 2f;

        // ���_/UV
        int n = polygon.Count;
        var verts = new Vector3[n];
        var uvs = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            var p = polygon[i];
            verts[i] = new Vector3(p.x, p.y, zPlane);
            uvs[i] = new Vector2((p.x - minX) / w, (p.y - minY) / h);
        }

        // ��`�g���C�A���O��
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

        // �����}�e���A���iRenderTexture�j
        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = rt;
        mr2.sharedMaterial = mat;
    }
}
