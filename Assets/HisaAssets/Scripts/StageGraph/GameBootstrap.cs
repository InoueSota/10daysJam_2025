using UnityEngine;

[DefaultExecutionOrder(-1000)] // �� ����ň�ԑ����N��
public class GameBootstrap : MonoBehaviour
{
    public static EditableJsonStageGraph Graph { get; private set; }
    [SerializeField] TextAsset baseGraphJson; // ��Ȃ� Resources ����ǂ�

    public static bool EnsureInitialized()
    {
        if (Graph != null) return true;
        var json = Resources.Load<TextAsset>("stage_graph");
        if (json == null)
        {
            Debug.LogError("Assets/Resources/stage_graph.json ��������܂���B");
            return false;
        }
        Graph = new EditableJsonStageGraph(json);
        return true;
    }

    void Awake()
    {
        if (!EnsureInitialized()) return;
        //DontDestroyOnLoad(gameObject); // �C�ӁF�V�[���ׂ��ňێ�
    }
}
