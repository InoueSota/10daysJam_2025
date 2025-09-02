using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private TilemapRenderer page1Renderer;
    [SerializeField] private TilemapRenderer page2Renderer;
    [SerializeField] private TilemapRenderer page3Renderer;

    void Start()
    {
        page1Renderer.enabled = false;
        page2Renderer.enabled = false;
        page3Renderer.enabled = false;
    }

    void Update()
    {
        
    }
}
