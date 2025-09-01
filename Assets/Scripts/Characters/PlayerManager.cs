using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;
    private PlayerTear tear;

    void Start()
    {
        // ���R���|�[�l���g���擾
        controller = GetComponent<PlayerController>();
        tear = GetComponent<PlayerTear>();

        tear.Initialize(controller);
    }

    void Update()
    {
        // ���R���|�[�l���g�̍X�V
        if (!tear.GetIsActive())
        {
            controller.ManualUpdate();
        }

        if (Input.GetButtonDown("Reset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
