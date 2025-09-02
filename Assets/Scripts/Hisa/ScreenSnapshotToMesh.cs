using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenSnapshotToMesh : MonoBehaviour
{
    public enum SourceMode { Screen, SpecificCamera }

    [Header("What to capture")]
    public SourceMode source = SourceMode.Screen; // ���or�J����
    public Camera targetCamera;                   // SpecificCamera���Ɏg�p
    public bool transparentBG = false;           // SpecificCamera���̂ݗL���i�w�i��0�j
    public int captureWidth = 0;                 // 0�Ȃ玩���iScreen/Camera��pixel�j
    public int captureHeight = 0;

    [Header("Mesh/Material")]
    public bool buildQuadToAspect = true;        // �e�N�X�`���䗦�̃N���b�h����������
    public float zPlane = 0f;
    public bool doubleSided = false;             // ���ʕ`�悵�����ꍇ�͒��_�𕡐�
    public Color tint = Color.white;             // Unlit�F�i���őS�̓����x�������\�j

    // ����
    MeshFilter mf;
    MeshRenderer mr;
    Texture2D captured;
    Material mat;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        StartCoroutine(CaptureAndApply());
    }

    // �K�v�ɂȂ����^�C�~���O�ōĎB�e�������ꍇ�ɌĂ�
    public void CaptureNow()
    {
        StartCoroutine(CaptureAndApply());
    }

    IEnumerator CaptureAndApply()
    {
        // ===== 1) �X�i�b�v�V���b�g�쐬 =====
        if (source == SourceMode.Screen)
        {
            // ��ʁiGameView�j���B�� �� UI�����܂�
            // EndOfFrame�܂ő҂��Ȃ��ƃt���[�����������Ă��Ȃ�
            yield return new WaitForEndOfFrame();

            int w = (captureWidth > 0) ? captureWidth : Screen.width;
            int h = (captureHeight > 0) ? captureHeight : Screen.height;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            tex.Apply(false, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            captured = tex;
        }
        else // SpecificCamera
        {
            if (!targetCamera) targetCamera = Camera.main;

            // �𑜓x�̓J������pixel�T�C�Y/�w��ɍ��킹�A�A�X�y�N�g�ێ�
            float aspect = Mathf.Max(0.0001f, targetCamera.aspect);
            int w, h;
            if (captureWidth > 0 && captureHeight > 0)
            {
                w = captureWidth; h = captureHeight; // ���[�U�[�w���D��
            }
            else if (captureWidth > 0)
            {
                w = captureWidth; h = Mathf.RoundToInt(w / aspect);
            }
            else if (captureHeight > 0)
            {
                h = captureHeight; w = Mathf.RoundToInt(h * aspect);
            }
            else
            {
                h = Mathf.Max(1, targetCamera.pixelHeight);
                w = Mathf.RoundToInt(h * aspect);
            }

            // ���J������G�炸�Ɉꎞ�J�����Ń����_�����O�i�A�X�y�N�g�����ŗ]���[���j
            var tempGO = new GameObject("SnapshotTempCamera");
            tempGO.transform.SetPositionAndRotation(targetCamera.transform.position, targetCamera.transform.rotation);
            var tempCam = tempGO.AddComponent<Camera>();
            tempCam.CopyFrom(targetCamera);
            tempCam.enabled = false;
            tempCam.rect = new Rect(0, 0, 1, 1);
            tempCam.aspect = (float)w / Mathf.Max(1, h);

            var prevFlags = tempCam.clearFlags;
            var prevBG = tempCam.backgroundColor;
            var prevSky = RenderSettings.skybox;
            if (transparentBG)
            {
                tempCam.clearFlags = CameraClearFlags.SolidColor;
                tempCam.backgroundColor = new Color(0, 0, 0, 0);
                RenderSettings.skybox = null;
            }

            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { name = "SnapshotRT" };
            var prevActive = RenderTexture.active;

            tempCam.targetTexture = rt;
            tempCam.Render();
            RenderTexture.active = rt;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            tex.Apply(false, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            // ��n��
            tempCam.clearFlags = prevFlags;
            tempCam.backgroundColor = prevBG;
            RenderSettings.skybox = prevSky;

            RenderTexture.active = prevActive;
            tempCam.targetTexture = null;
            Destroy(rt);
            Destroy(tempGO);

            captured = tex;
        }

        // ===== 2) �}�e���A���ɓ\�� =====
        string shaderName =
            (source == SourceMode.SpecificCamera && transparentBG) ? "Unlit/Transparent" : "Unlit/Texture";
        if (mat == null || mr.sharedMaterial == null || mr.sharedMaterial.shader.name != shaderName)
        {
            mat = new Material(Shader.Find(shaderName));
            mr.sharedMaterial = mat;
        }
        mat.mainTexture = captured;
        mat.color = tint;

        // ===== 3) �N���b�h�č\�z�i�K�v�Ȃ�j =====
        if (buildQuadToAspect && captured != null)
        {
            float aspectTex = (float)captured.width / Mathf.Max(1, captured.height);
            BuildQuad(aspectTex);
        }

        // ����
        yield break;
    }

    // �e�N�X�`���̃A�X�y�N�g������N���b�h��zPlane��ɐ����i���[�J�����_���S�j
    void BuildQuad(float aspect)
    {
        // ��{�T�C�Y�F����=1�A��=aspect�B�K�v�Ȃ�O����transform�̃X�P�[����ύX���Ďg���B
        float h = 1f;
        float w = h * aspect;

        var verts = new Vector3[]
        {
            new Vector3(-w/2f, -h/2f, zPlane),
            new Vector3(+w/2f, -h/2f, zPlane),
            new Vector3(+w/2f, +h/2f, zPlane),
            new Vector3(-w/2f, +h/2f, zPlane),
        };
        var uvs = new Vector2[]
        {
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
        };
        var tris = new int[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new Mesh { name = "SnapshotQuad" };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        if (doubleSided) MakeDoubleSided(mesh);

        mf.sharedMesh = mesh;
    }

    void MakeDoubleSided(Mesh mesh)
    {
        int vc = mesh.vertexCount;
        var verts = mesh.vertices;
        var uvs = mesh.uv;
        var tris = mesh.triangles;

        var newVerts = new Vector3[vc * 2];
        var newUVs = new Vector2[vc * 2];
        var newTris = new int[tris.Length * 2];

        for (int i = 0; i < vc; i++) { newVerts[i] = verts[i]; newUVs[i] = uvs[i]; }
        for (int i = 0; i < tris.Length; i++) newTris[i] = tris[i];

        for (int i = 0; i < vc; i++) { newVerts[i + vc] = verts[i]; newUVs[i + vc] = uvs[i]; }
        for (int i = 0; i < tris.Length; i += 3)
        {
            newTris[tris.Length + i + 0] = tris[i + 0] + vc;
            newTris[tris.Length + i + 1] = tris[i + 2] + vc;
            newTris[tris.Length + i + 2] = tris[i + 1] + vc;
        }

        mesh.vertices = newVerts;
        mesh.uv = newUVs;
        mesh.triangles = newTris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
