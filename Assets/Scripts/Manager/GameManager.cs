using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private UndoManager undoManager;

    // ���R���|�[�l���g
    private UIManager uiManager;
    private PlayerManager playerManager;

    // �S�[���֌W
    private bool isGoal;
    private enum GoalDirection { LEFT, RIGHT, UP, DOWN }
    private GoalDirection goalDirection;

    void Start()
    {
        // ���R���|�[�l���g�̎擾
        undoManager = GetComponent<UndoManager>();

        // ���R���|�[�l���g�̎擾
        uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
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
                // �v���C���[����S�[���������擾����
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (!player.GetComponent<PlayerController>().GetIsRocketMoving()) { goalDirection = GoalDirection.DOWN; }
                else { goalDirection = (GoalDirection)player.GetComponent<PlayerController>().GetDirection(); }

                // UI�̍X�V
                uiManager.Goal((int)goalDirection);

                isGoal = true;
            }
        }
        else
        {
            if (Input.GetButtonDown("Reset")) { uiManager.Reset(); isGoal = false; }
        }
    }

    void LateUpdate()
    {
        // Undo
        if (!playerManager.GetIsDeath() && Input.GetButtonDown("Undo")) { undoManager.Undo(); }

        // Reset
        if (!playerManager.GetIsDeath() && Input.GetButtonDown("Reset")) { undoManager.ResetToInitialState(); }
    }
}
