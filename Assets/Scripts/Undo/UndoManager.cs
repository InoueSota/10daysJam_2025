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
        divisionLineObj.SetActive(false);
    }

    // 現在の状態を保存
    public void SaveState()
    {
        GameState state = new GameState();

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
            state.blockActiveStates.Add(block.gameObject.activeSelf);
            state.blockParents.Add(block.parent);
        }

        // 分断線関係
        divisionLine = divisionLineObj.transform;
        state.divisionPosition = divisionLine.position;
        state.divisionLineActiveState = divisionLine.gameObject.activeSelf;
        state.divisionLineRotation = divisionLine.rotation;

        history.Push(state);
    }

    // ひとつ前に戻す
    public void Undo()
    {
        if (history.Count == 0) return;

        GameState prevState = history.Pop();

        // プレイヤー復元
        player.position = prevState.playerPosition;
        cut.SetDivisionPosition(prevState.divisionPosition);
        cut.SetIsDivision(prevState.isDivision);
        controller.RocketInitialize();

        // ブロック関係
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].position = prevState.blockPositions[i];
            blocks[i].gameObject.SetActive(prevState.blockActiveStates[i]);
            blocks[i].SetParent(prevState.blockParents[i]);
        }

        // 分断線関係
        divisionLine.position = prevState.divisionPosition;
        divisionLine.rotation = prevState.divisionLineRotation;
        divisionLine.gameObject.SetActive(prevState.divisionLineActiveState);
    }
}