using System;
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

    // 各ブロック
    private List<Transform> crabs = new List<Transform>();
    private List<Transform> switches = new List<Transform>();

    // 分断線関係
    private GameObject divisionLineObj;
    private Transform divisionLine;

    // ★Paper 参照（blockOffset を保存するため）
    [SerializeField] private PaperManagerScript paper;

    private Stack<GameState> history = new Stack<GameState>();
    private GameState initialState;

    // === 履歴に乗せる状態 ===
    [Serializable]
    public class GameState
    {
        // 分断線
        public Vector3 divisionPosition;
        public Quaternion divisionLineRotation;
        public bool divisionLineActiveState;
        public int divisionMode;   // DivisionLineManager.DivisionMode の int

        // プレイヤー／分断状態
        public Vector3 playerPosition;
        public bool isDivision;
        public GameObject warpObj;

        // ブロックの親
        public Vector3 objectParentPosition1;
        public Vector3 objectParentPosition2;

        // ブロック群
        public List<Vector3> blockPositions = new List<Vector3>();
        public List<Vector3> blockPrePositions = new List<Vector3>();
        public List<Vector3> blockCurrentPositions = new List<Vector3>();
        public List<bool> blockActiveStates = new List<bool>();
        public List<Transform> blockParents = new List<Transform>();

        // 各ブロック
        public List<int> crabThrowDirection = new List<int>();
        public List<int> switchStatus = new List<int>();

        // ★Paper のオフセット（今回の要望）
        public Vector3 blockOffset0;
        public Vector3 blockOffset1;
    }

    // 復元完了イベント（Paper がこれを受け取って再構築）
    public event Action<GameState> OnStateRestored;

    void Start()
    {
        // 参照取得
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        player = playerGo.transform;
        cut = playerGo.GetComponent<PlayerCut>();
        controller = playerGo.GetComponent<PlayerController>();

        foreach (GameObject blockObject in GameObject.FindGameObjectsWithTag("FieldObject"))
        {
            blocks.Add(blockObject.transform);
            if (blockObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.CRAB)
            {
                crabs.Add(blockObject.transform);
            }
            else if (blockObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.SWITCH)
            {
                switches.Add(blockObject.transform);
            }
        }

        divisionLineObj = GameObject.FindGameObjectWithTag("DivisionLine");
        divisionLineObj.SetActive(cut.GetIsCreateLineStart());

        if (paper == null) paper = FindObjectOfType<PaperManagerScript>();

        // 初期状態
        initialState = CaptureState();
    }

    // === 現在の状態を保存 ===
    public void SaveState()
    {
        history.Push(CaptureState());
    }

    // === ひとつ前に戻す ===
    public void Undo()
    {
        if (history.Count > 0)
        {
            RestoreState(history.Pop());
        }
        else
        {
            // ★スタックが無ければ初期状態に戻す
            ResetToInitialState();
        }
    }

    // === リセット ===
    public void ResetToInitialState()
    {
        if (initialState == null) return;
        RestoreState(initialState);
        history.Clear();
    }

    private GameState CaptureState()
    {
        GameState state = new GameState();

        // 分断線
        divisionLine = divisionLineObj.transform;
        state.divisionPosition = divisionLine.position;
        state.divisionLineActiveState = divisionLine.gameObject.activeSelf;
        state.divisionLineRotation = divisionLine.rotation;
        state.divisionMode = (int)divisionLine.GetComponent<DivisionLineManager>().GetDivisionMode();

        // プレイヤー
        player = GameObject.FindGameObjectWithTag("Player").transform;
        state.playerPosition = player.position;
        state.isDivision = cut.GetIsDivision();
        state.warpObj = controller.GetWarpObj();

        // 親
        state.objectParentPosition1 = cut.GetObjectTransform(1).position;
        state.objectParentPosition2 = cut.GetObjectTransform(2).position;

        // ブロック
        state.blockPositions.Clear();
        state.blockPrePositions.Clear();
        state.blockCurrentPositions.Clear();
        state.blockActiveStates.Clear();
        state.blockParents.Clear();

        foreach (var block in blocks)
        {
            state.blockPositions.Add(block.position);
            var af = block.GetComponent<AllFieldObjectManager>();
            state.blockPrePositions.Add(af.GetPrePosition());
            state.blockCurrentPositions.Add(af.GetCurrentPosition());
            state.blockActiveStates.Add(block.gameObject.activeSelf);
            state.blockParents.Add(block.parent);
        }

        // 各ブロック（蟹）
        state.crabThrowDirection.Clear();
        foreach (var crab in crabs)
        {
            state.crabThrowDirection.Add((int)crab.GetComponent<CrabManager>().GetThrowDirection());
        }
        state.switchStatus.Clear();
        foreach (var switchs in switches)
        {
            state.switchStatus.Add((int)switchs.GetComponent<SwitchManager>().GetStatus());
        }

        // ★Paper の blockOffset を保存
        if (paper != null)
        {
            state.blockOffset0 = paper.GetBlockOffset(0);
            state.blockOffset1 = paper.GetBlockOffset(1);
        }

        return state;
    }

    private void RestoreState(GameState state)
    {
        // 分断線
        divisionLine.position = state.divisionPosition;
        divisionLine.rotation = state.divisionLineRotation;
        divisionLine.gameObject.SetActive(state.divisionLineActiveState);
        divisionLine.GetComponent<DivisionLineManager>()
                    .Initialize((DivisionLineManager.DivisionMode)state.divisionMode);

        // プレイヤー
        player.position = state.playerPosition;
        cut.SetDivisionPosition(state.divisionPosition);
        cut.SetIsDivision(state.isDivision);
        controller.RocketInitialize();
        controller.FlagInitialize();
        controller.SetWarpObj(state.warpObj);

        // 親
        cut.GetObjectTransform(1).position = state.objectParentPosition1;
        cut.GetObjectTransform(2).position = state.objectParentPosition2;

        // ブロック
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].position = state.blockPositions[i];
            var af = blocks[i].GetComponent<AllFieldObjectManager>();
            af.SetPrePosition(state.blockPrePositions[i]);
            af.SetCurrentPosition(state.blockCurrentPositions[i]);
            blocks[i].gameObject.SetActive(state.blockActiveStates[i]);
            blocks[i].SetParent(state.blockParents[i]);
        }

        // 各ブロック
        for (int i = 0; i < crabs.Count; i++)
        {
            crabs[i].GetComponent<CrabManager>().SetThrowDirection((CrabManager.ThrowDirection)state.crabThrowDirection[i]);
        }
        for (int i = 0; i < switches.Count; i++)
        {
            switches[i].GetComponent<SwitchManager>().SetStatus((SwitchManager.Status)state.switchStatus[i]);
        }

        // ★Paper 側にも blockOffset を反映（イベントで受けてもらう）
        if (paper != null)
        {
            paper.SetBlockOffset(0, state.blockOffset0);
            paper.SetBlockOffset(1, state.blockOffset1);
        }

        // 復元完了を通知（Paper が blockOffset を使って Cut/Fix を再構築）
        OnStateRestored?.Invoke(state);
    }

    // 既存の Peek 系ユーティリティはそのまま（必要なら使用）
    public Vector3 GetPrevPlayerPosition()
    {
        if (history.Count == 0) return Vector3.zero;
        return history.Peek().playerPosition;
    }
    public Vector3 GetPrevDivisionPosition()
    {
        if (history.Count == 0) return Vector3.zero;
        return history.Peek().divisionPosition;
    }
    public bool GetIsDivision()
    {
        if (history.Count == 0) return false;
        return history.Peek().isDivision;
    }
    public int GetIsDivisionMode()
    {
        if (history.Count == 0) return 1;
        return history.Peek().divisionMode;
    }
    public Vector3 GetObjectParentPosition1()
    {
        if (history.Count == 0) return Vector3.zero;
        return history.Peek().objectParentPosition1;
    }
    public Vector3 GetObjectParentPosition2()
    {
        if (history.Count == 0) return Vector3.zero;
        return history.Peek().objectParentPosition2;
    }
}
