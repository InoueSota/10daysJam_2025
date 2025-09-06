using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    // プレイヤー関係
    private Transform player;
    private PlayerCut cut;
    private PlayerController controller;

    // ブロック関係
    private List<Transform> blocks = new List<Transform>();

    // 分断線関係
    private GameObject divisionLineObj;
    private Transform divisionLine;

    private Stack<GameState> history = new Stack<GameState>();
    private GameState initialState;

    void Start()
    {
        // プレイヤー情報の取得
        cut = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCut>();
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        // 最初に一度だけブロック一覧を登録する
        GameObject[] blockObjects = GameObject.FindGameObjectsWithTag("FieldObject");
        foreach (GameObject blockObject in blockObjects)
        {
            blocks.Add(blockObject.transform);
        }
        divisionLineObj = GameObject.FindGameObjectWithTag("DivisionLine");
        divisionLineObj.SetActive(cut.GetIsCreateLineStart());

        // 最初の状態を保存
        initialState = CaptureState();
    }
    private GameState CaptureState()
    {
        GameState state = new GameState();

        // 分断線関係
        divisionLine = divisionLineObj.transform;
        state.divisionPosition = divisionLine.position;
        state.divisionLineActiveState = divisionLine.gameObject.activeSelf;
        state.divisionLineRotation = divisionLine.rotation;

        // プレイヤー関係
        player = GameObject.FindGameObjectWithTag("Player").transform;
        state.playerPosition = player.position;
        // プレイヤーの変数を保存
        state.divisionPosition = cut.GetDivisionPosition();
        state.isDivision = cut.GetIsDivision();

        // ブロック関係
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
        // 分断線関係
        divisionLine.position = state.divisionPosition;
        divisionLine.rotation = state.divisionLineRotation;
        divisionLine.gameObject.SetActive(state.divisionLineActiveState);

        // プレイヤー復元
        player.position = state.playerPosition;
        cut.SetDivisionPosition(state.divisionPosition);
        cut.SetIsDivision(state.isDivision);
        controller.RocketInitialize();
        controller.FlagInitialize();

        // ブロック関係
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].position = state.blockPositions[i];
            blocks[i].GetComponent<AllFieldObjectManager>().SetPrePosition(state.blockPrePositions[i]);
            blocks[i].GetComponent<AllFieldObjectManager>().SetCurrentPosition(state.blockCurrentPositions[i]);
            blocks[i].gameObject.SetActive(state.blockActiveStates[i]);
            blocks[i].SetParent(state.blockParents[i]);
        }
    }

    // 現在の状態を保存
    public void SaveState()
    {
        history.Push(CaptureState());
    }

    // ひとつ前に戻す
    public void Undo()
    {
        if (history.Count == 0) return;

        GameState prevState = history.Pop();

        // 分断線関係
        divisionLine.position = prevState.divisionPosition;
        divisionLine.rotation = prevState.divisionLineRotation;
        divisionLine.gameObject.SetActive(prevState.divisionLineActiveState);

        // プレイヤー復元
        player.position = prevState.playerPosition;
        cut.SetDivisionPosition(prevState.divisionPosition);
        cut.SetIsDivision(prevState.isDivision);
        controller.RocketInitialize();
        controller.FlagInitialize();

        // ブロック関係
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

    // リセットする
    public void ResetToInitialState()
    {
        if (initialState == null) return;

        RestoreState(initialState);

        // Undo履歴もリセット
        history.Clear();
    }
}