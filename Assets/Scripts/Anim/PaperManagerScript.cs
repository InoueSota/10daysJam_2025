using System.Drawing;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PaperManagerScript : MonoBehaviour
{
    [SerializeField] PaperScript paperPrefab;
    [SerializeField] TilemapManager tilemap;
    [SerializeField] PaperScript firstPaper;
    [SerializeField] PaperScript secondPaper;
    DivisionLineManager divisionLine;
    PlayerCut playerCut;

    [SerializeField] Vector2 paperSizeBase;
    [SerializeField] Vector3 gridOffset;

    [SerializeField] Transform[] pageTransform = new Transform[2];
    [SerializeField] Vector3[] blockOffset = new Vector3[2];

    Vector2 pos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCut = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerCut>();
        divisionLine = playerCut.GetDivisionLineManager();
        pos = this.transform.position;

        pageTransform[0] = tilemap.GetPage1TransForm();
        pageTransform[1] = tilemap.GetPage2TransForm();

        firstPaper.transform.parent = pageTransform[0];
        secondPaper.transform.parent = pageTransform[1];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Reset")) FixPaper();

        if (Input.GetButtonDown("Undo")) {
         
        }

        if (playerCut.GetDivisionFlag() == true)
        {
            bool isCutHorizontal = false;
            if(divisionLine.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL) isCutHorizontal = true;
            Vector3 cutPos = playerCut.GetDivisionPosition();
            Debug.Log(cutPos);
            cutPos.x -= gridOffset.x ;
            cutPos.y = paperSizeBase.y - cutPos.y - 1.5f;
            Debug.Log(cutPos);
            CutPaper(cutPos, isCutHorizontal);

            //blockOffset[0] = -pageTransform[0].localPosition;
            //blockOffset[1] = -pageTransform[1].localPosition;
        }
    }

    private void FixPaper()
    {
        firstPaper.QuadSetter(gridOffset, paperSizeBase);
        secondPaper.QuadSetter(gridOffset, Vector3.zero);
    }

    private void CutPaper(Vector3 cutPos, bool isCutHorizontal)
    {
        Vector3 paperSize = paperSizeBase;
        Vector2 secondPaperSize = paperSizeBase;
        Vector3 secondPaperPos = gridOffset;

        if (isCutHorizontal == false)
        {
            paperSize.x = cutPos.x;
            secondPaperSize.x = paperSizeBase.x - cutPos.x;
            secondPaperPos.x =  cutPos.x + gridOffset.x;
        }
        else
        {
            paperSize.y = cutPos.y;
            secondPaperSize.y = paperSizeBase.y - cutPos.y;
            secondPaperPos.y =  gridOffset.y - cutPos.y;
        }

        firstPaper.QuadSetter(gridOffset + blockOffset[0], paperSize);
        secondPaper.QuadSetter(secondPaperPos + blockOffset[1], secondPaperSize);
    }

    private void SummonPaper(Vector3 pos, Vector2 size)
    {
        PaperScript secondPaper = Instantiate(paperPrefab, pos, Quaternion.identity,transform);
        secondPaper.QuadSetter(pos, size);
    }
}
