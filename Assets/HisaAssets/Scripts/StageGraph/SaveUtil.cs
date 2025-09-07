using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SaveData
{
    public int version = 2;
    public List<StageProgress> stages = new();
    // 任意: 解放済みステージ管理（UIで鍵表示などに）
    public HashSet<string> unlocked = new HashSet<string>(); // key: $"{areaId}:{stageId}"
}

public enum ClearDirection { Up = 0, Down = 1, Left = 2, Right = 3 }

[Serializable]
public class StageProgress
{
    public string areaId;
    public string stageId;

    // 4方向のクリア状態を配列で（indexはClearDirectionの順）
    public bool[] clearedByDir = new bool[4];

    // ステージ単位の「クリア演出を再生したか」
    public bool effectShown;
}

public class SimpleStageGraph : IStageGraph
{
    // 例: 同一エリア内で「ステージID: 1-1 → (Right) → 1-2」のように手で定義
    private readonly Dictionary<(string areaId, string stageId, ClearDirection dir), (string areaId, string stageId)> edges
        = new()
        {
            { ("Area01","1-1", ClearDirection.Right), ("Area01","1-2") },
            { ("Area01","1-2", ClearDirection.Right), ("Area01","1-3") },
            { ("Area01","1-2", ClearDirection.Left ), ("Area01","1-1") },
            // …必要に応じて追加
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

    // ---- クリア方向関連 ----
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

    // ---- 演出フラグ（ステージ単位） ----
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

    // ---- ステージ解放（「基準方向」を使って隣接ステージを開放）----
    // 基準方向 baseDir に対する隣接先を定義するグラフを渡して解放します。
    public static void UnlockByBaseline(
        SaveData data,
        string areaId, string stageId,
        ClearDirection clearedDir,              // プレイヤーが実際にクリアした方向（記録用）
        ClearDirection baseDir,                 // 解放に使う“基準方向”（例: Right固定）
        IStageGraph graph                       // ステージの隣接関係
    )
    {
        // 1) クリア方向を記録
        SetCleared(data, areaId, stageId, clearedDir, true);

        // 2) 基準方向にある隣接ステージを取得
        if (graph.TryGetNeighbor(areaId, stageId, baseDir, out var neighbor))
        {
            data.unlocked.Add(Key(neighbor.areaId, neighbor.stageId));
        }
    }

    // 任意: ステージを明示的に解放/確認したい場合
    public static void Unlock(SaveData data, string areaId, string stageId)
        => data.unlocked.Add(Key(areaId, stageId));

    public static bool IsUnlocked(SaveData data, string areaId, string stageId)
        => data.unlocked.Contains(Key(areaId, stageId));
}

// ステージ隣接関係のインターフェース
public interface IStageGraph
{
    bool TryGetNeighbor(string areaId, string stageId, ClearDirection dir, out (string areaId, string stageId) neighbor);
}