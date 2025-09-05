using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private UndoManager undoManager;

    void Start()
    {
        undoManager = GetComponent<UndoManager>();
    }

    void LateUpdate()
    {
        // Undo
        if (Input.GetButtonDown("Undo")) { undoManager.Undo(); }

        // Reset
        if (Input.GetButtonDown("Reset")) { undoManager.ResetToInitialState(); }
    }
}
