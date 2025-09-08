using System.Collections.Generic;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
    [SerializeField] private GameObject laserLinePrefab;

    public List<GameObject> lasers;  // ゴール一覧を保持

    // ゴールペアと生成済みラインを管理
    private Dictionary<(GameObject, GameObject), GameObject> laserLines = new();

    void Start()
    {
        // "FieldObject" タグを持つ全オブジェクトを取得
        GameObject[] fieldObjects = GameObject.FindGameObjectsWithTag("FieldObject");

        // Listを初期化
        lasers = new List<GameObject>();

        foreach (GameObject obj in fieldObjects)
        {
            AllFieldObjectManager manager = obj.GetComponent<AllFieldObjectManager>();
            if (manager != null && manager.GetObjectType() == AllFieldObjectManager.ObjectType.LASER)
            {
                lasers.Add(obj);
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < lasers.Count; i++)
        {
            if (lasers[i] == null || lasers[i].activeSelf == false) continue;
            Vector2 posA = lasers[i].transform.position;

            for (int j = i + 1; j < lasers.Count; j++)
            {
                if (lasers[j] == null || lasers[j].activeSelf == false) continue;
                Vector2 posB = lasers[j].transform.position;

                // 同じX軸またはY軸にいるか判定
                if (Mathf.Approximately(posA.x, posB.x) || Mathf.Approximately(posA.y, posB.y))
                {
                    var key = MakeKey(lasers[i], lasers[j]);

                    // 線がまだなければ生成
                    if (!laserLines.ContainsKey(key))
                    {
                        GameObject laserLineObj = Instantiate(laserLinePrefab);
                        laserLineObj.GetComponent<LaserLineManager>().Initialize(lasers[i].transform, lasers[j].transform, 1f);

                        laserLines[key] = laserLineObj;
                    }
                }
            }
        }

        // 無効化されたゴールのペアは線を消す
        List<(GameObject, GameObject)> toRemove = new List<(GameObject, GameObject)>();
        foreach (var kvp in laserLines)
        {
            var (laserA, laserB) = kvp.Key;
            if (laserA == null || laserB == null || !laserA.activeSelf || !laserB.activeSelf)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }

            // 軸が揃っていない場合
            Vector2 posA = laserA.transform.position;
            Vector2 posB = laserB.transform.position;

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
            laserLines.Remove(key);
        }
    }

    // ペアの順序を固定化する関数
    private (GameObject, GameObject) MakeKey(GameObject a, GameObject b)
    {
        // InstanceID を使って大小を判定
        return a.GetInstanceID() < b.GetInstanceID() ? (a, b) : (b, a);
    }
}
