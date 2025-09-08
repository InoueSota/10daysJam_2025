using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GoalManager : MonoBehaviour
{
    [SerializeField] private GameObject goalLinePrefab;

    public List<GameObject> goals;  // ゴール一覧を保持

    // ゴールペアと生成済みラインを管理
    private Dictionary<(GameObject, GameObject), GameObject> goalLines = new();

    void Start()
    {
        // "FieldObject" タグを持つ全オブジェクトを取得
        GameObject[] fieldObjects = GameObject.FindGameObjectsWithTag("FieldObject");

        // Listを初期化
        goals = new List<GameObject>();

        foreach (GameObject obj in fieldObjects)
        {
            AllFieldObjectManager manager = obj.GetComponent<AllFieldObjectManager>();
            if (manager != null && manager.GetObjectType() == AllFieldObjectManager.ObjectType.GOAL)
            {
                goals.Add(obj);
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < goals.Count; i++)
        {
            if (goals[i] == null || goals[i].activeSelf == false) continue;
            Vector2 posA = goals[i].transform.position;

            for (int j = i + 1; j < goals.Count; j++)
            {
                if (goals[j] == null || goals[j].activeSelf == false) continue;
                Vector2 posB = goals[j].transform.position;

                // 同じX軸またはY軸にいるか判定
                if (Mathf.Approximately(posA.x, posB.x) || Mathf.Approximately(posA.y, posB.y))
                {
                    var key = MakeKey(goals[i], goals[j]);

                    // 線がまだなければ生成
                    if (!goalLines.ContainsKey(key))
                    {
                        GameObject goalLineObj = Instantiate(goalLinePrefab);
                        goalLineObj.GetComponent<GoalLineManager>().Initialize(goals[i].transform, goals[j].transform, 1f);

                        goalLines[key] = goalLineObj;
                    }
                }
            }
        }

        // 無効化されたゴールのペアは線を消す
        List<(GameObject, GameObject)> toRemove = new List<(GameObject, GameObject)>();
        foreach (var kvp in goalLines)
        {
            var (goalA, goalB) = kvp.Key;
            if (goalA == null || goalB == null || !goalA.activeSelf || !goalB.activeSelf)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }

            // 軸が揃っていない場合
            Vector2 posA = goalA.transform.position;
            Vector2 posB = goalB.transform.position;

            bool sameAxis = Mathf.Approximately(posA.x, posB.x) || Mathf.Approximately(posA.y, posB.y);

            if (!sameAxis)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        // Dictionaryから削除
        foreach (var key in toRemove)
        {
            goalLines.Remove(key);
        }
    }

    // ペアの順序を固定化する関数
    private (GameObject, GameObject) MakeKey(GameObject a, GameObject b)
    {
        // InstanceID を使って大小を判定
        return a.GetInstanceID() < b.GetInstanceID() ? (a, b) : (b, a);
    }
}
