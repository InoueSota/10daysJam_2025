using System.Drawing;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PaperManagerScript : MonoBehaviour
{
    [SerializeField] PaperScript paperPrefab;
    [SerializeField] TilemapManager tilemap;
    [SerializeField] PaperScript firstPaper;
    [SerializeField] PaperScript secondPaper;
    [SerializeField] UndoManager undoManager;
    DivisionLineManager divisionLine;
    PlayerCut playerCut;
    [SerializeField]
    ParticleSystem[] effect;

    [SerializeField] Vector2 paperSizeBase;
    [SerializeField] Vector3 gridOffset;

    [SerializeField] Transform[] pageTransform = new Transform[2];
    [SerializeField] Vector3[] blockOffset = new Vector3[2];

    Vector2 pos;
    bool isDivision, preIsDivision;
    bool isActive, preIsActive;
    bool isCut = false;

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

        undoManager = GameObject.FindGameObjectWithTag("GameController").gameObject.GetComponent<UndoManager>();
    }

    // Update is called once per frame
    void Update()
    {

        isActive = playerCut.GetIsActive();
        isDivision = playerCut.GetIsDivision();

        if (isCut == true)
        {
            if (Input.GetButtonDown("Reset"))
            {
                effect[0].Play();
                blockOffset[0] = Vector3.zero;
                blockOffset[1] = Vector3.zero;
                FixPaper();
            }

            if (isActive == true && preIsActive == false)
            {
                effect[0].Play();
                blockOffset[0] = -pageTransform[0].localPosition;
                blockOffset[1] = -pageTransform[1].localPosition;
                FixPaper();
            }

            if (isDivision == false && preIsDivision == true)
            {
                effect[0].Play();
                blockOffset[0] = -pageTransform[0].localPosition;
                blockOffset[1] = -pageTransform[1].localPosition;
                FixPaper();
            }
        }

        if (Input.GetButtonDown("Undo"))
        {
            if (undoManager.GetIsDivision() == true)
            {
                if (isCut == false)
                {
                    blockOffset[0] = pageTransform[0].localPosition;
                    blockOffset[1] = pageTransform[1].localPosition;

                    bool isCutHorizontal = false;
                    if (undoManager.GetIsDivisionMode() == 0) isCutHorizontal = true;
                    Vector3 cutPos = undoManager.GetPrevDivisionPosition();
                    cutPos.x -= gridOffset.x;
                    cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;
                    CutPaper(cutPos, isCutHorizontal);
                }
                else if(isCut == true)
                {
                    Vector3 cutPos = undoManager.GetPrevDivisionPosition();
                    if (playerCut.GetDivisionPosition() != new Vector2(cutPos.x , cutPos.y)) {

                        blockOffset[0] = pageTransform[0].localPosition;
                        blockOffset[1] = pageTransform[1].localPosition;

                        Debug.Log("“{‚è");
                        bool isCutHorizontal = false;
                        if (undoManager.GetIsDivisionMode() == 0) isCutHorizontal = true;
                        cutPos.x -= gridOffset.x;
                        cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;
                        CutPaper(cutPos, isCutHorizontal);
                    }
                }
            }
            else
            {
                //blockOffset[0] = -undoManager.GetObjectParentPosition1();
                //blockOffset[1] = -undoManager.GetObjectParentPosition2();
                FixPaper();
            }
        }


       

            if (playerCut.GetDivisionFlag() == true)
        {
            blockOffset[0] = -pageTransform[0].localPosition;
            blockOffset[1] = -pageTransform[1].localPosition;

            bool isCutHorizontal = false;
            if(divisionLine.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL) isCutHorizontal = true;
            Vector3 cutPos = playerCut.GetDivisionPosition();
            cutPos.x -= gridOffset.x ;
            cutPos.y = paperSizeBase.y - cutPos.y - 8.5f;
            CutPaper(cutPos, isCutHorizontal);
        }

        preIsDivision = isDivision;
        preIsActive = isActive;
    }

    public void FixPaper()
    {
        firstPaper.QuadSetter(gridOffset + blockOffset[0], paperSizeBase);
        secondPaper.QuadSetter(gridOffset + blockOffset[1], Vector3.zero);
        isCut = false; 
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
        isCut = true;
    }

    private void SummonPaper(Vector3 pos, Vector2 size)
    {
        PaperScript secondPaper = Instantiate(paperPrefab, pos, Quaternion.identity,transform);
        secondPaper.QuadSetter(pos, size);
    }
}
