using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    

    [Header("Goal")]
    [SerializeField] private GameObject groupAfterGoal;
    [SerializeField] private Text goalDirectionT;
    [SerializeField] GameObject[] clearSelectTexts;
    bool[] cleaerSelectTextsActive=new bool[3];//setActiveをfalseにしても反応ないのでゴリ押し
    [SerializeField] GameObject areaOpenText;
    bool areaOpenFlag;
    void Start()
    {
        
    }

    void Update()
    {
        if (!groupAfterGoal.activeSelf) { return; }
        for (int i = 0; i < 3; i++)
        {
            if (clearSelectTexts[i]) clearSelectTexts[i].SetActive(!cleaerSelectTextsActive[i]);//cleaerSelectTextsActiveがtrueなら非表示に
        }
        if (areaOpenFlag)
        {
            areaOpenText.SetActive(true);
        }
    }

    // Setter
    public void Goal(int _goalDirection,int type)
    {
        // クリア後のUIを表示する
        groupAfterGoal.SetActive(true);

        //GameObject player = GameObject.FindGameObjectWithTag("Player");

       // SaveData save = SaveSystem.Load(1);

        string newText = "";

        // 右
        if (_goalDirection == 0) { newText="右のステージ"; }
        // 上
        else if (_goalDirection == 1) { newText = "上のステージ"; }
        // 左
        else if (_goalDirection == 2) { newText = "左のステージ"; }
        // 下
        else if (_goalDirection == 3) { newText = "下のステージ"; }

        if (type == 0)//ステージが無い場合
        {
            newText = "";
        }
        else if (type == 1) 
        {
            newText += "を開放した!";
        }
        else if (type == 2)
        {
            newText += "は開放済み";
        }
        else if (type == 3) 
        {
            newText = "ステージの端に到達した!";
        }

        goalDirectionT.text = newText;
    }
    public void Reset()
    {
        groupAfterGoal.SetActive(false);
    }

    public void SetActiveFalseIndex(int index)
    {
        cleaerSelectTextsActive[index] = true;
        clearSelectTexts[index].SetActive(false);
    }

    public void ClearCanvasActive()
    {
        groupAfterGoal.SetActive(true);
    }

    public void AreaOpen()
    {

        areaOpenFlag = true;
    }
}
