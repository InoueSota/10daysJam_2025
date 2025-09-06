using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SaveData
{
    public int version = 2;
    public List<StageProgress> stages = new();
    // �C��: ����ς݃X�e�[�W�Ǘ��iUI�Ō��\���ȂǂɁj
    public HashSet<string> unlocked = new HashSet<string>(); // key: $"{areaId}:{stageId}"
}

public enum ClearDirection { Up = 0, Down = 1, Left = 2, Right = 3 }

[Serializable]
public class StageProgress
{
    public string areaId;
    public string stageId;

    // 4�����̃N���A��Ԃ�z��Łiindex��ClearDirection�̏��j
    public bool[] clearedByDir = new bool[4];

    // �X�e�[�W�P�ʂ́u�N���A���o���Đ��������v
    public bool effectShown;
}

public class SimpleStageGraph : IStageGraph
{
    // ��: ����G���A���Łu�X�e�[�WID: 1-1 �� (Right) �� 1-2�v�̂悤�Ɏ�Œ�`
    private readonly Dictionary<(string areaId, string stageId, ClearDirection dir), (string areaId, string stageId)> edges
        = new()
        {
            { ("Area01","1-1", ClearDirection.Right), ("Area01","1-2") },
            { ("Area01","1-2", ClearDirection.Right), ("Area01","1-3") },
            { ("Area01","1-2", ClearDirection.Left ), ("Area01","1-1") },
            // �c�K�v�ɉ����Ēǉ�
        };

    public bool TryGetNeighbor(string areaId, string stageId, ClearDirection dir, out (string areaId, string stageId) neighbor)
    {
        if (edges.TryGetValue((areaId, stageId, dir), out var n))
        {
            neighbor = n;
            return true;
        }
        neighbor = default;
        return false;
    }
}


public static class SaveUtil
{
    private static string Key(string areaId, string stageId) => $"{areaId}:{stageId}";

    public static StageProgress GetOrCreateStage(SaveData data, string areaId, string stageId)
    {
        var s = data.stages.FirstOrDefault(x => x.areaId == areaId && x.stageId == stageId);
        if (s == null)
        {
            s = new StageProgress { areaId = areaId, stageId = stageId };
            data.stages.Add(s);
        }
        return s;
    }

    // ---- �N���A�����֘A ----
    public static void SetCleared(SaveData data, string areaId, string stageId, ClearDirection dir, bool value = true)
    {
        var s = GetOrCreateStage(data, areaId, stageId);
        s.clearedByDir[(int)dir] = value;
    }

    public static bool HasCleared(SaveData data, string areaId, string stageId, ClearDirection dir)
    {
        var s = data.stages.FirstOrDefault(x => x.areaId == areaId && x.stageId == stageId);
        return s != null && s.clearedByDir[(int)dir];
    }

    public static List<ClearDirection> GetClearedDirs(SaveData data, string areaId, string stageId)
    {
        var s = data.stages.FirstOrDefault(x => x.areaId == areaId && x.stageId == stageId);
        if (s == null) return new();
        var list = new List<ClearDirection>();
        for (int i = 0; i < 4; i++) if (s.clearedByDir[i]) list.Add((ClearDirection)i);
        return list;
    }

    // ---- ���o�t���O�i�X�e�[�W�P�ʁj ----
    public static bool IsEffectShown(SaveData data, string areaId, string stageId)
    {
        var s = data.stages.FirstOrDefault(x => x.areaId == areaId && x.stageId == stageId);
        return s != null && s.effectShown;
    }

    public static void MarkEffectShown(SaveData data, string areaId, string stageId, bool shown = true)
    {
        var s = GetOrCreateStage(data, areaId, stageId);
        s.effectShown = shown;
    }

    // ---- �X�e�[�W����i�u������v���g���ėאڃX�e�[�W���J���j----
    // ����� baseDir �ɑ΂���אڐ���`����O���t��n���ĉ�����܂��B
    public static void UnlockByBaseline(
        SaveData data,
        string areaId, string stageId,
        ClearDirection clearedDir,              // �v���C���[�����ۂɃN���A���������i�L�^�p�j
        ClearDirection baseDir,                 // ����Ɏg���g������h�i��: Right�Œ�j
        IStageGraph graph                       // �X�e�[�W�̗אڊ֌W
    )
    {
        // 1) �N���A�������L�^
        SetCleared(data, areaId, stageId, clearedDir, true);

        // 2) ������ɂ���אڃX�e�[�W���擾
        if (graph.TryGetNeighbor(areaId, stageId, baseDir, out var neighbor))
        {
            data.unlocked.Add(Key(neighbor.areaId, neighbor.stageId));
        }
    }

    // �C��: �X�e�[�W�𖾎��I�ɉ��/�m�F�������ꍇ
    public static void Unlock(SaveData data, string areaId, string stageId)
        => data.unlocked.Add(Key(areaId, stageId));

    public static bool IsUnlocked(SaveData data, string areaId, string stageId)
        => data.unlocked.Contains(Key(areaId, stageId));
}

// �X�e�[�W�אڊ֌W�̃C���^�[�t�F�[�X
public interface IStageGraph
{
    bool TryGetNeighbor(string areaId, string stageId, ClearDirection dir, out (string areaId, string stageId) neighbor);
}