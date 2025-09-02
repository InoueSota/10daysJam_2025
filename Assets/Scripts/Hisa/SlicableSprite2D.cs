using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �X��2D�X�v���C�g���A�^����ꂽ���[���h���W�̃|�����C����2��������B
/// �E�O���|���S���� PolygonCollider2D �� Path(0) ���g�p�i�P��O���z��j
/// �E�Ȑ����O����2�񉡐؂�Ƃ��A�O���̌ʁ{�Ȑ���2�̒P�����p�`�����
/// �EEar Clipping �ŎO�p�� �� MeshRenderer �𐶐��iSprite �̃e�N�X�`����\��j
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SlicableSprite2D : MonoBehaviour
{
    public Material overrideMaterial; // ���w��Ȃ� Sprite/Default ������

    SpriteRenderer sr;
    PolygonCollider2D polyCol;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        polyCol = GetComponent<PolygonCollider2D>();
    }

    /// <summary>���́F���[���h���W�̃h���b�O�Ȑ�</summary>
    public bool TrySliceByStroke(IList<Vector2> strokeWorld)
    {
        if (strokeWorld == null || strokeWorld.Count < 2) return false;
        if (polyCol.pathCount == 0 || polyCol.GetTotalPointCount() < 3) return false;

        // �O���i���[�J�����W�j
        var localOuter = new List<Vector2>(polyCol.GetPath(0));

        // �X�g���[�N�����[�J����
        List<Vector2> strokeLocal = new List<Vector2>(strokeWorld.Count);
        var tf = transform;
        for (int i = 0; i < strokeWorld.Count; i++)
            strokeLocal.Add(tf.InverseTransformPoint(strokeWorld[i]));

        // ��_��T���i�ŏ���2���̗p�j
        if (!FindPolylinePolygonIntersections(strokeLocal, localOuter, out var hitA, out var hitB))
            return false; // 2�񉡐؂��Ă��Ȃ�

        // �|�����C����̐ؒf��ԁiA��B�j���擾�i�����͎����j
        var cut = ExtractPolylineSegment(strokeLocal, hitA, hitB);

        // �O���� A��B / B��A �̋��E��
        var arcAB = BuildPolygonArc(localOuter, hitA.edgeIndex, hitB.edgeIndex, hitA.point, hitB.point, true);
        var arcBA = BuildPolygonArc(localOuter, hitA.edgeIndex, hitB.edgeIndex, hitA.point, hitB.point, false);

        // 2�̑��p�`
        var poly1 = new List<Vector2>(arcAB.Count + cut.Count);
        poly1.AddRange(arcAB);
        poly1.AddRange(cut);

        var poly2 = new List<Vector2>(arcBA.Count + cut.Count);
        var cutRev = new List<Vector2>(cut);
        cutRev.Reverse();
        poly2.AddRange(arcBA);
        poly2.AddRange(cutRev);

        // �O�p�����ă��b�V���𐶐�
        CreatePieceMesh("Piece_A", poly1);
        CreatePieceMesh("Piece_B", poly2);

        // �������_���[�͉B��/�j���i���D�݂Łj
        sr.enabled = false;
        polyCol.enabled = false;

        return true;
    }

    #region --- geometry ---

    struct Intersection
    {
        public Vector2 point;
        public int edgeIndex;   // polygon �ӂ̃C���f�b�N�X�iv[i]��v[i+1]�j
        public int polyIndex;   // stroke ���̋�ԏI�[�C���f�b�N�X�ii-1��i �� i�j
    }

    static bool FindPolylinePolygonIntersections(List<Vector2> polyline, List<Vector2> polygon,
        out Intersection a, out Intersection b)
    {
        a = default; b = default;
        var hits = new List<Intersection>();

        for (int i = 1; i < polyline.Count; i++)
        {
            Vector2 p0 = polyline[i - 1];
            Vector2 p1 = polyline[i];
            for (int e = 0; e < polygon.Count; e++)
            {
                Vector2 q0 = polygon[e];
                Vector2 q1 = polygon[(e + 1) % polygon.Count];
                if (SegmentIntersection(p0, p1, q0, q1, out Vector2 ip))
                {
                    hits.Add(new Intersection { point = ip, edgeIndex = e, polyIndex = i });
                    if (hits.Count == 2) { a = hits[0]; b = hits[1]; return true; }
                }
            }
        }
        return false;
    }

    static bool SegmentIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 ip)
    {
        ip = default;
        Vector2 r = p2 - p1;
        Vector2 s = q2 - q1;
        float rxs = Cross(r, s);
        float qpxr = Cross(q1 - p1, r);
        if (Mathf.Abs(rxs) < 1e-7f) return false; // ���s�i�����͖����j
        float t = Cross(q1 - p1, s) / rxs;
        float u = Cross(q1 - p1, r) / rxs;
        if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
        {
            ip = p1 + t * r;
            return true;
        }
        return false;
    }

    static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    static List<Vector2> ExtractPolylineSegment(List<Vector2> polyline, Intersection a, Intersection b)
    {
        var seg = new List<Vector2>();
        if (a.polyIndex <= b.polyIndex)
        {
            for (int i = a.polyIndex - 1; i <= b.polyIndex; i++)
                seg.Add(polyline[Mathf.Clamp(i, 0, polyline.Count - 1)]);
        }
        else
        {
            for (int i = a.polyIndex - 1; i >= b.polyIndex; i--)
                seg.Add(polyline[Mathf.Clamp(i, 0, polyline.Count - 1)]);
        }
        // �[����_�ɍ����ւ�
        if (seg.Count > 0) seg[0] = a.point;
        if (seg.Count > 0) seg[seg.Count - 1] = b.point;
        return seg;
    }

    static List<Vector2> BuildPolygonArc(List<Vector2> polygon, int edgeA, int edgeB, Vector2 ptA, Vector2 ptB, bool ccw)
    {
        // polygon �� CCW �z��iPolygonCollider2D �͒ʏ� CCW�j
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

    static bool IsConvex(Vector2 a, Vector2 b, Vector2 c) => Cross(b - a, c - b) > 0f;

    static bool PointInTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float c1 = Cross(b - a, p - a);
        float c2 = Cross(c - b, p - b);
        float c3 = Cross(a - c, p - c);
        bool neg = (c1 < 0) || (c2 < 0) || (c3 < 0);
        bool pos = (c1 > 0) || (c2 > 0) || (c3 > 0);
        return !(neg && pos);
    }

    static List<int> EarClip(List<Vector2> poly)
    {
        // CCW �ɂ��낦��
        if (SignedArea(poly) < 0f) poly.Reverse();

        var tris = new List<int>();
        var V = new List<int>(poly.Count);
        for (int i = 0; i < poly.Count; i++) V.Add(i);

        int guard = 0;
        while (V.Count > 3 && guard++ < 10000)
        {
            bool clipped = false;
            for (int i = 0; i < V.Count; i++)
            {
                int i0 = V[(i - 1 + V.Count) % V.Count];
                int i1 = V[i];
                int i2 = V[(i + 1) % V.Count];

                if (!IsConvex(poly[i0], poly[i1], poly[i2])) continue;

                bool contains = false;
                for (int j = 0; j < V.Count; j++)
                {
                    int vj = V[j];
                    if (vj == i0 || vj == i1 || vj == i2) continue;
                    if (PointInTri(poly[vj], poly[i0], poly[i1], poly[i2])) { contains = true; break; }
                }
                if (contains) continue;

                tris.Add(i0); tris.Add(i1); tris.Add(i2);
                V.RemoveAt(i);
                clipped = true;
                break;
            }
            if (!clipped) break;
        }
        if (V.Count == 3) { tris.Add(V[0]); tris.Add(V[1]); tris.Add(V[2]); }
        return tris;
    }

    #endregion

    #region --- mesh build ---

    void CreatePieceMesh(string name, List<Vector2> polygonLocal)
    {
        if (polygonLocal.Count < 3) return;

        var triangles = EarClip(new List<Vector2>(polygonLocal));
        if (triangles.Count < 3) return;

        // Sprite ��UV�v�Z�ibounds��textureRect����Z�o�j
        var sprite = sr.sprite;
        var tex = sprite.texture;
        var texRect = sprite.textureRect; // �s�N�Z��
        // ���[�J�����W�ł̋�`�i�X�v���C�g�̌����ڔ͈́j
        var b = sprite.bounds; // ���[�J���P��
        float minX = b.min.x, minY = b.min.y;
        float sizeX = b.size.x, sizeY = b.size.y;

        var go = new GameObject(name);
        go.transform.SetPositionAndRotation(transform.position, transform.rotation);
        go.transform.localScale = transform.localScale;

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();

        // �}�e���A��
        if (overrideMaterial != null)
            mr.sharedMaterial = overrideMaterial;
        else
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mr.sharedMaterial = mat;
        }
        mr.sharedMaterial.mainTexture = tex;
        mr.sortingLayerID = sr.sortingLayerID;
        mr.sortingOrder = sr.sortingOrder;

        // ���_/UV
        var verts = new Vector3[polygonLocal.Count];
        var uvs = new Vector2[polygonLocal.Count];
        for (int i = 0; i < polygonLocal.Count; i++)
        {
            Vector2 p = polygonLocal[i];
            verts[i] = new Vector3(p.x, p.y, 0f);

            // ���[�J����[0..1]���e�N�X�`��UV�i�A�g���X�Ή��j
            float nx = (p.x - minX) / sizeX;
            float ny = (p.y - minY) / sizeY;
            float u = (texRect.x + nx * texRect.width) / tex.width;
            float v = (texRect.y + ny * texRect.height) / tex.height;
            uvs[i] = new Vector2(u, v);
        }

        var mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mesh.SetVertices(verts);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        // �R���C�_�i�C�Ӂj
        var pc = go.AddComponent<PolygonCollider2D>();
        pc.SetPath(0, polygonLocal.ToArray());
        pc.isTrigger = polyCol.isTrigger;

        // ������t�������ꍇ�͂�����
        // var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0.3f;
    }

    #endregion
}
