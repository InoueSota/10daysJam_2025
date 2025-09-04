using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayActive : MonoBehaviour
{

    public GameObject[] targets;
    [SerializeField, Header("開始までの遅延時間")] float startDelay;
    [SerializeField, Header("間隔の時間")] float intervalDelay;

    float currentStartDelay;
    float currentIntervalDelay;
    int count;

    int totalCount;
    [SerializeField] bool random;
    [SerializeField] float max;
    public bool GetAllCount()
    {
        if (count >= totalCount) { return true; }
        else
        {
            return false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (random) {
            startDelay=Random.Range(0f, max);
        }
        foreach (GameObject target in targets)
        {
            if (target == null)
            {
                Debug.LogError(name + "がnullです");
                continue;
            }



            target.SetActive(false);
        }
        ///StartCoroutine(Simple());
        totalCount = targets.Length;



    }

    // Update is called once per frame
    void Update()
    {
        currentStartDelay += Time.deltaTime;
        if (currentStartDelay > startDelay)
        {
            Active();
        }

    }

    void Active()
    {
        if (currentIntervalDelay > 0)
        {
            currentIntervalDelay -= Time.deltaTime;
            return;
        }
        currentIntervalDelay = intervalDelay;
        if (count >= targets.Length) { return; }
        if (targets[count] == null) { count++; return; }

        targets[count].SetActive(true);
        count++;

    }

    private IEnumerator Simple()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            // 一文字ごとに0.2秒待機
            yield return new WaitForSeconds(0.2f);

            // 文字の表示数を増やしていく
            targets[i].SetActive(false);
        }
    }

    /// <summary>
    /// 現在のTargetの情報を取得する
    /// </summary>
    /// <returns></returns>
    public GameObject[] GetTargets()
    {
        return targets;
    }

    /// <summary>
    /// Activeにするかの実行時間前化を判別する
    /// </summary>
    /// <param name="detectionTime">: 何秒前から検知するか</param>
    /// <returns></returns>
    public bool GetBeforeExecute(int detectionTime)
    {
        if (currentIntervalDelay >= detectionTime || currentStartDelay >= detectionTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetCurrentStartDelay() { return currentStartDelay; }
    public float GetCurrentIntervalDelay() { return currentIntervalDelay; }

    public float GetStartDelay() { return startDelay; }
    public float GetIntervalDelay() { return intervalDelay; }
}