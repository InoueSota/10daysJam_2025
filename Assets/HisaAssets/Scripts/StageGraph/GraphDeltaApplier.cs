using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// いま入力したエッジだけを override.json に保存するためのユーティリティ。
/// - BeginCapture → UpsertEdge(...×n) → SaveOverrideDelta → EndCapture を1ボタンで実行
/// - GameBootstrap.Graph (EditableJsonStageGraph) を使います。
/// - Resources の base JSON は変更しません。保存先は persistentDataPath の override.json。
/// </summary>
public class GraphDeltaApplier : MonoBehaviour
{
    [Serializable]
    public struct EdgeInput
    {
        public string areaId;
        public string stageId;
        public ClearDirection dir;     // Up/Down/Left/Right
        public string neighborAreaId;
        public string neighborStageId;
    }

    [Header("この一覧に入力したエッジ“だけ”を保存します")]
    public List<EdgeInput> edges = new();

    [Tooltip("実行前に既存の override.json を削除します（完全に今入力した分だけにしたいときON）")]
    public bool clearOverrideFirst = false;

    [Tooltip("ログを詳しく出します")]
    public bool verbose = true;

    // ---- 実行API ----

    /// <summary>
    /// 入力中のエッジだけを差分保存（過去の一時データは消えます）
    /// </summary>
    [ContextMenu("Apply & Save Delta (Upserts only)")]
    public void ApplyAndSaveDelta()
    {
        if (!TryGetEditableGraph(out var g)) return;

        if (clearOverrideFirst)
        {
            g.ClearOverride();
            Log("[Delta] Cleared existing override file.");
        }

        g.BeginCapture();

        int ok = 0, skip = 0;
        foreach (var e in edges)
        {
            if (IsInvalid(e))
            {
                skip++;
                LogWarning($"[Delta] skipped (empty field): {Dump(e)}");
                continue;
            }

            g.UpsertEdge(e.areaId, e.stageId, e.dir, e.neighborAreaId, e.neighborStageId);
            ok++;
            Log($"[Delta] upsert: {Dump(e)}");
        }

        g.SaveOverrideDelta(); // ← このセッションで触った Upsert 分“だけ”を書き出し
        g.EndCapture();

        Log($"[Delta] saved. upserts={ok}, skipped={skip}");
        ShowSavedPathHint();
    }

    /// <summary>
    /// 1件だけ即コミット保存（入力の先頭要素を使います）
    /// </summary>
    [ContextMenu("Commit 1st Edge (save only that)")]
    public void CommitFirstEdge()
    {
        if (!TryGetEditableGraph(out var g)) return;
        if (edges.Count == 0) { LogError("edges が空です"); return; }

        var e = edges[0];
        if (IsInvalid(e)) { LogError("先頭のエッジに未入力があります"); return; }

        g.CommitUpsert(e.areaId, e.stageId, e.dir, e.neighborAreaId, e.neighborStageId);
        Log($"[Commit] wrote only: {Dump(e)}");
        ShowSavedPathHint();
    }

    // ---- 内部ヘルパ ----

    private static bool IsInvalid(EdgeInput e)
        => string.IsNullOrWhiteSpace(e.areaId) ||
           string.IsNullOrWhiteSpace(e.stageId) ||
           string.IsNullOrWhiteSpace(e.neighborAreaId) ||
           string.IsNullOrWhiteSpace(e.neighborStageId);

    private static string Dump(EdgeInput e)
        => $"{e.areaId}/{e.stageId} --{e.dir}--> {e.neighborAreaId}/{e.neighborStageId}";

    private bool TryGetEditableGraph(out EditableJsonStageGraph g)
    {
        g = GameBootstrap.Graph as EditableJsonStageGraph;
        if (g == null)
        {
            LogError("GameBootstrap.Graph が EditableJsonStageGraph ではありません（または未初期化）。" +
                     "編集保存したい場合は GameBootstrap で EditableJsonStageGraph を作成してください。");
            return false;
        }
        return true;
    }

    private void ShowSavedPathHint()
    {
        Log($"Saved to: {Application.persistentDataPath}/stage_graph_override.json");
    }

    private void Log(string msg) { if (verbose) Debug.Log(msg); }
    private void LogWarning(string msg) { if (verbose) Debug.LogWarning(msg); }
    private void LogError(string msg) { Debug.LogError(msg); }
}
