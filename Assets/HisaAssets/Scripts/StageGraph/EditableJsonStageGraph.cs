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
/// JSON�x�[�X�̕ҏW�\�O���t�B
/// - base: Resources �� stage_graph.json�i�ǂݎ���p�j
/// - override: persistentDataPath/stage_graph_override.json�i�������݉j
///
/// ����:
/// - Upsert/Remove �� current ���X�V���A�����Ƀ����^�C���֔��f
/// - SaveOverride(): current �S�̂� override �ɕۑ�
/// - CommitUpsert(): ���O��1�������� override �ɕۑ��i�u���������������v�j
/// - BeginCapture/SaveOverrideDelta(): �Z�b�V�������ɐG�����������������ۑ�
/// - ClearOverride(): override ���폜�ibase �݂̂ցj
/// - ClearEdges(): current ����Ɂimap�č\�z�j
/// </summary>
public class EditableJsonStageGraph : IStageGraph
{
    // ���b�N�A�b�v�p�}�b�v�iarea, stage, dir -> neighbor(area, stage)�j
    private readonly Dictionary<(string area, string stage, ClearDirection dir), (string area, string stage)> map
        = new();

    // ���݂̎��́ibase + override �𔽉f�������́j
    private StageGraphJson current = new StageGraphJson();

    // �ǋL�ۑ��t�@�C���i�������ݐ�j
    private readonly string overridePath;

    // �Z�b�V���������g���b�L���O
    private readonly HashSet<(string area, string stage, ClearDirection dir)> sessionDirty
        = new();
    private readonly HashSet<(string area, string stage, ClearDirection dir)> sessionRemoved
        = new();

    /// <param name="baseJson">Resources ���烍�[�h���� TextAsset (��: Resources.Load&lt;TextAsset&gt;("stage_graph"))</param>
    /// <param name="overrideFileName">persistentDataPath �ɍ��㏑���t�@�C�����B����: stage_graph_override.json</param>
    public EditableJsonStageGraph(TextAsset baseJson, string overrideFileName = "stage_graph_override.json")
    {
        if (baseJson == null) throw new ArgumentNullException(nameof(baseJson), "baseJson is null. Place Assets/Resources/stage_graph.json.");

        // base ���f
        current = JsonUtility.FromJson<StageGraphJson>(baseJson.text) ?? new StageGraphJson();

        // override �̏ꏊ
        overridePath = Path.Combine(Application.persistentDataPath, overrideFileName);

        // ���� override ������΃}�[�W�iUpsert�����j
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

        // ���b�N�A�b�v����
        Ingest(current);
    }

    // ---------- �ǂݎ�� ----------

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

    // ---------- �ҏW�i�����^�C�����f�j ----------

    /// <summary>�G�b�W��ǉ�/�X�V�icurrent �� map �𑦎��X�V�j</summary>
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

    /// <summary>�G�b�W���폜�icurrent �� map �𑦎��X�V�j</summary>
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

    // ---------- �ۑ��n ----------

    /// <summary>current �S�̂� override �ɕۑ��i�e�X�g�����܂ߑS���f�j</summary>
    public void SaveOverride()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);
        string json = JsonUtility.ToJson(current, true);
        File.WriteAllText(overridePath, json);
        Debug.Log($"[EditableGraph] Saved FULL graph �� {overridePath}");
    }

    /// <summary>
    /// ���߂�1�������� override �ɕۑ��i�u�����������̂����v�j
    /// - current �� Upsert �ς�
    /// - override �͂���1���݂̂ŏ㏑���i�ߋ��ɏ������ꎞ�f�[�^�͏�����j
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
        Debug.Log($"[EditableGraph] CommitUpsert wrote ONLY this edge �� {overridePath}");
    }

    /// <summary>
    /// �Z�b�V�����ŐG���� Upsert �������� override �ɕۑ��i�����ۑ��j
    /// includeRemovals=true �̏ꍇ�A�폜�����f�������̂� FULL �ۑ��Ƀt�H�[���o�b�N���܂��B
    /// </summary>
    public void SaveOverrideDelta(bool includeRemovals = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);

        if (includeRemovals && sessionRemoved.Count > 0)
        {
            // �폜�܂Ŋm���ɔ��f�������ꍇ�� FULL �ۑ������S
            File.WriteAllText(overridePath, JsonUtility.ToJson(current, true));
            Debug.Log($"[EditableGraph] Saved FULL (include removals) �� {overridePath}");
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
        Debug.Log($"[EditableGraph] Saved DELTA (upserts only: {patch.edges.Count}) �� {overridePath}");
    }

    /// <summary>override �t�@�C�����폜�i����� base �݂̂ō\�z�j</summary>
    public void ClearOverride()
    {
        if (File.Exists(overridePath))
        {
            File.Delete(overridePath);
            Debug.Log($"[EditableGraph] Deleted override file: {overridePath}");
        }
    }

    /// <summary>current �̃G�b�W��S�����imap�č\�z�j�B�K�v�Ȃ� SaveOverride/SaveOverrideDelta �𑱂��ČĂԁB</summary>
    public void ClearEdges()
    {
        current.edges.Clear();
        Ingest(current);
    }

    /// <summary>���݂� current �� JSON �e�L�X�g���i�G�f�B�^�o�͂ɗ��p�j</summary>
    public string ToJson(bool pretty = true) => JsonUtility.ToJson(current, pretty);

    /// <summary>�����L���v�`���J�n/�I���iSaveOverrideDelta �p�j</summary>
    public void BeginCapture() { sessionDirty.Clear(); sessionRemoved.Clear(); }
    public void EndCapture() { sessionDirty.Clear(); sessionRemoved.Clear(); }

    // ---------- �������[�e�B���e�B ----------

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
    /// �Z�b�V�����ŐG���� Upsert ���������u���� override �ɒǋL�}�[�W�v���ĕۑ��B
    /// ���� (area,stage,dir) ������΍ŐV�ŏ㏑���A����ȊO�͎c���B
    /// </summary>
    public void SaveOverrideDeltaAppend()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);

        // ���� override ��ǂށi�Ȃ���΋�j
        var existing = new StageGraphJson();
        if (File.Exists(overridePath))
        {
            try { existing = JsonUtility.FromJson<StageGraphJson>(File.ReadAllText(overridePath)) ?? new StageGraphJson(); }
            catch { existing = new StageGraphJson(); }
        }

        // ���� + ���񍷕� �������Ƀ}�[�W�i�L�[�Farea,stage,dir�j
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

        // JSON �ɖ߂��ĕۑ�
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
        Debug.Log($"[EditableGraph] Saved APPEND delta (count={outJson.edges.Count}) �� {overridePath}");
    }

    /// <summary>
    /// 1�������ǋL�}�[�W���đ��ۑ��i����1���Ŋ����ƃ}�[�W�A�����͎c���j
    /// </summary>
    public void CommitUpsertAppend(string areaId, string stageId, ClearDirection dir, string neighborAreaId, string neighborStageId)
    {
        // �������X�V
        UpsertEdge(areaId, stageId, dir, neighborAreaId, neighborStageId);

        Directory.CreateDirectory(Path.GetDirectoryName(overridePath)!);

        // �����ǂݍ���
        var existing = new StageGraphJson();
        if (File.Exists(overridePath))
        {
            try { existing = JsonUtility.FromJson<StageGraphJson>(File.ReadAllText(overridePath)) ?? new StageGraphJson(); }
            catch { existing = new StageGraphJson(); }
        }

        // ������������
        var dict = new Dictionary<(string area, string stage, string dir), (string nArea, string nStage)>();
        if (existing.edges != null)
        {
            foreach (var e in existing.edges)
                dict[(e.areaId, e.stageId, e.dir)] = (e.neighborAreaId, e.neighborStageId);
        }

        // ����1�����㏑��
        dict[(areaId, stageId, dir.ToString())] = (neighborAreaId, neighborStageId);

        // JSON �ɖ߂��ĕۑ�
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
        Debug.Log($"[EditableGraph] CommitUpsertAppend saved (total={outJson.edges.Count}) �� {overridePath}");
    }
}


