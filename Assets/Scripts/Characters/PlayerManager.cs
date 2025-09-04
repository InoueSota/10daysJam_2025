using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;
    private PlayerCut cut;

    void Start()
    {
        // 自コンポーネントを取得
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
