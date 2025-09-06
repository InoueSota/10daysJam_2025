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
        // �N���A���UI��\������
        groupAfterGoal.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // �E
        if (_goalDirection == 0) { goalDirectionT.text = "�A�����b�N�����F��"; }
        // ��
        else if (_goalDirection == 1) { goalDirectionT.text = "�A�����b�N�����F��"; }
        // ��
        else if (_goalDirection == 2) { goalDirectionT.text = "�A�����b�N�����F�E"; }
        // ��
        else if (_goalDirection == 3) { goalDirectionT.text = "�A�����b�N�����F��"; }
    }
    public void Reset()
    {
        groupAfterGoal.SetActive(false);
    }
}
