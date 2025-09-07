using System.Drawing;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PaperManagerScript : MonoBehaviour
{
    [SerializeField] PaperScript paperPrefab;
    [SerializeField] TilemapManager tilemap;
    [SerializeField] PaperScript firstPaper;
    [SerializeField] PaperScript secondPaper;
    [SerializeField] UndoManager undoManager;
    DivisionLineManager divisionLine;
    PlayerCut playerCut;
    [SerializeField]
    ParticleSystem[] effect;

    [SerializeField] Vector2 paperSizeBase;
    [SerializeField] Vector3 gridOffset;

    [SerializeField] Transform[] pageTransform = new Transform[2];
    [SerializeField] Vector3[] blockOffset = new Vector3[2];

    Vector2 pos;
    bool isDivision, preIsDivision;
    bool isActive, preIsActive;
    bool isCut = false;

    void Start()
    {
        playerCut = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerCut>();
        divisionLine = playerCut.GetDivisionLineManager();
        pos = this.transform.position;

        pageTransform[0] = tilemap.GetPage1TransForm();
        pageTransform[1] = tilemap.GetPage2TransForm();

        firstPaper.transform.parent = pageTransform[0];
        secondPaper.transform.parent = pageTransform[1];

        undoManager = GameObject.FindGameObjectWithTag("GameController")
                        .GetComponent<UndoManager>();

        // ★追加：復元完了イベントを購読
        undoManager.OnStateRestored += HandleUndoRestored;
    }

    void OnDestroy()
    {
        if (undoManager != null)
            undoManager.OnStateRestored -= HandleUndoRestored;
    }

    // ★追加: Undo 復元直後に必ず呼ばれるハンドラ
    private void HandleUndoRestored(GameState state)
    {
        // ★まず紙を“ページの現在位置”に合わせる
        blockOffset[0] = -pageTransform[0].localPosition;
        blockOffset[1] = -pageTransform[1].localPosition;

        if (state.isDivision)
        {
            bool isCutHorizontal =
                (DivisionLineManager.DivisionMode)state.divisionMode
                == DivisionLineManager.DivisionMode.HORIZONTAL;

            // ワールドの分断位置 → 紙座標へ（既存式を踏襲）
            Vector3 cutPos = state.divisionPosition;
            cutPos.x -= gridOffset.x;
            cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;

            // ★ページ位置に揃えた blockOffset を使ってから切る
            CutPaper(cutPos, isCutHorizontal);
        }
        else
        {
            // 分断なしへ復元：ページ位置に揃えてから1枚化
            FixPaper();
        }
    }

    void Update()
    {
        isActive = playerCut.GetIsActive();
        isDivision = playerCut.GetIsDivision();

        if (isCut == true)
        {
            // Reset で紙だけ元に戻す挙動はそのまま
            if (Input.GetButtonDown("Reset"))
            {
                effect[0].Play();
                blockOffset[0] = Vector3.zero;
                blockOffset[1] = Vector3.zero;
                FixPaper();
            }

            // Special の押下や分断解除で “紙を今のページ位置に追従させて” 固定
            if ((isActive == true && preIsActive == false) ||
                (isDivision == false && preIsDivision == true))
            {
                effect[0].Play();
                blockOffset[0] = -pageTransform[0].localPosition;
                blockOffset[1] = -pageTransform[1].localPosition;
                FixPaper();
            }
        }

        // ★削除: Undo キーを直接見て紙を切る処理
        // if (Input.GetButtonDown("Undo")) { … } を **丸ごと削除** してください。
        // 紙の更新は HandleUndoRestored() に一本化。

        // プレイヤーが“新規に切った”瞬間はこれまで通り
        if (playerCut.GetDivisionFlag() == true)
        {
            blockOffset[0] = -pageTransform[0].localPosition;
            blockOffset[1] = -pageTransform[1].localPosition;

            bool isCutHorizontal = false;
            if (divisionLine.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL) isCutHorizontal = true;

            Vector3 cutPos = playerCut.GetDivisionPosition();
            cutPos.x -= gridOffset.x;
            cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;
            CutPaper(cutPos, isCutHorizontal);
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
            paperSize.x = cutPos.x;
            secondPaperSize.x = paperSizeBase.x - cutPos.x;
            secondPaperPos.x = cutPos.x + gridOffset.x;
        }
        else
        {
            paperSize.y = cutPos.y;
            secondPaperSize.y = paperSizeBase.y - cutPos.y;
            secondPaperPos.y = gridOffset.y - cutPos.y;
        }

        firstPaper.QuadSetter(gridOffset + blockOffset[0], paperSize);
        secondPaper.QuadSetter(secondPaperPos + blockOffset[1], secondPaperSize);
        isCut = true;
    }

}
