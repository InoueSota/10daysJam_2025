using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static StageCell;
public class AreaManager : MonoBehaviour
{
    [SerializeField] string areaName;
    public StageCell curSelectStage;
    [SerializeField] TargetFollow2DScript cameraFollow;

    [SerializeField] SpriteRenderer curVisualStageImage;//UIのステージ内部の画像
    [SerializeField] AmpritudePosition imageAmpritude;
    [SerializeField, Header("ステージ、エリア選択のアニメーション")] Animator[] selectAnime;

    [SerializeField] Transform cellParent;
    public List<StageCell> cells = new List<StageCell>();
    [SerializeField] GameObject trophyObj;
    public int GetClearStageNum()
    {
        int clearStage = 0;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].GetSetClear)
            {
                clearStage++;
            }
        }
        return clearStage;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curSelectStage.GetSetActive = true;
        curSelectStage.SetSelectObj(false);
        curVisualStageImage.sprite = curSelectStage.GetStageImage();

        for (int i = 0; i < cellParent.childCount; i++)
        {
            cells.Add(cellParent.GetChild(i).GetComponent<StageCell>());
        }

        if (GetClearStageNum() == cellParent.childCount)
        {
            trophyObj.SetActive(true);
        }
        else
        {
            trophyObj.SetActive(false);
        }
        ActiveDateLoad();

        StageGraph();
    }


    //どのエリアを選ぶか確定した時にステージ選択を始められるようにする,エリア選択にもどる時にステージ選択の枠を消す
    public void SetSelectActive(bool active)
    {
        //curSelectStage.GetSetActive = active;
        curSelectStage.SetSelectObj(active);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }

    //directionの方向のセルがnullじゃないかつ、起動状態ならステージ選択できるようにする
    void SelectCell(StageDirection direction)
    {
        if (curSelectStage.GetStageCell(direction) != null && curSelectStage.GetStageCell(direction).GetSetActive)
        {
            curSelectStage.GetStageCell(direction).SetSelectObj(true);
            curSelectStage.SetSelectObj(false);
            curSelectStage = curSelectStage.GetStageCell(direction);
            cameraFollow.SetTarget(curSelectStage.transform);
            curVisualStageImage.sprite = curSelectStage.GetStageImage();
            imageAmpritude.EaseStart();
        }
    }


    //選択してるセル(ステージ)の切り替え

    public void ChangeCell(Vector2 inputDire)
    {
        //左右
        if (inputDire.x != 0 && inputDire.y == 0)
        {
            //左
            if (inputDire.x < 0)
            {
                SelectCell(StageDirection.left);
            }
            else
            {
                SelectCell(StageDirection.right);
            }
        }
        else if (inputDire.x == 0 && inputDire.y != 0)
        {
            //左
            if (inputDire.y < 0)
            {
                SelectCell(StageDirection.down);
            }
            else
            {
                SelectCell(StageDirection.up);
            }
        }
    }

    public void AreaSelectAnime(bool flag)
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", flag);
        }
    }

    public void AreaSelectAnime(string triggerName)
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetTrigger(triggerName);
        }
    }


    public void ClearEffect()
    {
        bool playEffect = false;

        //Jsonかなにかを読み取って、既にエフェクトを出してたらリターン

        var loaded = SaveSystem.Load(1);
        SaveData save = SaveSystem.Load(1) ?? new SaveData();
        if (loaded != null)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var dirs = SaveUtil.GetClearedDirs(loaded, areaName, cells[i].GetStageName());

                if (dirs.Count == 0)
                {
                    continue;
                }

                bool effect = SaveUtil.IsEffectShown(loaded, areaName, cells[i].GetStageName());

                //まだエフェクトを出してなかったら出して、保存する

                if (!effect)
                {
                    cells[i].StartClearEffect();
                    SaveUtil.MarkEffectShown(save, areaName, cells[i].GetStageName(), true);
                    playEffect = true;
                }

            }

        }
        if (playEffect) SaveSystem.Save(save, 1);//気休め程度の処理軽減、クリア演出した時だけ保存する
    }

    //シーン開始時にセーブを読み込んでアクティブやクリアを変える
    public void ActiveDateLoad()
    {
        var loaded = SaveSystem.Load(1);
        if (loaded != null)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var dirs = SaveUtil.GetClearedDirs(loaded, areaName, cells[i].GetStageName());


                if (dirs.Count == 0)
                {
                    continue;
                }
                //どこかの方向にクリアしてる時
                cells[i].GetSetClear = true;

                if (dirs.Contains(ClearDirection.Up))
                {
                    if (cells[i].GetStageCell(StageDirection.up)) cells[i].GetStageCell(StageDirection.up).GetSetActive = true;
                }
                if (dirs.Contains(ClearDirection.Down))
                {
                    if (cells[i].GetStageCell(StageDirection.down)) cells[i].GetStageCell(StageDirection.down).GetSetActive = true;
                }
                if (dirs.Contains(ClearDirection.Left))
                {
                    if (cells[i].GetStageCell(StageDirection.left)) cells[i].GetStageCell(StageDirection.left).GetSetActive = true;
                }
                if (dirs.Contains(ClearDirection.Right))
                {
                    if (cells[i].GetStageCell(StageDirection.right)) cells[i].GetStageCell(StageDirection.right).GetSetActive = true;
                }
            }

        }
    }



    void StageGraph()
    {
        var g = GameBootstrap.Graph as EditableJsonStageGraph;
        if (g == null) { Debug.LogError("EditableJsonStageGraph が未初期化 or 型違いです"); return; }

        g.BeginCapture();


        // 書き込み
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].GetStageCell(StageDirection.up))
            {
                g.CommitUpsertAppend(areaName, cells[i].GetStageName(), ClearDirection.Up, areaName, cells[i].GetStageCell(StageDirection.up).GetStageName());

            }
            if (cells[i].GetStageCell(StageDirection.down))
            {
                g.CommitUpsertAppend(areaName, cells[i].GetStageName(), ClearDirection.Down, areaName, cells[i].GetStageCell(StageDirection.down).GetStageName());

            }
            if (cells[i].GetStageCell(StageDirection.left))
            {
                g.CommitUpsertAppend(areaName, cells[i].GetStageName(), ClearDirection.Left, areaName, cells[i].GetStageCell(StageDirection.left).GetStageName());

            }
            if (cells[i].GetStageCell(StageDirection.right))
            {
                g.CommitUpsertAppend(areaName, cells[i].GetStageName(), ClearDirection.Right, areaName, cells[i].GetStageCell(StageDirection.right).GetStageName());

            }
        }
        g.SaveOverrideDeltaAppend();   // ← 今触った3件“だけ”を書き出し
        g.EndCapture();

        // 保存先の実ファイルと中身を確認
        string path = Path.Combine(Application.persistentDataPath, "stage_graph_override.json");
        Debug.Log($"override path: {path}");
        if (File.Exists(path))
        {
            Debug.Log("override exists.\n" + File.ReadAllText(path));
        }
        else
        {
            Debug.LogWarning("override が見つかりません（前処理が呼ばれていない/差分0件の可能性）。");
        }
    }

    
}
