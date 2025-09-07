#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class StageGraphExporter
{
    [MenuItem("Tools/StageGraph/Export JSON from Runtime")]
    public static void ExportFromRuntime()
    {
        // ランタイムで使っている Graph を取得
        var g = GameBootstrap.Graph;
        if (g == null)
        {
            Debug.LogError("GameBootstrap.Graph が初期化されていません。再生中に実行してください。");
            return;
        }

        // Graph 内部を JSON に変換（EditableJsonStageGraph に ToJson を追加しておく必要あり）
        string json = g.ToJson(pretty: true);

        // 出力先
        string outPath = "Assets/Resources/stage_graph.json";

        // 書き込み
        File.WriteAllText(outPath, json);

        // Unity にアセット更新を通知
        AssetDatabase.Refresh();

        Debug.Log($"Stage graph exported to: {outPath}");
    }

    private const string outPath = "Assets/Resources/stage_graph.json";

    [MenuItem("Tools/StageGraph/Reset JSON")]
    public static void ResetJson()
    {
        var container = new StageGraphJson(); // edges = []
        string json = JsonUtility.ToJson(container, true);

        File.WriteAllText("Assets/Resources/stage_graph.json", json);
        AssetDatabase.Refresh();

        Debug.Log("Stage graph reset (empty).");
    }

    [MenuItem("Tools/StageGraph/Export JSON (from Resources)")]
    public static void ExportFromResources()
    {
        var jsonAsset = Resources.Load<TextAsset>("stage_graph");
        if (jsonAsset == null)
        {
            Debug.LogError("Assets/Resources/stage_graph.json が見つかりません。");
            return;
        }

        string outPath = "Assets/Resources/stage_graph.json";
        File.WriteAllText(outPath, jsonAsset.text);
        AssetDatabase.Refresh();

        Debug.Log("Exported from Resources (ignoring runtime overrides).");
    }
}
#endif
