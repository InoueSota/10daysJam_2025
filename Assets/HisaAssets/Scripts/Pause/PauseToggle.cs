using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseToggle : MonoBehaviour
{
    [SerializeField] string pauseSceneName = "PauseScene";

    public bool paused;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!paused) Pause();
            else Resume();
        }
    }

    

    // PauseToggle.cs（差分のみ）
    void Pause()
    {
       // paused = true;
        Time.timeScale = 0f;

        // ★ 追加：ゲーム側の MonoBehaviour を停止（必要なければ false）
        //PauseFreezer.Freeze(pauseSceneName, strict: true);

        SceneManager.LoadScene(pauseSceneName, LoadSceneMode.Additive);
    }

    public void Resume()
    {
        // ★ 追加：元の enabled 状態に戻す
       // PauseFreezer.Thaw();

        //paused = false;
        Time.timeScale = 1f;
       // var s = SceneManager.GetSceneByName(pauseSceneName);
       // if (s.IsValid() && s.isLoaded) SceneManager.UnloadSceneAsync(s);
    }
}
