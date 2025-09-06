#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class GraphTools
{
    [MenuItem("Tools/StageGraph/Copy Override �� Resources")]
    public static void CopyOverrideToResources()
    {
        string src = Path.Combine(Application.persistentDataPath, "stage_graph_override.json");
        string dst = "Assets/Resources/stage_graph.json";

        if (!File.Exists(src))
        {
            Debug.LogError("override ������܂���B�܂� Play ���� SaveOverrideDelta/CommitUpsert �ŕۑ����Ă��������B");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        File.Copy(src, dst, overwrite: true);
        AssetDatabase.Refresh();
        Debug.Log($"Copied override �� {dst}");
        EditorUtility.RevealInFinder(dst);
    }
}
#endif
