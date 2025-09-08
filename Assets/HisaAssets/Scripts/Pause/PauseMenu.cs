// PauseMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] SetTextScript debugText;
    [SerializeField] int index;
    bool change;

    [SerializeField] string pauseSceneName = "PauseScene";
    [SerializeField] string selectSceneName = "StageSelect";
    void Start()
    {
        // PauseScene が Additive でロードされた時点で呼ばれる
        Time.timeScale = 0f;
        PauseFreezer.Freeze(pauseSceneName, strict: true);
    }
    public void OnResume()
    {
        PauseFreezer.Thaw();              // ← スクリプトから呼ぶ
        Time.timeScale = 1f;
        var s = SceneManager.GetSceneByName(pauseSceneName);
        if (s.IsValid() && s.isLoaded) SceneManager.UnloadSceneAsync(s);
    }

    public void OnRestartStage()
    {
        PauseFreezer.Thaw();              // ← リスタート時も戻しておく
        Time.timeScale = 1f;

        var game = FindGameScene();
        if (game.IsValid()) SceneManager.LoadScene(game.name, LoadSceneMode.Single);
    }

    public void OnGoToSelect()
    {
        PauseFreezer.Thaw();              // ← セレクトに行く前も戻す
        Time.timeScale = 1f;
        SceneManager.LoadScene(selectSceneName, LoadSceneMode.Single);
    }

    Scene FindGameScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != pauseSceneName) return s;
        }
        return SceneManager.GetActiveScene();
    }

    private void Update()
    {
        debugText.SetText(index);

        if (Input.GetKeyDown(KeyCode.W))
        {
            index++;

            if (index > 3) { index = 0; }
        }
        SelectMode();

    }

    void SelectMode()
    {
        if (change) { return; }

        if (Input.GetButtonDown("Select"))
        {
            change = true;
            //ゲームに戻る
            if (index == 0)
            {
                OnResume();
                
            }
            //ステージをリセットする
            else if (index == 1)
            {
                OnRestartStage();
            }
            //セレクトへ戻る
            else if (index == 2)
            {
                OnGoToSelect();
            }
        }

        if (!change&&Input.GetButtonDown("Back"))
        {
            change = true;
            //ゲームに戻る
            OnResume();
        }
    }
}
