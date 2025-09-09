using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Rendering;

public class PaperManagerScript : MonoBehaviour
{
    [SerializeField] PaperScript paperPrefab;
    [SerializeField] TilemapManager tilemap;
    [SerializeField] PaperScript firstPaper;
    [SerializeField] PaperScript secondPaper;
    [SerializeField] UndoManager undoManager;

    DivisionLineManager divisionLine;
    PlayerCut playerCut;
    [SerializeField] ParticleSystem[] effect;

    [SerializeField] Vector2 paperSizeBase;
    [SerializeField] Vector3 gridOffset;

    [SerializeField] Transform[] pageTransform = new Transform[2];
    [SerializeField] Vector3[] blockOffset = new Vector3[2];

    Vector2 pos;
    bool isDivision, preIsDivision;
    bool isActive, preIsActive;
    bool isCut = false;

    bool isCrash = false;

    Vector3 preVector = Vector3.zero;

    bool divisionFlag = false;

    GameManager gameManager;
    [SerializeField] GameObject sunPrefab;

    void Start()
    {
        playerCut = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerCut>();
        divisionLine = playerCut.GetDivisionLineManager();
        pos = this.transform.position;

        pageTransform[0] = tilemap.GetPage1TransForm();
        pageTransform[1] = tilemap.GetPage2TransForm();

        firstPaper.transform.parent = pageTransform[0];
        secondPaper.transform.parent = pageTransform[1];

        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();

        gameManager=FindFirstObjectByType<GameManager>();
        // 復元完了イベントを購読
        undoManager.OnStateRestored += HandleUndoRestored;

        if (playerCut.GetIsCreateLineStart())
        {
            divisionFlag = true;
        }

        if(gameManager.GetAreaName() == "Area4")
        {
            Instantiate(sunPrefab, new Vector3(0f, -0.5f,0f), Quaternion.identity);
        }
    }

    void OnDestroy()
    {
        if (undoManager != null) undoManager.OnStateRestored -= HandleUndoRestored;
    }

    // ★Undo 復元直後：保存しておいた blockOffset をそのまま復元してから Cut/Fix
    private void HandleUndoRestored(UndoManager.GameState state)
    {
        // 1) blockOffset を履歴から復元
        blockOffset[0] = state.blockOffset0;
        blockOffset[1] = state.blockOffset1;

        // 2) 状態に応じて紙を再構築
        if (state.isDivision)
        {
            bool isCutHorizontal =
                (DivisionLineManager.DivisionMode)state.divisionMode
                == DivisionLineManager.DivisionMode.HORIZONTAL;

            // ワールド座標 → 紙座標（既存式）
            Vector3 cutPos = state.divisionPosition;
            cutPos.x -= gridOffset.x;
            cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;

            CutPaper(cutPos, isCutHorizontal);
        }
        else
        {
            FixPaper();
        }
    }

    void Update()
    {
        isActive = playerCut.GetIsActive();
        isDivision = playerCut.GetIsDivision();

        if (isCut)
        {
            // Reset で紙だけ元に戻す
            if (!gameManager.GetIsGoal()&&Input.GetButtonDown("Reset"))
            {
                if (effect != null && effect.Length > 0 && effect[0] != null) effect[0].Play();
                blockOffset[0] = Vector3.zero;
                blockOffset[1] = Vector3.zero;
                FixPaper();
            }

            // Special 押下や分断解除 → “現在ページ位置”に紙を固定して 1 枚化
            if ((isActive && !preIsActive) || (!isDivision && preIsDivision))
            {
                if (effect != null && effect.Length > 0 && effect[0] != null) effect[0].Play();

                blockOffset[0] = -pageTransform[0].localPosition;
                blockOffset[1] = -pageTransform[1].localPosition;
                FixPaper();
            }
        }

        // Undoキーの直接処理はしない（UndoManagerイベントで同期）

        // 新規カット確定の瞬間
        if (playerCut.GetDivisionFlag() || divisionFlag == true)
        {
            // 切った瞬間のページ位置に追従
            blockOffset[0] = -pageTransform[0].localPosition;
            blockOffset[1] = -pageTransform[1].localPosition;

            bool isCutHorizontal =
                (divisionLine.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL);

            Vector3 cutPos = playerCut.GetDivisionPosition();
            cutPos.x -= gridOffset.x;
            cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;

            CutPaper(cutPos, isCutHorizontal);
            divisionFlag = false;
            preVector = Vector3.zero;
        }

        preIsDivision = isDivision;
        preIsActive = isActive;
    }

    public void FixPaper()
    {
        firstPaper.QuadSetter(gridOffset + blockOffset[0], paperSizeBase);
        secondPaper.QuadSetter(gridOffset + blockOffset[1], Vector3.zero);
        isCut = false;
    }

    private void CutPaper(Vector3 cutPos, bool isCutHorizontal)
    {
        Vector3 paperSize = paperSizeBase;
        Vector2 secondPaperSize = paperSizeBase;
        Vector3 secondPaperPos = gridOffset;

        if (!isCutHorizontal)
        {
            // 垂直カット（左右）
            paperSize.x = cutPos.x;
            secondPaperSize.x = paperSizeBase.x - cutPos.x;
            secondPaperPos.x = cutPos.x + gridOffset.x;
        }
        else
        {
            // 水平カット（上下）
            paperSize.y = cutPos.y;
            secondPaperSize.y = paperSizeBase.y - cutPos.y;
            secondPaperPos.y = gridOffset.y - cutPos.y;
        }

        firstPaper.QuadSetter(gridOffset + blockOffset[0], paperSize);
        secondPaper.QuadSetter(secondPaperPos + blockOffset[1], secondPaperSize);
        isCut = true;
    }

    public void SetIsDivisionFlag()
    {

        divisionFlag = true;
    }


    // ==== 追加：UndoManager から読み書きできるように公開 ====
    public Vector3 GetBlockOffset(int index) => blockOffset[Mathf.Clamp(index, 0, 1)];
    public void SetBlockOffset(int index, Vector3 v) { blockOffset[Mathf.Clamp(index, 0, 1)] = v; }

    public void CrashCut()
    {
        if(isCrash == true)
        {
            divisionFlag = true;
            isCrash = false;
        }
    }

    public void SetCrash(bool crash_)
    {
        isCrash = true;
    }
}
