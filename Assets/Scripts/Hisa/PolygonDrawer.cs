using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonDrawer : MonoBehaviour
{
    [Tooltip("�|���S���̒��_ (���v���܂��͔����v���)")]
    public Vector3[] vertices;

    private void Start()
    {
        DrawPolygon(vertices);
    }

    void DrawPolygon(Vector3[] verts)
    {
        if (verts.Length < 3)
        {
            Debug.LogWarning("�|���S����`�悷��ɂ�3�ȏ�̒��_���K�v�ł�");
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "PolygonMesh";

        // ���_���Z�b�g
        mesh.vertices = verts;

        // �O�p�`�C���f�b�N�X���쐬�i�ȈՓI��Ear Clipping���g��Ȃ������p�`�����j
        int[] triangles = new int[(verts.Length - 2) * 3];
        for (int i = 0; i < verts.Length - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.triangles = triangles;

        // �@����UV�������v�Z
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // MeshFilter�ɓK�p
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;
    }
}
