using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;
    private PlayerCut cut;

	// ���R���|�[�l���g
	private UndoManager undoManager;

	void Start()
    {
        // ���R���|�[�l���g���擾
        controller = GetComponent<PlayerController>();
        cut = GetComponent<PlayerCut>();

		// ���R���|�[�l���g���擾
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
