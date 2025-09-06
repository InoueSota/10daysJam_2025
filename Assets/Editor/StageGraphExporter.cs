#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class StageGraphExporter
{
    [MenuItem("Tools/StageGraph/Export JSON from Runtime")]
    public static void ExportFromRuntime()
    {
        // �����^�C���Ŏg���Ă��� Graph ���擾
        var g = GameBootstrap.Graph;
        if (g == null)
        {
            Debug.LogError("GameBootstrap.Graph ������������Ă��܂���B�Đ����Ɏ��s���Ă��������B");
            return;
        }

        // Graph ������ JSON �ɕϊ��iEditableJsonStageGraph �� ToJson ��ǉ����Ă����K�v����j
        string json = g.ToJson(pretty: true);

        // �o�͐�
        string outPath = "Assets/Resources/stage_graph.json";

        // ��������
        File.WriteAllText(outPath, json);

        // Unity �ɃA�Z�b�g�X�V��ʒm
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
            Debug.LogError("Assets/Resources/stage_graph.json ��������܂���B");
            return;
        }

        string outPath = "Assets/Resources/stage_graph.json";
        File.WriteAllText(outPath, jsonAsset.text);
        AssetDatabase.Refresh();

        Debug.Log("Exported from Resources (ignoring runtime overrides).");
    }
}
#endif
