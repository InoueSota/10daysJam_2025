using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;
    private PlayerCut cut;

	// 他コンポーネント
	private UndoManager undoManager;

	void Start()
    {
        // 自コンポーネントを取得
        controller = GetComponent<PlayerController>();
        cut = GetComponent<PlayerCut>();

		// 他コンポーネントを取得
		undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
	}

    void Update()
    {
        cut.ManualUpdate();
        controller.ManualUpdate();

        if (Input.GetButtonDown("Reset"))
        {
            undoManager.ResetToInitialState();
        }
    }
}
