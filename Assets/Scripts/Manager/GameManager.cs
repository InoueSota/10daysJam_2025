using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 自コンポーネント
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
