using UnityEngine;

/// _CameraCaptureTex(グローバル) をメッシュの _BaseMap に流し込み、16:9にスケール。
[ExecuteAlways]
public class CaptureToMeshBinder : MonoBehaviour
{
    public Renderer targetRenderer;            // Quad 等
    public string globalTexName = "_CameraCaptureTex";
    public string materialTexProperty = "_BaseMap";
    public float planeHeight = 1f;             // 見た目の高さ
    public bool flipY = false;

    MaterialPropertyBlock mpb;

    void OnEnable() { if (mpb==null) mpb = new MaterialPropertyBlock(); Fit16x9(); }
    void Update() { Bind(); }

    void Bind()
    {
        if (!targetRenderer) return;
        var tex = Shader.GetGlobalTexture(globalTexName) as Texture;
        if (!tex) return;

        if (tex.filterMode != FilterMode.Point) tex.filterMode = FilterMode.Point;

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetTexture(materialTexProperty, tex);
        mpb.SetTexture("_MainTex", tex); // 互換
        targetRenderer.SetPropertyBlock(mpb);
    }

    void Fit16x9()
    {
        float aspect = 16f / 9f;
        var s = new Vector3(planeHeight * aspect, planeHeight, 1f);
        if (flipY) s.y *= -1f;
        transform.localScale = s;
    }
}
