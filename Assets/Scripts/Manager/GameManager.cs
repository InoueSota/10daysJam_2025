using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private UndoManager undoManager;

    // ���R���|�[�l���g
    private UIManager uiManager;
    private PlayerManager playerManager;

    // �S�[���֌W
    private bool isGoal;
    private enum GoalDirection { LEFT = 0, RIGHT = 2, UP = 3, DOWN = 1 }
    private GoalDirection goalDirection;

    //�X�e�[�W���
    [SerializeField] string areaName;//�ǂ̃G���A��(Area1,Area2)
    [SerializeField] string stageName;//�ǂ̃X�e�[�W��(Stage1,Stage2)

    string connectStage;

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
        SceneChange();
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
                if (!player.GetComponent<PlayerController>().GetIsRocketMoving()) { goalDirection = GoalDirection.UP; }
                else { goalDirection = (GoalDirection)player.GetComponent<PlayerController>().GetDirection(); }

                // UI�̍X�V
                uiManager.Goal((int)goalDirection);
                Debug.Log("goalDirection" + goalDirection);

                isGoal = true;

                //�אڂ���X�e�[�W�̌���

                switch (goalDirection)
                {
                    case GoalDirection.LEFT:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Left, out var lStage))
                        {
                            Debug.Log($"���� {lStage.areaId} / {lStage.stageId}");
                            connectStage = lStage.stageId;
                        }
                        else
                        {
                            Debug.Log("�אڂ����ݒ�ł�");
                        }

                        break;
                    case GoalDirection.RIGHT:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Right, out var rStage))
                        {
                            Debug.Log($"���� {rStage.areaId} / {rStage.stageId}");
                            connectStage = rStage.stageId;
                        }
                        else
                        {
                            Debug.Log("�אڂ����ݒ�ł�");
                        }
                        break;
                    case GoalDirection.UP:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Up, out var uStage))
                        {
                            Debug.Log($"���� {uStage.areaId} / {uStage.stageId}");
                            connectStage = uStage.stageId;
                        }
                        else
                        {
                            Debug.Log("�אڂ����ݒ�ł�");
                        }
                        break;
                    case GoalDirection.DOWN:
                        if (GameBootstrap.Graph.TryGetNeighbor(areaName, stageName, ClearDirection.Down, out var dStage))
                        {
                            Debug.Log($"���� {dStage.areaId} / {dStage.stageId}");
                            connectStage = dStage.stageId;
                        }
                        else
                        {
                            Debug.Log("�אڂ����ݒ�ł�");
                        }
                        break;
                    default:
                        break;


                }
                Debug.Log(connectStage);
            }
        }
        else
        {
            if (Input.GetButtonDown("Reset")) { uiManager.Reset(); isGoal = false; }
        }
    }

    void SceneChange()
    {
        if (!isGoal) { return; }
        if (Input.GetButtonDown("Select"))
        {


            if (connectStage != null)
            {
                SceneManager.LoadScene(connectStage);
            }
            else
            {
                SceneManager.LoadScene("StageSelectScene");

            }
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
