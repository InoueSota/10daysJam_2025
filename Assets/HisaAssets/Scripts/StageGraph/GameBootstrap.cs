using UnityEngine;

[DefaultExecutionOrder(-1000)] // ← これで一番早く起動
public class GameBootstrap : MonoBehaviour
{
    public static EditableJsonStageGraph Graph { get; private set; }
    [SerializeField] TextAsset baseGraphJson; // 空なら Resources から読む

    public static bool EnsureInitialized()
    {
        if (Graph != null) return true;
        var json = Resources.Load<TextAsset>("stage_graph");
        if (json == null)
        {
            Debug.LogError("Assets/Resources/stage_graph.json が見つかりません。");
            return false;
        }
        Graph = new EditableJsonStageGraph(json);
        return true;
    }

    void Awake()
    {
        if (!EnsureInitialized()) return;
        //DontDestroyOnLoad(gameObject); // 任意：シーン跨ぎで維持
    }
}
