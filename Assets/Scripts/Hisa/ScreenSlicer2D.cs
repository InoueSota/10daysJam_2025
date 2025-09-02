using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// カメラの映像を一度だけスナップショットしてテクスチャ化し、メッシュに貼り付ける。
/// 左クリック: 縦切り（x=const） / 右クリック: 横切り（y=const）
/// 切断後は ←→（縦切り時）/ ↑↓（横切り時）で残す側を選択。選ばれなかった側はヒラヒラ落下。
/// 何度でも分割可能。URPで裏面も描画（Cull Off）。
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenSlicer2D : MonoBehaviour
{
    [Header("Targets")]
    public Camera targetCamera;

    [Header("Options")]
    public bool captureOnStart = true;
    public float planeZ = 0f;              // 表示するメッシュのZ平面（2Dなら0でOK）
    public float pixelsPerUnit = 100f;     // テクスチャサイズ -> ワールドサイズ換算
    public float dropDuration = 2.5f;      // 落下パーツの寿命（秒）
    public float dropGravity = 5.0f;       // 落下の重力加速度（見た目用）
    public float flutterAngle = 25f;       // ヒラヒラ回転の最大角度
    public float flutterFreq = 3.0f;       // ヒラヒラ回転の周波数
    public LineRenderer guideLine;         // ガイド表示（任意）

    [Header("Material (optional)")]
    public Material overrideMaterial;      // 指定なければURP Unlit生成

    // 内部状態
    Texture2D snapshotTex;
    Material runtimeMat;
    MeshFilter mf;
    MeshRenderer mr;

    // 現在生きている「残る側」ポリゴン（ローカル座標）
    List<Vector2> currentPoly = new List<Vector2>();
    Rect originalRect; // UV計算の基準
    bool awaitingChoice = false;
    bool lastCutVertical = true; // true=縦切り、false=横切り
    float lastCutCoord = 0f;     // x or y の値（ローカル座標）

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        if (guideLine == null)
        {
            var go = new GameObject("GuideLine");
            go.transform.SetParent(transform, false);
            guideLine = go.AddComponent<LineRenderer>();
            guideLine.positionCount = 2;
            guideLine.widthMultiplier = 0.02f;
            guideLine.enabled = false;
            // URPのラインマテリアル（Sprites/DefaultでもOK）
            guideLine.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void Start()
    {
        if (captureOnStart) CaptureAndBuild();
    }

    public void CaptureAndBuild()
    {
        if (targetCamera == null)
        {
            Debug.LogError("[ScreenSlicer2D] Target Camera が未設定です。");
            return;
        }

        // --- カメラをRenderTextureに描画してから Texture2D へコピー ---
        var pr = targetCamera.pixelRect;
        int texW = Mathf.Max(1, targetCamera.pixelWidth);
        int texH = Mathf.Max(1, targetCamera.pixelHeight);

        RenderTexture rt = new RenderTexture(texW, texH, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 1; // 必要なら変更
        targetCamera.targetTexture = rt;
        targetCamera.Render();

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        snapshotTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        var rect = new Rect(0, 0, texW, texH);
        snapshotTex.ReadPixels(rect, 0, 0, false);
        snapshotTex.wrapMode = TextureWrapMode.Clamp;
        snapshotTex.filterMode = FilterMode.Bilinear; // PixelPerfectなら Point 推奨

        snapshotTex.Apply(false);

        RenderTexture.active = prev;
        targetCamera.targetTexture = null;
        rt.Release();

        // --- マテリアル作成（URP Unlit / Cull Off） ---
        if (overrideMaterial != null)
        {
            runtimeMat = new Material(overrideMaterial);
        }
        else
        {
            Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (urpUnlit == null)
            {
                // フォールバック
                urpUnlit = Shader.Find("Sprites/Default");
            }
            runtimeMat = new Material(urpUnlit);
        }

        // マテリアルにテクスチャをセットした直後（CaptureAndBuild内）
        if (runtimeMat.HasProperty("_BaseMap"))
        {
            runtimeMat.SetTexture("_BaseMap", snapshotTex);
            runtimeMat.SetTextureScale("_BaseMap", Vector2.one);
            runtimeMat.SetTextureOffset("_BaseMap", Vector2.zero);
        }
        if (runtimeMat.HasProperty("_MainTex"))
        {
            runtimeMat.SetTexture("_MainTex", snapshotTex);
            runtimeMat.SetTextureScale("_MainTex", Vector2.one);
            runtimeMat.SetTextureOffset("_MainTex", Vector2.zero);
        }

        // 両面描画（Cull Off）
        if (runtimeMat.HasProperty("_Cull"))
        {
            runtimeMat.SetInt("_Cull", (int)CullMode.Off);
        }
        mr.sharedMaterial = runtimeMat;

        // --- 最初のポリゴン（長方形）を作成 ---
        // カメラの可視範囲と一致するワールド幅/高さを算出
        float worldW, worldH;

        if (targetCamera.orthographic)
        {
            worldH = 2f * targetCamera.orthographicSize;
            worldW = worldH * targetCamera.aspect;
        }
        else
        {
            // planeZ の “ワールドZ” 平面に合わせたフラスタム幅/高さ
            // このオブジェクトの transform.position.z を基準に計算
            float dist = Mathf.Abs(planeZ - targetCamera.transform.position.z);
            float worldHAtZ = 2f * dist * Mathf.Tan(0.5f * targetCamera.fieldOfView * Mathf.Deg2Rad);
            worldH = worldHAtZ;
            worldW = worldH * targetCamera.aspect;
        }

        // 中心を (0,0) に配置した矩形を作成（これが見た目のアスペクトの正解）
        currentPoly.Clear();
        currentPoly.Add(new Vector2(-worldW * 0.5f, -worldH * 0.5f));
        currentPoly.Add(new Vector2(-worldW * 0.5f, worldH * 0.5f));
        currentPoly.Add(new Vector2(worldW * 0.5f, worldH * 0.5f));
        currentPoly.Add(new Vector2(worldW * 0.5f, -worldH * 0.5f));

        // UV の基準（0..1 に正規化するための矩形）
        originalRect = new Rect(-worldW * 0.5f, -worldH * 0.5f, worldW, worldH);

        // 仕上げ
        RebuildMesh(currentPoly);
    }

    void Update()
    {
        if (snapshotTex == null || awaitingChoice) // 選択待ちは入力を限定
        {
            if (awaitingChoice) HandleChoiceInput();
            return;
        }

        // 切断の入力
        if (Input.GetMouseButtonDown(0)) // 縦切り x = const
        {
            Vector3 wp = MouseOnPlane();
            BeginCut(vertical: true, coord: transform.InverseTransformPoint(wp).x);
        }
        else if (Input.GetMouseButtonDown(1)) // 横切り y = const
        {
            Vector3 wp = MouseOnPlane();
            BeginCut(vertical: false, coord: transform.InverseTransformPoint(wp).y);
        }
    }

    Vector3 MouseOnPlane()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float t = 0f;
        // z=planeZ の平面にヒット
        if (Mathf.Abs(ray.direction.z) < 1e-6f) return ray.origin + ray.direction * 10f;
        t = (planeZ - ray.origin.z) / ray.direction.z;
        return ray.origin + ray.direction * t;
    }

    // 切断開始（ポリゴンから左右/上下の2ポリゴンに分割）
    void BeginCut(bool vertical, float coord)
    {
        lastCutVertical = vertical;
        lastCutCoord = coord;

        // 2つにクリップ
        List<Vector2> a = ClipHalf(currentPoly, vertical, coord, keepPositiveSide: true);
        List<Vector2> b = ClipHalf(currentPoly, vertical, coord, keepPositiveSide: false);

        // 両方に有効頂点があるか
        if (a.Count < 3 || b.Count < 3)
        {
            // 分割できない（端すぎる等）
            return;
        }

        // 可視化ライン
        ShowGuideLine(vertical, coord);

        // メッシュ2つ生成（同一テクスチャ＆マテリアル）
        var goA = CreatePieceGO("Piece_KeepCandidate_A", a);
        var goB = CreatePieceGO("Piece_KeepCandidate_B", b);

        // 一時的に両方表示、選択待ちへ
        awaitingChoice = true;
        // 一時停止中は currentPiece は未確定なので非表示に
        mf.sharedMesh = null;

        // 内部に保持（選択用）
        pendingA = goA;
        pendingB = goB;
        polyA = a;
        polyB = b;
    }

    void ShowGuideLine(bool vertical, float coord)
    {
        guideLine.enabled = true;
        Vector3 p0, p1;
        float extend = 100f;
        if (vertical)
        {
            p0 = transform.TransformPoint(new Vector3(coord, -extend, planeZ));
            p1 = transform.TransformPoint(new Vector3(coord, extend, planeZ));
        }
        else
        {
            p0 = transform.TransformPoint(new Vector3(-extend, coord, planeZ));
            p1 = transform.TransformPoint(new Vector3(extend, coord, planeZ));
        }
        guideLine.SetPosition(0, p0);
        guideLine.SetPosition(1, p1);
    }

    // クリップ（Sutherland–Hodgman）: verticalなら x>=coord 側（正）/ x<=coord 側（負）で半平面クリップ
    List<Vector2> ClipHalf(List<Vector2> poly, bool vertical, float coord, bool keepPositiveSide)
    {
        var outList = new List<Vector2>(poly);
        var inList = new List<Vector2>();

        // 境界： vertical -> x=coord（法線±X） / horizontal -> y=coord（法線±Y）
        for (int i = 0; i < outList.Count; i++)
        {
            inList.Add(outList[i]);
        }
        outList.Clear();

        for (int i = 0; i < inList.Count; i++)
        {
            Vector2 cur = inList[i];
            Vector2 prev = inList[(i - 1 + inList.Count) % inList.Count];

            bool curInside = Inside(cur, vertical, coord, keepPositiveSide);
            bool prevInside = Inside(prev, vertical, coord, keepPositiveSide);

            if (prevInside && curInside)
            {
                // そのまま追加
                outList.Add(cur);
            }
            else if (prevInside && !curInside)
            {
                // 出るとき：交点を追加
                outList.Add(Intersect(prev, cur, vertical, coord));
            }
            else if (!prevInside && curInside)
            {
                // 入るとき：交点→頂点
                outList.Add(Intersect(prev, cur, vertical, coord));
                outList.Add(cur);
            }
            // 両方外：何もしない
        }

        // 重複＆極小辺の整理
        RemoveNearDuplicates(outList, 1e-6f);
        return outList;
    }

    bool Inside(Vector2 p, bool vertical, float coord, bool keepPositiveSide)
    {
        if (vertical)
            return keepPositiveSide ? (p.x >= coord - 1e-7f) : (p.x <= coord + 1e-7f);
        else
            return keepPositiveSide ? (p.y >= coord - 1e-7f) : (p.y <= coord + 1e-7f);
    }

    Vector2 Intersect(Vector2 p1, Vector2 p2, bool vertical, float coord)
    {
        // 線分p1->p2 と x=coord or y=coord の交点
        Vector2 d = p2 - p1;
        if (vertical)
        {
            float t = Mathf.Approximately(d.x, 0f) ? 0f : (coord - p1.x) / d.x;
            return p1 + d * t;
        }
        else
        {
            float t = Mathf.Approximately(d.y, 0f) ? 0f : (coord - p1.y) / d.y;
            return p1 + d * t;
        }
    }

    void RemoveNearDuplicates(List<Vector2> list, float eps)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            int ni = (i + 1) % list.Count;
            if ((list[ni] - list[i]).sqrMagnitude < eps * eps)
            {
                list.RemoveAt(i);
            }
        }
    }

    // 2Dポリゴン -> Mesh（Ear Clippingで三角化）
    void RebuildMesh(List<Vector2> poly)
    {
        if (poly.Count < 3) return;

        var mesh = new Mesh();
        var v3 = new List<Vector3>(poly.Count);
        var uv = new List<Vector2>(poly.Count);

        for (int i = 0; i < poly.Count; i++)
        {
            var p = poly[i];
            v3.Add(new Vector3(p.x, p.y, planeZ));

            // UV（スナップショット基準で正規化）
            float u = (p.x - originalRect.xMin) / originalRect.width;
            float v = (p.y - originalRect.yMin) / originalRect.height;
            uv.Add(new Vector2(u, v));
        }

        var tris = TriangulateEarClipping(poly);

        mesh.SetVertices(v3);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(tris, 0, true);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
        mr.sharedMaterial = runtimeMat;
    }

    List<int> TriangulateEarClipping(List<Vector2> poly)
    {
        // 通常のEar Clipping。凸・凹OK（自己交差は不可）。
        var indices = new List<int>();
        int n = poly.Count;
        if (n < 3) return indices;

        // CCW判定。CWなら反転してCCWに。
        if (SignedArea(poly) < 0) poly = new List<Vector2>(poly); // コピー
        if (SignedArea(poly) < 0) { } // 保険
        if (SignedArea(poly) < 0) // 3回呼んでも意味は同じだが、エッジケース保険
        {
            poly.Reverse();
        }
        else
        {
            // もしCWなら一発で反転
            if (SignedArea(poly) < 0f)
                poly.Reverse();
        }

        var V = new List<int>();
        for (int i = 0; i < n; i++) V.Add(i);

        int guard = 0;
        while (V.Count > 3 && guard < 10000)
        {
            guard++;
            bool earFound = false;
            for (int i = 0; i < V.Count; i++)
            {
                int i0 = V[(i - 1 + V.Count) % V.Count];
                int i1 = V[i];
                int i2 = V[(i + 1) % V.Count];

                Vector2 a = poly[i0];
                Vector2 b = poly[i1];
                Vector2 c = poly[i2];

                if (IsConvex(a, b, c))
                {
                    bool hasPointInside = false;
                    for (int j = 0; j < V.Count; j++)
                    {
                        if (j == (i - 1 + V.Count) % V.Count ||
                            j == i ||
                            j == (i + 1) % V.Count) continue;

                        Vector2 p = poly[V[j]];
                        if (PointInTriangle(p, a, b, c))
                        {
                            hasPointInside = true;
                            break;
                        }
                    }
                    if (!hasPointInside)
                    {
                        // 耳
                        indices.Add(i0);
                        indices.Add(i1);
                        indices.Add(i2);
                        V.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }
            }
            if (!earFound)
            {
                // 失敗時は単純に扇形分割でフォールバック（凸ならOK）
                indices.Clear();
                for (int i = 1; i < poly.Count - 1; i++)
                {
                    indices.Add(0);
                    indices.Add(i);
                    indices.Add(i + 1);
                }
                return indices;
            }
        }

        if (V.Count == 3)
        {
            indices.Add(V[0]);
            indices.Add(V[1]);
            indices.Add(V[2]);
        }
        return indices;
    }

    float SignedArea(List<Vector2> poly)
    {
        double a = 0;
        for (int i = 0; i < poly.Count; i++)
        {
            int j = (i + 1) % poly.Count;
            a += (double)poly[i].x * poly[j].y - (double)poly[j].x * poly[i].y;
        }
        return (float)(a * 0.5);
    }

    bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return Cross(b - a, c - b) > 0f; // CCW 前提
    }

    float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float c1 = Cross(b - a, p - a);
        float c2 = Cross(c - b, p - b);
        float c3 = Cross(a - c, p - c);
        bool hasNeg = (c1 < 0) || (c2 < 0) || (c3 < 0);
        bool hasPos = (c1 > 0) || (c2 > 0) || (c3 > 0);
        return !(hasNeg && hasPos);
    }

    GameObject CreatePieceGO(string name, List<Vector2> poly)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform.parent, worldPositionStays: false);
        go.transform.position = transform.position;
        go.transform.rotation = transform.rotation;
        go.transform.localScale = transform.localScale;

        var _mf = go.AddComponent<MeshFilter>();
        var _mr = go.AddComponent<MeshRenderer>();
        _mr.sharedMaterial = runtimeMat;

        // メッシュ構築
        var mesh = new Mesh();
        var v3 = new List<Vector3>(poly.Count);
        var uv = new List<Vector2>(poly.Count);

        for (int i = 0; i < poly.Count; i++)
        {
            var p = poly[i];
            v3.Add(new Vector3(p.x, p.y, planeZ));
            float u = (p.x - originalRect.xMin) / originalRect.width;
            float v = (p.y - originalRect.yMin) / originalRect.height;
            uv.Add(new Vector2(u, v));
        }
        var tris = TriangulateEarClipping(poly);
        mesh.SetVertices(v3);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(tris, 0, true);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _mf.sharedMesh = mesh;

        return go;
    }

    // ---- 切断後の選択状態 ----
    GameObject pendingA, pendingB;
    List<Vector2> polyA, polyB;

    void HandleChoiceInput()
    {
        // lastCutVertical = true (縦) -> ← で左、 →で右 を残す
        // lastCutVertical = false(横) -> ↑ で上、  ↓で下 を残す
        bool keepA = false, keepB = false;

        if (lastCutVertical)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { keepA = true; keepB = false; }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { keepA = false; keepB = true; }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) { keepA = true; keepB = false; }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { keepA = false; keepB = true; }
        }

        if (!(keepA || keepB)) return;

        // 残す側選定
        GameObject keepGO = keepA ? pendingA : pendingB;
        List<Vector2> keepPoly = keepA ? polyA : polyB;
        GameObject dropGO = keepA ? pendingB : pendingA;

        // 落とす側にDropPieceを付与してヒラヒラ落下
        var dp = dropGO.AddComponent<DropPiece>();
        dp.duration = dropDuration;
        dp.gravity = dropGravity;
        dp.flutterAngle = flutterAngle;
        dp.flutterFreq = flutterFreq;

        // current に反映
        currentPoly = keepPoly;
        // 自身（元のホスト）に戻す：現在は pending を表示しているので、keepGO のメッシュをこのオブジェクトに移し替え
        var srcMF = keepGO.GetComponent<MeshFilter>();
        var srcMesh = srcMF.sharedMesh;
        mf.sharedMesh = srcMesh;
        mr.sharedMaterial = runtimeMat;

        // pendingオブジェクトは不要（keep側は即時破棄、drop側は寿命で消える）
        Destroy(keepGO);
        // 自分のTransformは最初からここにあるのでOK

        // 状態解除
        awaitingChoice = false;
        pendingA = pendingB = null;
        polyA = polyB = null;

        guideLine.enabled = false;
    }
}

/// <summary>
/// 切り落とされたピースをヒラヒラ落下させ、一定時間後に破棄するコンポーネント。
/// Rigidbody不要の見た目アニメ。2D用途を想定。
/// </summary>
public class DropPiece : MonoBehaviour
{
    public float duration = 2.5f;
    public float gravity = 5f;
    public float flutterAngle = 25f;
    public float flutterFreq = 3f;

    float elapsed;
    float vy = 0f;
    float baseAngle;

    void Start()
    {
        baseAngle = Random.Range(-10f, 10f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        vy -= gravity * Time.deltaTime;

        // 位置更新（下向き）
        transform.position += new Vector3(0f, vy * Time.deltaTime, 0f);

        // ヒラヒラ（サイン回転）
        float ang = baseAngle + Mathf.Sin(elapsed * flutterFreq * Mathf.PI * 2f) * flutterAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
