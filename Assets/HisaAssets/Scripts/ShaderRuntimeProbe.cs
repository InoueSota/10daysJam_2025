using UnityEngine;

public class ShaderRuntimeProbe : MonoBehaviour
{
    public Renderer targetRenderer;   // Renderer 経路で使うならセット
    public UnityEngine.UI.Graphic targetGraphic; // UI経路で使うならセット

    const string kPath = "Hisa/URP/VerticalScrollGradientRampBlend_DitherEase";

    void Start()
    {
        var found = Shader.Find(kPath);
        Debug.Log($"[Probe] Shader.Find('{kPath}') => {(found ? found.name : "NULL")}");

        Material mat = null;
        if (targetRenderer) mat = targetRenderer.sharedMaterial ?? targetRenderer.material;
        if (!mat && targetGraphic) mat = targetGraphic.material;

        if (!mat)
        {
            Debug.LogError("[Probe] Material is NULL on target.");
            return;
        }

        Debug.Log($"[Probe] Material.name={mat.name}, shader={(mat.shader ? mat.shader.name : "NULL")}");

        if (mat.shader != null)
        {
            Debug.Log($"[Probe] shader.isSupported={mat.shader.isSupported}");
            Debug.Log($"[Probe] same shader object? {(found && mat.shader == found)}");
        }

        // 参照テクスチャの有無も確認（nullだと紫の誘因）
        var a = mat.GetTexture("_RampA");
        var b = mat.GetTexture("_RampB");
        Debug.Log($"[Probe] _RampA={(a ? a.name : "NULL")} _RampB={(b ? b.name : "NULL")}");
    }
}
