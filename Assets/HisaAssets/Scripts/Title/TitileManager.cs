using UnityEngine;
using UnityEngine.SceneManagement;

public class TitileManager : MonoBehaviour
{
    [SerializeField] string selectSceneName;
    [SerializeField] GameObject saveDeleteObj;
    bool delete;
    float deleteTime;
    [SerializeField] GaugeScript deleteGauge;

    [SerializeField] SceneTransition sceneTransitionPrefab;
    SceneTransition sceneTransitionObj;
    public bool isSceneChange;
    float sceneChangeCT;
    [SerializeField] GameObject allClear;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deleteGauge.SetRatio(0);

        if (PlayerPrefs.GetString("King") == "true")
        {
            allClear.SetActive(true);
        }

        StageSelectManager.areaSelect = true;//タイトルに戻ったらエリアセレクトから始める
        StageSelectManager.allOpenFlag =false;
    }

    // Update is called once per frame
    void Update()
    {

        SceneChange();
        SaveDelete();

    }

    public void SetStart()
    {
        sceneTransitionObj = Instantiate(sceneTransitionPrefab);
        sceneTransitionObj.StartTransition(selectSceneName);
        isSceneChange = true;
    }

    void SceneChange()
    {
        if (isSceneChange) { return; }
        if (sceneChangeCT < 0.5f)
        {
            sceneChangeCT += Time.deltaTime;
            return;
        }

        if (Input.GetButtonDown("Select"))
        {
            //SetStart();
        }
    }

    void SaveDelete()
    {
        if (delete) { return; }
       
        if (Input.GetButton("Menu"))//GetButton("Menu")
        {
            deleteTime += Time.deltaTime;
            deleteGauge.SetRatio(deleteTime/2);

            if (deleteTime > 2)
            {
                delete = true;
                Instantiate(saveDeleteObj);
                StageSelectManager.Initalize();
                deleteGauge.SetRatio(0);
                allClear.SetActive(false);
            }
        }
        else
        {
            deleteTime = 0;
            deleteGauge.SetRatio(deleteTime / 2);

        }

    }
}
