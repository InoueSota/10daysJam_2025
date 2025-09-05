using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private UndoManager undoManager;

    // ���R���|�[�l���g
    private UIManager uiManager;

    // �S�[���֌W
    private bool isGoal;

    void Start()
    {
        // ���R���|�[�l���g�̎擾
        undoManager = GetComponent<UndoManager>();

        // ���R���|�[�l���g�̎擾
        uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
    }

    void Update()
    {
        // �S�[������
        CheckGoal();
    }

    /// <summary>
    /// �S�[������
    /// </summary>
    void CheckGoal()
    {
        if (!isGoal)
        {
            GameObject goalLine = GameObject.FindGameObjectWithTag("GoalLine");

            if (goalLine != null && goalLine.GetComponent<GoalLineManager>().IsGoal())
            {
                // UI�̍X�V
                uiManager.Goal();

                isGoal = true;
            }
        }
    }

    void LateUpdate()
    {
        // Undo
        if (Input.GetButtonDown("Undo")) { undoManager.Undo(); }

        // Reset
        if (Input.GetButtonDown("Reset")) { undoManager.ResetToInitialState(); }
    }
}
