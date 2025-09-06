using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    // �v���C���[�֌W
    private Transform player;
    private PlayerCut cut;
    private PlayerController controller;

    // �u���b�N�֌W
    private List<Transform> blocks = new List<Transform>();

    // ���f���֌W
    private GameObject divisionLineObj;
    private Transform divisionLine;

    private Stack<GameState> history = new Stack<GameState>();
    private GameState initialState;

    void Start()
    {
        // �v���C���[���̎擾
        cut = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCut>();
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        // �ŏ��Ɉ�x�����u���b�N�ꗗ��o�^����
        GameObject[] blockObjects = GameObject.FindGameObjectsWithTag("FieldObject");
        foreach (GameObject blockObject in blockObjects)
        {
            blocks.Add(blockObject.transform);
        }
        divisionLineObj = GameObject.FindGameObjectWithTag("DivisionLine");
        divisionLineObj.SetActive(cut.GetIsCreateLineStart());

        // �ŏ��̏�Ԃ�ۑ�
        initialState = CaptureState();
    }
    private GameState CaptureState()
    {
        GameState state = new GameState();

        // ���f���֌W
        divisionLine = divisionLineObj.transform;
        state.divisionPosition = divisionLine.position;
        state.divisionLineActiveState = divisionLine.gameObject.activeSelf;
        state.divisionLineRotation = divisionLine.rotation;

        // �v���C���[�֌W
        player = GameObject.FindGameObjectWithTag("Player").transform;
        state.playerPosition = player.position;
        // �v���C���[�̕ϐ���ۑ�
        state.divisionPosition = cut.GetDivisionPosition();
        state.isDivision = cut.GetIsDivision();

        // �u���b�N�֌W
        state.blockPositions = new List<Vector3>();
        foreach (var block in blocks)
        {
            state.blockPositions.Add(block.position);
            state.blockPrePositions.Add(block.GetComponent<AllFieldObjectManager>().GetPrePosition());
            state.blockCurrentPositions.Add(block.GetComponent<AllFieldObjectManager>().GetCurrentPosition());
            state.blockActiveStates.Add(block.gameObject.activeSelf);
            state.blockParents.Add(block.parent);
        }

        return state;
    }
    private void RestoreState(GameState state)
    {
        // ���f���֌W
        divisionLine.position = state.divisionPosition;
        divisionLine.rotation = state.divisionLineRotation;
        divisionLine.gameObject.SetActive(state.divisionLineActiveState);

        // �v���C���[����
        player.position = state.playerPosition;
        cut.SetDivisionPosition(state.divisionPosition);
        cut.SetIsDivision(state.isDivision);
        controller.RocketInitialize();
        controller.FlagInitialize();

        // �u���b�N�֌W
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].position = state.blockPositions[i];
            blocks[i].GetComponent<AllFieldObjectManager>().SetPrePosition(state.blockPrePositions[i]);
            blocks[i].GetComponent<AllFieldObjectManager>().SetCurrentPosition(state.blockCurrentPositions[i]);
            blocks[i].gameObject.SetActive(state.blockActiveStates[i]);
            blocks[i].SetParent(state.blockParents[i]);
        }
    }

    // ���݂̏�Ԃ�ۑ�
    public void SaveState()
    {
        history.Push(CaptureState());
    }

    // �ЂƂO�ɖ߂�
    public void Undo()
    {
        if (history.Count == 0) return;

        GameState prevState = history.Pop();

        // ���f���֌W
        divisionLine.position = prevState.divisionPosition;
        divisionLine.rotation = prevState.divisionLineRotation;
        divisionLine.gameObject.SetActive(prevState.divisionLineActiveState);

        // �v���C���[����
        player.position = prevState.playerPosition;
        cut.SetDivisionPosition(prevState.divisionPosition);
        cut.SetIsDivision(prevState.isDivision);
        controller.RocketInitialize();
        controller.FlagInitialize();

        // �u���b�N�֌W
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].position = prevState.blockPositions[i];
            blocks[i].GetComponent<AllFieldObjectManager>().SetPrePosition(prevState.blockPrePositions[i]);
            blocks[i].GetComponent<AllFieldObjectManager>().SetCurrentPosition(prevState.blockCurrentPositions[i]);
            blocks[i].gameObject.SetActive(prevState.blockActiveStates[i]);
            blocks[i].SetParent(prevState.blockParents[i]);
        }
    }

    public Vector3 GetPrevPlayerPosition()
    {
        if (history.Count == 0) return Vector3.zero;

        GameState prevState = history.Peek();

        return prevState.playerPosition;
    }

    // ���Z�b�g����
    public void ResetToInitialState()
    {
        if (initialState == null) return;

        RestoreState(initialState);

        // Undo���������Z�b�g
        history.Clear();
    }
}