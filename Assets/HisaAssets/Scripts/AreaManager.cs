using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static StageCell;
public class AreaManager : MonoBehaviour
{
    [SerializeField] string areaName;
    public StageCell curSelectStage;
    [SerializeField] TargetFollow2DScript cameraFollow;

    [SerializeField] SpriteRenderer curVisualStageImage;//UI�̃X�e�[�W�����̉摜
    [SerializeField] AmpritudePosition imageAmpritude;
    [SerializeField, Header("�X�e�[�W�A�G���A�I���̃A�j���[�V����")] Animator[] selectAnime;

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


    //�ǂ̃G���A��I�Ԃ��m�肵�����ɃX�e�[�W�I�����n�߂���悤�ɂ���,�G���A�I���ɂ��ǂ鎞�ɃX�e�[�W�I���̘g������
    public void SetSelectActive(bool active)
    {
        //curSelectStage.GetSetActive = active;
        curSelectStage.SetSelectObj(active);
        //curVisualStageImage.sprite = curSelectStage.GetStageImage();
    }

    //direction�̕����̃Z����null����Ȃ����A�N����ԂȂ�X�e�[�W�I���ł���悤�ɂ���
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


    //�I�����Ă�Z��(�X�e�[�W)�̐؂�ւ�

    public void ChangeCell(Vector2 inputDire)
    {
        //���E
        if (inputDire.x != 0 && inputDire.y == 0)
        {
            //��
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
            //��
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

        //Json���Ȃɂ���ǂݎ���āA���ɃG�t�F�N�g���o���Ă��烊�^�[��

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

                //�܂��G�t�F�N�g���o���ĂȂ�������o���āA�ۑ�����

                if (!effect)
                {
                    cells[i].StartClearEffect();
                    SaveUtil.MarkEffectShown(save, areaName, cells[i].GetStageName(), true);
                    playEffect = true;
                }

            }

        }
        if (playEffect) SaveSystem.Save(save, 1);//�C�x�ߒ��x�̏����y���A�N���A���o�����������ۑ�����
    }

    //�V�[���J�n���ɃZ�[�u��ǂݍ���ŃA�N�e�B�u��N���A��ς���
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
                //�ǂ����̕����ɃN���A���Ă鎞
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
        if (g == null) { Debug.LogError("EditableJsonStageGraph ���������� or �^�Ⴂ�ł�"); return; }

        g.BeginCapture();


        // ��������
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
        g.SaveOverrideDeltaAppend();   // �� ���G����3���g�����h�������o��
        g.EndCapture();

        // �ۑ���̎��t�@�C���ƒ��g���m�F
        string path = Path.Combine(Application.persistentDataPath, "stage_graph_override.json");
        Debug.Log($"override path: {path}");
        if (File.Exists(path))
        {
            Debug.Log("override exists.\n" + File.ReadAllText(path));
        }
        else
        {
            Debug.LogWarning("override ��������܂���i�O�������Ă΂�Ă��Ȃ�/����0���̉\���j�B");
        }
    }

    
}
