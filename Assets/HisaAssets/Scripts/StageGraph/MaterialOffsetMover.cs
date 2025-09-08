using UnityEngine;

public class MaterialOffsetMover : MonoBehaviour
{
    [Header("動かしたいRenderer")]
    public Material targetMaterial;

    [Header("スクロール速度 (X,Y)")]
    public Vector2 scrollSpeed = new Vector2(0.1f, 0f);

    private Vector2 offset = Vector2.zero;

    void Start()
    {
        
    }

    void Update()
    {
        // 時間経過でオフセットを加算
        offset += scrollSpeed * Time.deltaTime;

        // 値をループさせる（0〜1範囲に収める）
        offset.x = Mathf.Repeat(offset.x, 1f);
        offset.y = Mathf.Repeat(offset.y, 1f);

        // マテリアルに適用
        targetMaterial.mainTextureOffset = offset;
    }
}
