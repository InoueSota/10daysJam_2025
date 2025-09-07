using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class StageGraphJson
{
    public List<Edge> edges = new();

    [Serializable]
    public class Edge
    {
        public string areaId;
        public string stageId;
        public string dir; // "Up", "Down", "Left", "Right"
        public string neighborAreaId;
        public string neighborStageId;
    }
}
public class JsonStageGraph : IStageGraph
{
    protected readonly Dictionary<(string area, string stage, ClearDirection dir), (string area, string stage)> map = new();

    public JsonStageGraph(TextAsset jsonAsset)
    {
        var data = JsonUtility.FromJson<StageGraphJson>(jsonAsset.text);
        Ingest(data);
    }

    protected void Ingest(StageGraphJson data)
    {
        map.Clear();
        foreach (var e in data.edges)
        {
            var parsed = (ClearDirection)Enum.Parse(typeof(ClearDirection), e.dir);
            map[(e.areaId, e.stageId, parsed)] = (e.neighborAreaId, e.neighborStageId);
        }
    }

    public bool TryGetNeighbor(string areaId, string stageId, ClearDirection dir, out (string areaId, string stageId) neighbor)
    {
        if (map.TryGetValue((areaId, stageId, dir), out var n))
        {
            neighbor = n;
            return true;
        }
        neighbor = default;
        return false;
    }
}


/// <summary>
/// JSONベースの編集可能グラフ。
/// - base: Resources の stage_graph.json（読み取り専用）
/// - override: persistentDataPath/stage_graph_override.json（書き込み可）
///
/// 特徴:
/// - Upsert/Remove で current を更新し、すぐにランタイムへ反映
/// - SaveOverride(): current 全体を override に保存
/// - CommitUpsert(): 直前の1件だけを override に保存（「今書いた分だけ」）
/// - BeginCapture/SaveOverrideDelta(): セッション中に触った分だけを差分保存
/// - ClearOverride(): override を削除（base のみへ）
/// - ClearEdges(): current を空に（map再構築）
/// </summary>
public class EditableJsonStageGraph : IStageGraph
{
    // ルックアップ用マップ（area, stage, dir -> neighbor(area, stage)）
    private readonly Dictionary<(string area, string stage, ClearDirection dir), (string area, string stage)> map
        = new();

    // 現在の実体（base + override を反映したもの）
    private StageGraphJson current = new StageGraphJson();

    // 追記保存ファイル（書き込み先）
    private readonly string overridePath;

    // セッション差分トラッキング
    private readonly HashSet<(string area, string stage, ClearDirection dir)> sessionDirty
        = new();
    private readonly HashSet<(string area, string stage, ClearDirection dir)> sessionRemoved
        = new();

    /// <param name="baseJson">Resources からロードした TextAsset (例: Resources.Load&lt;TextAsset&gt;("stage_graph"))</param>
    /// <param name="overrideFileName">persistentDataPath に作る上書きファイル名。既定: stage_graph_override.json</param>
    public EditableJsonStageGraph(TextAsset baseJson, string overrideFileName = "stage_graph_override.json")
    {
        if (baseJson == null) throw new ArgumentNullException(nameof(baseJson), "baseJson is null. Place Assets/Resources/stage_graph.json.");

        // base 反映
        current = JsonUtility.FromJson<StageGraphJson>(baseJson.text) ?? new StageGraphJson();

        // override の場所
        overridePath = Path.Combine(Application.persistentDataPath, overrideFileName);

        // 既存 override があればマージ（Upsert相当）
        if (File.Exists(overridePath))
        {
            try
            {
                var txt = File.ReadAllText(overridePath);
                var ov = JsonUtility.FromJson<StageGraphJson>(txt);
                if (ov != null) ApplyOverride(ov);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EditableGraph] Failed to read override: {overridePath}\n{e}");
            }
        }

        // ルックアップ生成
        Ingest(current);
    }

    // ---------- 読み取り ----------

    public bool TryGetNeighbor(string areaId, string stageId, ClearDirection dir, out (string areaId, string stageId) neighbor)
    {
        if (map.TryGetValue((areaId, stageId, dir), out var n))
        {
            neighbor = (n.area, n.stage);
            return true;
        }
        neighbor = default;
        return false;
    }

    // ---------- 編集（ランタイム反映） ----------

    /// <summary>エッジを追加/更新（current と map を即時更新）</summary>
    public void UpsertEdge(string areaId, string stageId, ClearDirection dir, string neighborAreaId, string neighborStageId)
    {
        if (string.IsNullOrWhiteSpace(areaId) || string.IsNullOrWhiteSpace(stageId) ||
            string.IsNullOrWhiteSpace(neighborAreaId) || string.IsNullOrWhiteSpace(neighborStageId))
        {
            Debug.LogWarning("[EditableGraph] UpsertEdge: empty string detected. Skipped.");
            return;
        }

        string dirStr = dir.ToString();
        var found = current.edges.Find(x =>
            x.areaId == areaId && x.stageId == stageId &&
            string.Equals(x.dir, dirStr, StringComparison.OrdinalIgnoreCase));

        if (found == null)
        {
            current.edges.Add(new StageGraphJson.Edge
            {
                areaId = areaId,
                stageId = stageId,
                dir = dirStr,
                neighborAreaId = neighborAreaId,
                neighborStageId = neighborStageId
            });
        }
        else
        {
            found.neighborAreaId = neighborAreaId;
            found.neighborStageId = neighborStageId;
        }

        sessionDirty.Add((areaId, stageId, dir));
        sessionRemoved.Remove((areaId, stageId, dir));

        Ingest(current);
    }

    /// <summary>エッジを削除（current と map を即時更新）</summary>
    public bool RemoveEdge(string areaId, string stageId, ClearDirection dir)
    {
        string dirStr = dir.ToString();
        int removed = current.edges.RemoveAll(x =>
            x.areaId == areaId && x.stageId == stageId &&
            string.Equals(x.dir, dirStr, StringComparison.OrdinalIgnoreCase));

        if (removed > 0)
        {
            sessionRemoved.Add((areaId, stageId, dir));
            sessionDirty.Remove((areaId, stageId, dir));
            Ingest(current);
            return true;
        }
        return false;
    }

    // ---------- 保存系 ----------

    /// <summary>current 全体を override に保存（テスト分も含め全反映）</summary>
    public void SaveOverride()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);
        string json = JsonUtility.ToJson(current, true);
        File.WriteAllText(overridePath, json);
        Debug.Log($"[EditableGraph] Saved FULL graph → {overridePath}");
    }

    /// <summary>
    /// 直近の1件だけを override に保存（「今書いたものだけ」）
    /// - current は Upsert 済み
    /// - override はこの1件のみで上書き（過去に書いた一時データは消える）
    /// </summary>
    public void CommitUpsert(string areaId, string stageId, ClearDirection dir, string neighborAreaId, string neighborStageId)
    {
        UpsertEdge(areaId, stageId, dir, neighborAreaId, neighborStageId);

        var only = new StageGraphJson();
        only.edges.Add(new StageGraphJson.Edge
        {
            areaId = areaId,
            stageId = stageId,
            dir = dir.ToString(),
            neighborAreaId = neighborAreaId,
            neighborStageId = neighborStageId
        });

        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);
        File.WriteAllText(overridePath, JsonUtility.ToJson(only, true));
        Debug.Log($"[EditableGraph] CommitUpsert wrote ONLY this edge → {overridePath}");
    }

    /// <summary>
    /// セッションで触った Upsert 分だけを override に保存（差分保存）
    /// includeRemovals=true の場合、削除も反映したいので FULL 保存にフォールバックします。
    /// </summary>
    public void SaveOverrideDelta(bool includeRemovals = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);

        if (includeRemovals && sessionRemoved.Count > 0)
        {
            // 削除まで確実に反映したい場合は FULL 保存が安全
            File.WriteAllText(overridePath, JsonUtility.ToJson(current, true));
            Debug.Log($"[EditableGraph] Saved FULL (include removals) → {overridePath}");
            return;
        }

        var patch = new StageGraphJson();
        foreach (var k in sessionDirty)
        {
            if (map.TryGetValue((k.area, k.stage, k.dir), out var nb))
            {
                patch.edges.Add(new StageGraphJson.Edge
                {
                    areaId = k.area,
                    stageId = k.stage,
                    dir = k.dir.ToString(),
                    neighborAreaId = nb.area,
                    neighborStageId = nb.stage
                });
            }
        }

        File.WriteAllText(overridePath, JsonUtility.ToJson(patch, true));
        Debug.Log($"[EditableGraph] Saved DELTA (upserts only: {patch.edges.Count}) → {overridePath}");
    }

    /// <summary>override ファイルを削除（次回は base のみで構築）</summary>
    public void ClearOverride()
    {
        if (File.Exists(overridePath))
        {
            File.Delete(overridePath);
            Debug.Log($"[EditableGraph] Deleted override file: {overridePath}");
        }
    }

    /// <summary>current のエッジを全消去（map再構築）。必要なら SaveOverride/SaveOverrideDelta を続けて呼ぶ。</summary>
    public void ClearEdges()
    {
        current.edges.Clear();
        Ingest(current);
    }

    /// <summary>現在の current を JSON テキスト化（エディタ出力に利用）</summary>
    public string ToJson(bool pretty = true) => JsonUtility.ToJson(current, pretty);

    /// <summary>差分キャプチャ開始/終了（SaveOverrideDelta 用）</summary>
    public void BeginCapture() { sessionDirty.Clear(); sessionRemoved.Clear(); }
    public void EndCapture() { sessionDirty.Clear(); sessionRemoved.Clear(); }

    // ---------- 内部ユーティリティ ----------

    private void Ingest(StageGraphJson data)
    {
        map.Clear();
        if (data?.edges == null) return;

        foreach (var e in data.edges)
        {
            if (string.IsNullOrWhiteSpace(e.areaId) ||
                string.IsNullOrWhiteSpace(e.stageId) ||
                string.IsNullOrWhiteSpace(e.dir) ||
                string.IsNullOrWhiteSpace(e.neighborAreaId) ||
                string.IsNullOrWhiteSpace(e.neighborStageId))
                continue;

            if (!Enum.TryParse<ClearDirection>(e.dir, true, out var dir)) continue;

            map[(e.areaId, e.stageId, dir)] = (e.neighborAreaId, e.neighborStageId);
        }
    }

    private void ApplyOverride(StageGraphJson ov)
    {
        if (ov?.edges == null) return;

        foreach (var e in ov.edges)
        {
            if (!Enum.TryParse<ClearDirection>(e.dir, true, out var dir)) continue;

            var exist = current.edges.Find(x =>
                x.areaId == e.areaId && x.stageId == e.stageId &&
                string.Equals(x.dir, e.dir, StringComparison.OrdinalIgnoreCase));

            if (exist == null)
            {
                current.edges.Add(new StageGraphJson.Edge
                {
                    areaId = e.areaId,
                    stageId = e.stageId,
                    dir = dir.ToString(),
                    neighborAreaId = e.neighborAreaId,
                    neighborStageId = e.neighborStageId
                });
            }
            else
            {
                exist.neighborAreaId = e.neighborAreaId;
                exist.neighborStageId = e.neighborStageId;
            }
        }

        Ingest(current);
    }

    /// <summary>
    /// セッションで触った Upsert 分だけを「既存 override に追記マージ」して保存。
    /// 同じ (area,stage,dir) があれば最新で上書き、それ以外は残す。
    /// </summary>
    public void SaveOverrideDeltaAppend()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);

        // 既存 override を読む（なければ空）
        var existing = new StageGraphJson();
        if (File.Exists(overridePath))
        {
            try { existing = JsonUtility.FromJson<StageGraphJson>(File.ReadAllText(overridePath)) ?? new StageGraphJson(); }
            catch { existing = new StageGraphJson(); }
        }

        // 既存 + 今回差分 を辞書にマージ（キー：area,stage,dir）
        var dict = new Dictionary<(string area, string stage, string dir), (string nArea, string nStage)>();

        if (existing.edges != null)
        {
            foreach (var e in existing.edges)
                dict[(e.areaId, e.stageId, e.dir)] = (e.neighborAreaId, e.neighborStageId);
        }

        foreach (var k in sessionDirty)
        {
            if (map.TryGetValue((k.area, k.stage, k.dir), out var nb))
                dict[(k.area, k.stage, k.dir.ToString())] = (nb.area, nb.stage);
        }

        // JSON に戻して保存
        var outJson = new StageGraphJson { edges = new List<StageGraphJson.Edge>() };
        foreach (var kv in dict)
        {
            outJson.edges.Add(new StageGraphJson.Edge
            {
                areaId = kv.Key.area,
                stageId = kv.Key.stage,
                dir = kv.Key.dir,
                neighborAreaId = kv.Value.nArea,
                neighborStageId = kv.Value.nStage
            });
        }

        File.WriteAllText(overridePath, JsonUtility.ToJson(outJson, true));
        Debug.Log($"[EditableGraph] Saved APPEND delta (count={outJson.edges.Count}) → {overridePath}");
    }

    /// <summary>
    /// 1件だけ追記マージして即保存（その1件で既存とマージ、既存は残す）
    /// </summary>
    public void CommitUpsertAppend(string areaId, string stageId, ClearDirection dir, string neighborAreaId, string neighborStageId)
    {
        // メモリ更新
        UpsertEdge(areaId, stageId, dir, neighborAreaId, neighborStageId);

        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);

        // 既存読み込み
        var existing = new StageGraphJson();
        if (File.Exists(overridePath))
        {
            try { existing = JsonUtility.FromJson<StageGraphJson>(File.ReadAllText(overridePath)) ?? new StageGraphJson(); }
            catch { existing = new StageGraphJson(); }
        }

        // 既存を辞書化
        var dict = new Dictionary<(string area, string stage, string dir), (string nArea, string nStage)>();
        if (existing.edges != null)
        {
            foreach (var e in existing.edges)
                dict[(e.areaId, e.stageId, e.dir)] = (e.neighborAreaId, e.neighborStageId);
        }

        // 今回1件を上書き
        dict[(areaId, stageId, dir.ToString())] = (neighborAreaId, neighborStageId);

        // JSON に戻して保存
        var outJson = new StageGraphJson { edges = new List<StageGraphJson.Edge>() };
        foreach (var kv in dict)
        {
            outJson.edges.Add(new StageGraphJson.Edge
            {
                areaId = kv.Key.area,
                stageId = kv.Key.stage,
                dir = kv.Key.dir,
                neighborAreaId = kv.Value.nArea,
                neighborStageId = kv.Value.nStage
            });
        }

        File.WriteAllText(overridePath, JsonUtility.ToJson(outJson, true));
        Debug.Log($"[EditableGraph] CommitUpsertAppend saved (total={outJson.edges.Count}) → {overridePath}");
    }
}


