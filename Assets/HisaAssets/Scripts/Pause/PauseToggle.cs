using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseToggle : MonoBehaviour
{
    [SerializeField] string pauseSceneName = "PauseScene";

    public bool paused;
    public float nextPauseCT;//ポーズからセレクト画面に戻った時にすぐポーズしてしまわないようにする

    void Update()
    {
        if (nextPauseCT > 0) nextPauseCT -=Time.deltaTime;
    }




    // PauseToggle.cs（差分のみ）
    public void Pause(string sceneName)
    {
        if(nextPauseCT>0) {return;}
       // paused = true;
        Time.timeScale = 0f;

        // ★ 追加：ゲーム側の MonoBehaviour を停止（必要なければ false）
        PauseFreezer.Freeze(pauseSceneName, strict: true);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        nextPauseCT = 0.2f;
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
