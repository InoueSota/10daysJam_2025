using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;
    private PlayerCut cut;

    void Start()
    {
        // ���R���|�[�l���g���擾
        controller = GetComponent<PlayerController>();
        cut = GetComponent<PlayerCut>();
    }

    void Update()
    {
        cut.ManualUpdate();
        controller.ManualUpdate();

        if (Input.GetButtonDown("Reset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
