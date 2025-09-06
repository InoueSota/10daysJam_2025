using UnityEngine;

public class PaperScript : MonoBehaviour
{

    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public  void QuadSetter(Vector3 pos,Vector2 Size)
    {
        this.transform.position = pos;
        spriteRenderer.size = Size;

    }

}
