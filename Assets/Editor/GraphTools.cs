#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class GraphTools
{
    [MenuItem("Tools/StageGraph/Copy Override Å® Resources")]
    public static void CopyOverrideToResources()
    {
        string src = Path.Combine(Application.persistentDataPath, "stage_graph_override.json");
        string dst = "Assets/Resources/stage_graph.json";

        if (!File.Exists(src))
        {
            Debug.LogError("override Ç™Ç†ÇËÇ‹ÇπÇÒÅBÇ‹Ç∏ Play íÜÇ… SaveOverrideDelta/CommitUpsert Ç≈ï€ë∂ÇµÇƒÇ≠ÇæÇ≥Ç¢ÅB");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        File.Copy(src, dst, overwrite: true);
        AssetDatabase.Refresh();
        Debug.Log($"Copied override Å® {dst}");
        EditorUtility.RevealInFinder(dst);
    }
}
#endif
