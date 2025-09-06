using UnityEngine;

public class PaperManagerScript : MonoBehaviour
{
    [SerializeField] PaperScript paperPrefab;
    [SerializeField] PaperScript firstPaper;

    [SerializeField] Vector2 paperSizeBase;

    [SerializeField] Vector2 c;
    [SerializeField] bool isc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            CutPaper(c, isc);



        }
    }

    private void CutPaper(Vector3 cutPos, bool isCutHorizontal)
    {

        Vector2 paperSize = paperSizeBase;

        if(isCutHorizontal == true) paperSize.y = paperSizeBase.y - cutPos.y;
        paperSize.y = cutPos.x;

    }

    private void SummonPaper(Vector3 pos, Vector2 size)
    {
        PaperScript paper = Instantiate(paperPrefab, pos, Quaternion.identity,transform);
        SpriteRenderer paperSprite = paper.gameObject.GetComponent<SpriteRenderer>();

        paperSprite.transform.localPosition = Vector3.zero;
        paperSprite.size = size;
    }
}
