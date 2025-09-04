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
        divisionLineObj.SetActive(false);
    }

    // ���݂̏�Ԃ�ۑ�
    public void SaveState()
    {
        GameState state = new GameState();

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
            state.blockActiveStates.Add(block.gameObject.activeSelf);
            state.blockParents.Add(block.parent);
        }

        // ���f���֌W
        divisionLine = divisionLineObj.transform;
        state.divisionPosition = divisionLine.position;
        state.divisionLineActiveState = divisionLine.gameObject.activeSelf;
        state.divisionLineRotation = divisionLine.rotation;

        history.Push(state);
    }

    // �ЂƂO�ɖ߂�
    public void Undo()
    {
        if (history.Count == 0) return;

        GameState prevState = history.Pop();

        // �v���C���[����
        player.position = prevState.playerPosition;
        cut.SetDivisionPosition(prevState.divisionPosition);
        cut.SetIsDivision(prevState.isDivision);
        controller.RocketInitialize();

        // �u���b�N�֌W
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].position = prevState.blockPositions[i];
            blocks[i].gameObject.SetActive(prevState.blockActiveStates[i]);
            blocks[i].SetParent(prevState.blockParents[i]);
        }

        // ���f���֌W
        divisionLine.position = prevState.divisionPosition;
        divisionLine.rotation = prevState.divisionLineRotation;
        divisionLine.gameObject.SetActive(prevState.divisionLineActiveState);
    }
}