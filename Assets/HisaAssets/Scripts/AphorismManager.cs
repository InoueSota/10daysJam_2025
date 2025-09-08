using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AphorismManager : MonoBehaviour
{
    [SerializeField] float sceneChangeCT;
    float curSceneChangeCT;
    [System.Serializable]
    public class HaikuSet
    {
        [Tooltip("5音（上の句）")]
        public string line5a;

        [Tooltip("7音（中の句）")]
        public string line7;

        [Tooltip("5音（下の句）")]
        public string line5b;
    }

    [Header("俳句リスト (5-7-5)")]
    public List<HaikuSet> haikuSets = new List<HaikuSet>();

    [SerializeField] TypewriterEffect[] haikuText;
    public static int index;
    bool sceneChange;
    [SerializeField] GameObject buttonUI;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        buttonUI.SetActive(false);

        haikuText[0].SetText(haikuSets[index].line5a);
        haikuText[1].SetText(haikuSets[index].line7);
        haikuText[2].SetText(haikuSets[index].line5b);
    }

    // Update is called once per frame
    void Update()
    {
        if (sceneChange) { return; }
        curSceneChangeCT += Time.deltaTime;
        if (curSceneChangeCT> sceneChangeCT)
        {
            buttonUI.SetActive(true);
            if (Input.GetButtonDown("Select"))
            {
                sceneChange = true;
                index++;
                if (index >= haikuSets.Count)
                {
                    index= 0;   
                }
                SceneManager.LoadScene("StageSelectScene");
            }
        }
    }
}
