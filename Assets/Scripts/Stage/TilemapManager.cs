using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private TilemapRenderer page1Renderer;
    [SerializeField] private TilemapRenderer page2Renderer;
    [SerializeField] private TilemapRenderer page3Renderer;

    [SerializeField] private Tilemap tilemap_;

    [Header("対応するタイルを登録してください")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase blockTile;

    [SerializeField] Sprite[] blocks;
    int maxX = 0;
    int maxY = 0;

    private int[,] grid;

    public enum TileType
    {
        empty = 0,
        ground = 1,
        block = 2
    }

    // タイルマップを走査して int[,] に変換
    public void SaveTilemapToArrayAutoBounds()
    {
        BoundsInt bounds = tilemap_.cellBounds;
        Vector3Int bottomLeft = new Vector3Int(bounds.xMin, bounds.yMin, 0);
        Vector3Int topRight = new Vector3Int(bounds.xMax - 1, bounds.yMax - 1, 0);
        maxX = bounds.size.x - 1;
        maxY = bounds.size.y - 1;

        int width = bounds.size.x;
        int height = bounds.size.y;

        grid = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int pos = new Vector3Int(bottomLeft.x + x, bottomLeft.y + y, 0);
                TileBase tile = tilemap_.GetTile(pos);

                if (tile == null)
                {
                    grid[x, y] = (int)TileType.empty;
                }
                else if (tile == groundTile)
                {
                    grid[x, y] = (int)TileType.ground;
                }
                else if (tile == blockTile)
                {
                    grid[x, y] = (int)TileType.block;
                }
                else
                {
                    grid[x, y] = (int)TileType.empty;
                }
            }
        }
    }


    // 配列から種類を取得
    public TileType GetTileType(int x, int y)
    {
        if (grid == null) return TileType.empty;
        return (TileType)grid[x, y];
    }

    public int CompareTileType(int cx, int cy, int x, int y)
    {
        if(x < 0 || y < 0 || x > maxX || y > maxY) return 0;
        if (grid == null) return 0;

        TileType selfType = (TileType)grid[cx, cy];
        TileType otherType = (TileType)grid[x, y];

        Debug.Log(selfType);
        return (selfType == otherType) ? 1 : 0;
    }

    void Start()
    {
        if (page1Renderer) { page1Renderer.enabled = false; }
        if (page2Renderer) { page2Renderer.enabled = false; }
         if (page3Renderer) { page3Renderer.enabled = false; }

        SaveTilemapToArrayAutoBounds();

        // Debug.Log(grid.GetLength(0));
        CheckBlockSprite(1, 1);
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                if (GetTileType(x, y) == TileType.block)
                {
                    GameObject block = tilemap_.GetInstantiatedObject(new Vector3Int(tilemap_.cellBounds.xMin + x, tilemap_.cellBounds.yMin + y, 0));
                    SpriteRenderer blockSprite = block.GetComponent<SpriteRenderer>();

                    int blockNum = CheckBlockSprite(x, y);

                    blockSprite.sprite = blocks[blockNum];
                }
            }
        }
    }

    private int CheckBlockSprite(int x,int y)
    {
        int num = 1;

        int adjacent = 0;

        adjacent |= CompareTileType(x ,y, x - 1, y + 1) << 7;
        adjacent |= CompareTileType(x, y, x , y + 1) << 6;
        adjacent |= CompareTileType(x, y, x + 1, y + 1) << 5;
        adjacent |= CompareTileType(x, y, x - 1, y  ) << 4;
        adjacent |= CompareTileType(x, y, x + 1, y  ) << 3;
        adjacent |= CompareTileType(x, y, x - 1, y - 1) << 2;
        adjacent |= CompareTileType(x, y, x , y - 1) << 1;
        adjacent |= CompareTileType(x, y, x + 1, y - 1) << 0;

        if            (adjacent == 0b11111111) num = 0;
        else if ((adjacent & 0b11011110) == 0b11010110)  num = 3;
        else if ((adjacent & 0b01011111) == 0b00011111) num = 4;
        else if ((adjacent & 0b01111011) == 0b01101011) num = 5;
        else if ((adjacent & 0b11111010) == 0b11111000) num = 6;
        else if ((adjacent & 0b01011110) == 0b00010110) num = 7;
        else if ((adjacent & 0b01011011) == 0b00001011) num = 8;
        else if ((adjacent & 0b01111010) == 0b01101000) num = 9;
        else if ((adjacent & 0b11011010) == 0b11010000) num = 10;
        else if ((adjacent & 0b01011010) == 0b00010000) num = 11;
        else if ((adjacent & 0b01011010) == 0b00000010) num = 12;
        else if ((adjacent & 0b01011010) == 0b00001000) num = 13;
        else if ((adjacent & 0b01011010) == 0b01000000) num = 14;
        else if ((adjacent & 0b11011110) == 0b01010010) num = 15;
        else if ((adjacent & 0b01011111) == 0b00011010) num = 16;
        else if ((adjacent & 0b01111011) == 0b01001010) num = 17;
        else if ((adjacent & 0b11111010) == 0b01011000) num = 18;
        else if ((adjacent & 0b01011110) == 0b00010010) num = 19;
        else if ((adjacent & 0b01011011) == 0b00001010) num = 20;
        else if ((adjacent & 0b01111010) == 0b01001000) num = 21;
        else if ((adjacent & 0b11011010) == 0b01010000) num = 22;
        else if ((adjacent & 0b11111111) == 0b11011111) num = 23;
        else if ((adjacent & 0b11111111) == 0b01111111) num = 24;
        else if ((adjacent & 0b11111111) == 0b11111011) num = 25;
        else if ((adjacent & 0b11111111) == 0b11111110) num = 26;
        else if ((adjacent & 0b11011110) == 0b01010110) num = 27;
        else if ((adjacent & 0b01011111) == 0b00011011) num = 28;
        else if ((adjacent & 0b01111011) == 0b01101010) num = 29;
        else if ((adjacent & 0b11111010) == 0b11011000) num = 30;
        else if ((adjacent & 0b11011110) == 0b11010010) num = 31;
        else if ((adjacent & 0b01011111) == 0b00011110) num = 32;
        else if ((adjacent & 0b01111011) == 0b01001011) num = 33;
        else if ((adjacent & 0b11111010) == 0b01111000) num = 34;
        else if ((adjacent & 0b11111111) == 0b11011110) num = 35;
        else if ((adjacent & 0b11111111) == 0b01011111) num = 36;
        else if ((adjacent & 0b11111111) == 0b01111011) num = 37;
        else if ((adjacent & 0b11111111) == 0b11111010) num = 38;
        else if ((adjacent & 0b11111111) == 0b11011010) num = 39;
        else if ((adjacent & 0b11111111) == 0b01011110) num = 40;
        else if ((adjacent & 0b11111111) == 0b01011011) num = 41;
        else if ((adjacent & 0b11111111) == 0b01111010) num = 42;
        else if ((adjacent & 0b11111111) == 0b11011011) num = 43;
        else if ((adjacent & 0b11111111) == 0b01111110) num = 44;
        else if ((adjacent & 0b01011010) == 0b00011000) num = 45;
        else if ((adjacent & 0b01011010) == 0b01000010) num = 46;
        else if ((adjacent & 0b11111111) == 0b01011010) num = 2;
        else num = 1;



        return num;
    }


}
