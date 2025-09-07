using UnityEngine;

public class PaperScript : MonoBehaviour
{

   [SerializeField]  SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
