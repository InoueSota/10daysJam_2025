using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;
    private PlayerTear tear;

    void Start()
    {
        // 自コンポーネントを取得
        controller = GetComponent<PlayerController>();
        tear = GetComponent<PlayerTear>();
    }

    void Update()
    {
        tear.ManualUpdate();
        controller.ManualUpdate();

        if (Input.GetButtonDown("Reset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
