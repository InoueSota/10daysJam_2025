using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;
    private PlayerTear tear;

    // 他コンポーネント
    [SerializeField] private ChangeLayerManager changeLayerManager;

    void Start()
    {
        // 自コンポーネントを取得
        controller = GetComponent<PlayerController>();
        tear = GetComponent<PlayerTear>();

        tear.Initialize(controller);
    }

    void Update()
    {
        // レイヤー変更操作をしていない場合のみ更新
        if (!changeLayerManager.GetIsActive())
        {
            tear.ManualUpdate();

            // 破り操作をしていない場合のみ更新
            if (!tear.GetIsActive())
            {
                controller.ManualUpdate();
            }
        }

        if (Input.GetButtonDown("Reset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
