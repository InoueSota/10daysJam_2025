using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [SerializeField] private GameObject goalLinePrefab;
    [SerializeField] private LayerMask groundLayer;

    public List<GameObject> goals;  // ゴール一覧を保持

    // ゴールペアと生成済みラインを管理
    private Dictionary<(GameObject, GameObject), GameObject> goalLines = new();
    // ゴールのスプライト参照を保持するための辞書
    private Dictionary<GameObject, int> goalLineRefCount = new Dictionary<GameObject, int>();

    private bool completeDelay;

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

        completeDelay = true;
    }

    void Update()
    {
        if (completeDelay)
        {
            if (Input.GetButtonDown("Undo") || Input.GetButtonDown("Reset")) { completeDelay = false; }

            for (int i = 0; i < goals.Count; i++)
            {
                if (goals[i] == null || goals[i].activeSelf == false) continue;
                Vector2 posA = goals[i].transform.position;

                for (int j = i + 1; j < goals.Count; j++)
                {
                    if (goals[j] == null || goals[j].activeSelf == false) continue;
                    Vector2 posB = goals[j].transform.position;

                    // 同じX軸またはY軸にいるか判定
                    if ((Mathf.Approximately(posA.x, posB.x) || Mathf.Approximately(posA.y, posB.y)) && Vector3.Distance(posA, posB) > 1.1f)
                    {
                        bool noBlock = true;

                        foreach (RaycastHit2D hit in Physics2D.RaycastAll(posA, (posB - posA).normalized, Vector3.Distance(posA, posB), groundLayer))
                        {
                            // TagがFieldObjectなら
                            if (hit && hit.collider.gameObject.CompareTag("FieldObject") && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.GOAL)
                            {
                                noBlock = false;
                                break;
                            }
                        }

                        if (noBlock)
                        {
                            var key = MakeKey(goals[i], goals[j]);

                            // 線がまだなければ生成
                            if (!goalLines.ContainsKey(key))
                            {
                                GameObject goalLineObj = Instantiate(goalLinePrefab);
                                goalLineObj.GetComponent<GoalLineManager>().Initialize(goals[i].transform, goals[j].transform, 1f);

                                goalLines[key] = goalLineObj;

                                // --- ゴールのスプライトを切り替え ---
                                UpdateGoalSprite(goals[i], +1);
                                UpdateGoalSprite(goals[j], +1);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            completeDelay = true;
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

                // --- ゴールのスプライトを元に戻す ---
                UpdateGoalSprite(goalA, -1);
                UpdateGoalSprite(goalB, -1);
            }

            // 軸が揃っていない場合
            Vector2 posA = goalA.transform.position;
            Vector2 posB = goalB.transform.position;

            bool sameAxis = Mathf.Approximately(posA.x, posB.x) || Mathf.Approximately(posA.y, posB.y);

            if (!sameAxis)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);

                // --- ゴールのスプライトを元に戻す ---
                UpdateGoalSprite(goalA, -1);
                UpdateGoalSprite(goalB, -1);
            }
            // 間にブロックがある場合
            else
            {
                bool noBlock = true;

                foreach (RaycastHit2D hit in Physics2D.RaycastAll(posA, (posB - posA).normalized, Vector3.Distance(posA, posB), groundLayer))
                {
                    // TagがFieldObjectなら
                    if (hit && hit.collider.gameObject.CompareTag("FieldObject") && hit.collider.GetComponent<AllFieldObjectManager>().GetObjectType() != AllFieldObjectManager.ObjectType.GOAL)
                    {
                        noBlock = false;
                        break;
                    }
                }

                if (!noBlock)
                {
                    Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);

                    // --- ゴールのスプライトを元に戻す ---
                    UpdateGoalSprite(goalA, -1);
                    UpdateGoalSprite(goalB, -1);
                }
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

    // ゴールの参照カウントを更新してスプライトを切り替える
    private void UpdateGoalSprite(GameObject goal, int delta)
    {
        if (goal == null) return;

        if (!goalLineRefCount.ContainsKey(goal))
            goalLineRefCount[goal] = 0;

        goalLineRefCount[goal] += delta;

        var sr = goal.GetComponent<Animator>();
        if (sr == null) return;

        bool isLineActive = false;

        if (goalLineRefCount[goal] > 0) { isLineActive = true; }

        sr.SetBool("on", isLineActive);
    }
}
