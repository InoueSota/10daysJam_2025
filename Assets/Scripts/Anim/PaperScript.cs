using DG.Tweening.Core.Easing;
using UnityEngine;
using static TilemapManager;

public class PaperScript : MonoBehaviour
{

   [SerializeField]  SpriteRenderer spriteRenderer;

    [SerializeField] Sprite[] sprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        string areaName = gameManager.GetAreaName();
        if (areaName == "Area2") spriteRenderer.sprite = sprite[0];
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public  void QuadSetter(Vector3 pos,Vector2 Size)
    {
        this.transform.localPosition = pos;
        spriteRenderer.size = Size;

    }

}
