using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ܓ��͂����G�b�W������ override.json �ɕۑ����邽�߂̃��[�e�B���e�B�B
/// - BeginCapture �� UpsertEdge(...�~n) �� SaveOverrideDelta �� EndCapture ��1�{�^���Ŏ��s
/// - GameBootstrap.Graph (EditableJsonStageGraph) ���g���܂��B
/// - Resources �� base JSON �͕ύX���܂���B�ۑ���� persistentDataPath �� override.json�B
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

    [Header("���̈ꗗ�ɓ��͂����G�b�W�g�����h��ۑ����܂�")]
    public List<EdgeInput> edges = new();

    [Tooltip("���s�O�Ɋ����� override.json ���폜���܂��i���S�ɍ����͂����������ɂ������Ƃ�ON�j")]
    public bool clearOverrideFirst = false;

    [Tooltip("���O���ڂ����o���܂�")]
    public bool verbose = true;

    // ---- ���sAPI ----

    /// <summary>
    /// ���͒��̃G�b�W�����������ۑ��i�ߋ��̈ꎞ�f�[�^�͏����܂��j
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

        g.SaveOverrideDelta(); // �� ���̃Z�b�V�����ŐG���� Upsert ���g�����h�������o��
        g.EndCapture();

        Log($"[Delta] saved. upserts={ok}, skipped={skip}");
        ShowSavedPathHint();
    }

    /// <summary>
    /// 1���������R�~�b�g�ۑ��i���͂̐擪�v�f���g���܂��j
    /// </summary>
    [ContextMenu("Commit 1st Edge (save only that)")]
    public void CommitFirstEdge()
    {
        if (!TryGetEditableGraph(out var g)) return;
        if (edges.Count == 0) { LogError("edges ����ł�"); return; }

        var e = edges[0];
        if (IsInvalid(e)) { LogError("�擪�̃G�b�W�ɖ����͂�����܂�"); return; }

        g.CommitUpsert(e.areaId, e.stageId, e.dir, e.neighborAreaId, e.neighborStageId);
        Log($"[Commit] wrote only: {Dump(e)}");
        ShowSavedPathHint();
    }

    // ---- �����w���p ----

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
            LogError("GameBootstrap.Graph �� EditableJsonStageGraph �ł͂���܂���i�܂��͖��������j�B" +
                     "�ҏW�ۑ��������ꍇ�� GameBootstrap �� EditableJsonStageGraph ���쐬���Ă��������B");
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
