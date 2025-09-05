using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 自コンポーネント
    private UndoManager undoManager;

    // 他コンポーネント
    private UIManager uiManager;

    // ゴール関係
    private bool isGoal;

    void Start()
    {
        // 自コンポーネントの取得
        undoManager = GetComponent<UndoManager>();

        // 他コンポーネントの取得
        uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
    }

    void Update()
    {
        // ゴール判定
        CheckGoal();
    }

    /// <summary>
    /// ゴール判定
    /// </summary>
    void CheckGoal()
    {
        if (!isGoal)
        {
            GameObject goalLine = GameObject.FindGameObjectWithTag("GoalLine");

            if (goalLine != null && goalLine.GetComponent<GoalLineManager>().IsGoal())
            {
                // UIの更新
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
