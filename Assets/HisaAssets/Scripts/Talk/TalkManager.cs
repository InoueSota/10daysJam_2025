using UnityEngine;
using UnityEngine.SceneManagement;

public class TalkManager : MonoBehaviour
{
    [Header("ゲーム側と区別するための名前")]
    [SerializeField] string talkSceneName = "TalkScene";

    [SerializeField] GameObject[] talkObjs;
    public int curIndex;
    float pushCT;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       for (int i = 1; i < talkObjs.Length; i++)
        {
            talkObjs[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        pushCT -= Time.unscaledDeltaTime;
        ChangeTalk();
    }

    void ChangeTalk()
    {
        if (pushCT > 0.3f) { return; }

        if (Input.GetButtonDown("Select"))
        {
            curIndex++;
            if (curIndex == talkObjs.Length) {
                //会話終了
                OnResume();
            }
            else
            {
                //文字送り
                talkObjs[curIndex-1].SetActive(false);
                talkObjs[curIndex].SetActive(true);
            }
        }
    }

    [ContextMenu("ゲームに戻る")]

    public void OnResume()
    {
        PauseFreezer.Thaw();
        Time.timeScale = 1f;

        // PauseScene を閉じる
        var s = SceneManager.GetSceneByName(talkSceneName);
        if (s.IsValid() && s.isLoaded) SceneManager.UnloadSceneAsync(s);
    }

}
