using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    private Vector3 diffValue = new Vector3(11.0f, 2.5f, 0f);       // ずらし量
    public float cellSize = 1f;     // マスの大きさ
    public int gridWidth = 20;      // 横マス数
    public int gridHeight = 12;     // 縦マス数
    public Material lineMaterial;   // シンプルなUnlit/Colorマテリアル推奨
    public Color lineColor = Color.gray;
    public float lineWidth;

    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        // 縦線
        for (int x = 0; x <= gridWidth; x++)
        {
            CreateLine(new Vector3(x * cellSize - diffValue.x, -diffValue.y, 0),
                       new Vector3(x * cellSize - diffValue.x, gridHeight * cellSize - diffValue.y, 0));
        }

        // 横線
        for (int y = 0; y <= gridHeight; y++)
        {
            CreateLine(new Vector3(-diffValue.x, y * cellSize - diffValue.y, 0),
                       new Vector3(gridWidth * cellSize - diffValue.x, y * cellSize - diffValue.y, 0));
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = this.transform;

        var lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.material = lineMaterial;
        lr.startColor = lr.endColor = lineColor;
        lr.startWidth = lr.endWidth = lineWidth; // 線の太さ
        lr.sortingLayerName = "Overlay"; // プレイヤーやブロックより奥に描く
    }
}
