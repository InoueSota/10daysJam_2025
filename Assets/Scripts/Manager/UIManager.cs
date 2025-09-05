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
        // �N���A���UI��\������
        groupAfterGoal.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // �E
        if (player.GetComponent<PlayerController>().GetDirection() == 0) { goalDirectionT.text = "�A�����b�N�����F��"; }
        // ��
        else if (player.GetComponent<PlayerController>().GetDirection() == 1) { goalDirectionT.text = "�A�����b�N�����F��"; }
        // ��
        else if (player.GetComponent<PlayerController>().GetDirection() == 2) { goalDirectionT.text = "�A�����b�N�����F�E"; }
        // ��
        else if (player.GetComponent<PlayerController>().GetDirection() == 3) { goalDirectionT.text = "�A�����b�N�����F��"; }
    }
}
