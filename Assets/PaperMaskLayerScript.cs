using UnityEngine;

public class PaperMaskLayerScript : MonoBehaviour
{
    [SerializeField] SpriteRenderer paper;
    [SerializeField] Vector3 offsetPlus;
    [SerializeField] Vector3 offsetMinus;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 paperSize = Vector3.zero;
        paperSize.x = paper.size.x;
        paperSize.y = paper.size.y;
        transform.localScale = paperSize - offsetPlus - offsetMinus;
        Vector3 pos = offsetPlus;
        pos.y *= -1f;
        this.transform.localPosition = pos;
    }
}
