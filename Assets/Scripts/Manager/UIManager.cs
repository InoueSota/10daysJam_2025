using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Goal")]
    [SerializeField] private GameObject groupAfterGoal;
    [SerializeField] private Text goalDirectionT;
    [SerializeField] Animator[] clearSelectTexts;
    bool nextStageFalse;
    [SerializeField] GameObject areaOpenText;
    bool areaOpenFlag;
    Vector2 inputDire;
    float inputCoolTime;
    int curSelectIndex;//0次へ1やりなおす2セレクトへ
    int preSlectIndex;
    float inputDelay;
    [SerializeField] Text areaOpenTexts;

    [Header("Frame")]
    [SerializeField] private Image backFrame;
    [SerializeField] private Image frame;
    [Header("Area1 Colors")]
    [SerializeField] private Color area1BackColor;
    [SerializeField] private Color area1Color;
    [Header("Area2 Colors")]
    [SerializeField] private Color area2BackColor;
    [SerializeField] private Color area2Color;
    [Header("Area3 Colors")]
    [SerializeField] private Color area3BackColor;
    [SerializeField] private Color area3Color;
    [Header("Area4 Colors")]
    [SerializeField] private Color area4BackColor;
    [SerializeField] private Color area4Color;
    [Header("Area5 Colors")]
    [SerializeField] private Color area5BackColor;
    [SerializeField] private Color area5Color;

    [SerializeField] SetTextScript stageNameText;

    void Start()
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        if (gameManager.GetAreaName() == "Area1"){
            backFrame.color = area1BackColor;
            frame.color = area1Color;
        }
        else if (gameManager.GetAreaName() == "Area2") {
            backFrame.color = area2BackColor;
            frame.color = area2Color;
        }
        else if (gameManager.GetAreaName() == "Area3") {
            backFrame.color = area3BackColor;
            frame.color = area3Color;
        }
        else if (gameManager.GetAreaName() == "Area4") {
            backFrame.color = area4BackColor;
            frame.color = area4Color;
        }
        else if (gameManager.GetAreaName() == "Area5") {
            backFrame.color = area5BackColor;
            frame.color = area5Color;
        }

        stageNameText.SetText(StageCell.lastSelectSellName);
    }

    void Update()
    {
        if (!groupAfterGoal.activeSelf) { return; }
       // if (nextStageFalse) clearSelectTexts[0].gameObject.SetActive(!nextStageFalse);//cleaerSelectTextsActiveがtrueなら非表示に

        if (inputDelay < 1.5f)
        {
            inputDelay += Time.deltaTime;
            return;
        }

        if (areaOpenFlag)
        {
            areaOpenText.SetActive(true);
        }
        clearSelectTexts[curSelectIndex].SetBool("Select", true);

        InputDire();
        ChangeIndex();

    }

    // Setter
    public void Goal(int _goalDirection, int type)
    {
        // クリア後のUIを表示する
        groupAfterGoal.SetActive(true);

        //GameObject player = GameObject.FindGameObjectWithTag("Player");

        // SaveData save = SaveSystem.Load(1);

        string newText = "";

        // 右
        if (_goalDirection == 0) { newText = "右のステージ"; }
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
            newText += "を解放した!";
        }
        else if (type == 2)
        {
            newText += "は解放済み";
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

    public void SetActiveFalseIndex()
    {
        nextStageFalse = true;
        curSelectIndex = 1;
        preSlectIndex = 1;
        clearSelectTexts[curSelectIndex].SetBool("Select", true);
    }

    public void ClearCanvasActive()
    {
        groupAfterGoal.SetActive(true);
    }

    public void AreaOpen()
    {

        areaOpenFlag = true;
    }

    void ChangeIndex()
    {
        preSlectIndex = curSelectIndex;
        if (nextStageFalse)
        {
            if (inputDire.y < 0)
            {

                curSelectIndex++;

                if (curSelectIndex > 1)
                {
                    curSelectIndex = 0;//次のステージは選択できないため。
                }
            }
            else if (inputDire.y > 0)
            {
                curSelectIndex--;
                if (curSelectIndex < 0)//次のステージは選択できないため。
                {
                    curSelectIndex = 1;
                }
            }

        }
        else
        {
            if (inputDire.y < 0)
            {
                curSelectIndex++;
                if (curSelectIndex > 1)
                {
                    curSelectIndex = 0;
                }
            }
            else if (inputDire.y > 0)
            {
                curSelectIndex--;
                if (curSelectIndex < 0)
                {
                    curSelectIndex = 1;
                }
            }
        }

        //切り替えたら切り替える前のUIのフラグを下ろす
        if (preSlectIndex != curSelectIndex)
        {
            clearSelectTexts[preSlectIndex].SetBool("Select", false);

        }
    }

    void InputDire()
    {
        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");
        if (inputCoolTime > 0)
        {
            inputCoolTime -= Time.deltaTime;


            //ボタン連打で動けるようにする
            if (inputDire.magnitude <= 0)
            {
                inputCoolTime = 0;

            }
            inputDire = Vector2.zero;
            return;
        }

        if (inputDire.magnitude > 0)
        {
            inputCoolTime = 0.3f;
        }

        //Debug.Log("InputDire" + inputDire);

    }

    public bool GetInputDelay()
    {
        if (inputDelay >= 1.5f)
        {
            return true;
        }

        return false;
    }

    public int GetCurSelectIndex() { return curSelectIndex; }

    public void SetAreaOpenText(int areOpenIndex)
    {
        if (areOpenIndex == 3)//コンテスト用にエリア3解放時のテキストを変える
        {
            areaOpenTexts.text = "はさ爺が何かを言いたそうだ...";
            areaOpenTexts.transform.localScale *= 0.7f;
        }
        else
        {
            areaOpenTexts.text = "エリア" + areOpenIndex + "が解放した!";

        }
    }


}
