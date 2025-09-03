using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private TilemapRenderer page1Renderer;
    [SerializeField] private TilemapRenderer page2Renderer;
    [SerializeField] private TilemapRenderer page3Renderer;

    void Start()
    {
        if (page1Renderer) { page1Renderer.enabled = false; }
        if (page2Renderer) { page2Renderer.enabled = false; }
        if (page3Renderer) { page3Renderer.enabled = false; }
    }
}
