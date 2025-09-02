using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenSnapshotToMesh : MonoBehaviour
{
    public enum SourceMode { Screen, SpecificCamera }

    [Header("What to capture")]
    public SourceMode source = SourceMode.Screen; // 画面orカメラ
    public Camera targetCamera;                   // SpecificCamera時に使用
    public bool transparentBG = false;           // SpecificCamera時のみ有効（背景α0）
    public int captureWidth = 0;                 // 0なら自動（Screen/Cameraのpixel）
    public int captureHeight = 0;

    [Header("Mesh/Material")]
    public bool buildQuadToAspect = true;        // テクスチャ比率のクワッドを自動生成
    public float zPlane = 0f;
    public bool doubleSided = false;             // 両面描画したい場合は頂点を複製
    public Color tint = Color.white;             // Unlit色（αで全体透明度も調整可能）

    // 内部
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

    // 必要になったタイミングで再撮影したい場合に呼ぶ
    public void CaptureNow()
    {
        StartCoroutine(CaptureAndApply());
    }

    IEnumerator CaptureAndApply()
    {
        // ===== 1) スナップショット作成 =====
        if (source == SourceMode.Screen)
        {
            // 画面（GameView）を撮る → UI等も含む
            // EndOfFrameまで待たないとフレームが完成していない
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

            // 解像度はカメラのpixelサイズ/指定に合わせ、アスペクト維持
            float aspect = Mathf.Max(0.0001f, targetCamera.aspect);
            int w, h;
            if (captureWidth > 0 && captureHeight > 0)
            {
                w = captureWidth; h = captureHeight; // ユーザー指定を優先
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

            // 元カメラを触らずに一時カメラでレンダリング（アスペクト強制で余白ゼロ）
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

            // 後始末
            tempCam.clearFlags = prevFlags;
            tempCam.backgroundColor = prevBG;
            RenderSettings.skybox = prevSky;

            RenderTexture.active = prevActive;
            tempCam.targetTexture = null;
            Destroy(rt);
            Destroy(tempGO);

            captured = tex;
        }

        // ===== 2) マテリアルに貼る =====
        string shaderName =
            (source == SourceMode.SpecificCamera && transparentBG) ? "Unlit/Transparent" : "Unlit/Texture";
        if (mat == null || mr.sharedMaterial == null || mr.sharedMaterial.shader.name != shaderName)
        {
            mat = new Material(Shader.Find(shaderName));
            mr.sharedMaterial = mat;
        }
        mat.mainTexture = captured;
        mat.color = tint;

        // ===== 3) クワッド再構築（必要なら） =====
        if (buildQuadToAspect && captured != null)
        {
            float aspectTex = (float)captured.width / Mathf.Max(1, captured.height);
            BuildQuad(aspectTex);
        }

        // 完了
        yield break;
    }

    // テクスチャのアスペクト比を持つクワッドをzPlane上に生成（ローカル原点中心）
    void BuildQuad(float aspect)
    {
        // 基本サイズ：高さ=1、幅=aspect。必要なら外部でtransformのスケールを変更して使う。
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
