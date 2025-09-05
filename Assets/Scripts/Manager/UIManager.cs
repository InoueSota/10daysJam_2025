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
    public void Goal()
    {
        // クリア後のUIを表示する
        groupAfterGoal.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // 右
        if (player.GetComponent<PlayerController>().GetDirection() == 0) { goalDirectionT.text = "アンロック方向：左"; }
        // 上
        else if (player.GetComponent<PlayerController>().GetDirection() == 1) { goalDirectionT.text = "アンロック方向：下"; }
        // 左
        else if (player.GetComponent<PlayerController>().GetDirection() == 2) { goalDirectionT.text = "アンロック方向：右"; }
        // 下
        else if (player.GetComponent<PlayerController>().GetDirection() == 3) { goalDirectionT.text = "アンロック方向：上"; }
    }
}
