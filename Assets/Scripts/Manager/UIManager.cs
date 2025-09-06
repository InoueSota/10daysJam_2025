using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    

    [Header("Goal")]
    [SerializeField] private GameObject groupAfterGoal;
    [SerializeField] private Text goalDirectionT;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // Setter
    public void Goal(int _goalDirection)
    {
        // クリア後のUIを表示する
        groupAfterGoal.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // 右
        if (_goalDirection == 0) { goalDirectionT.text = "アンロック方向：左"; }
        // 上
        else if (_goalDirection == 1) { goalDirectionT.text = "アンロック方向：下"; }
        // 左
        else if (_goalDirection == 2) { goalDirectionT.text = "アンロック方向：右"; }
        // 下
        else if (_goalDirection == 3) { goalDirectionT.text = "アンロック方向：上"; }
    }
    public void Reset()
    {
        groupAfterGoal.SetActive(false);
    }
}
