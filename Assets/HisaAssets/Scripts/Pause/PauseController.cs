using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("ゲーム側と区別するための名前")]
    [SerializeField] string pauseSceneName = "PauseScene";
    [SerializeField] string selectSceneName = "StageSelect";

    [SerializeField] SetTextScript debugText;
    [SerializeField] int index;
    public bool change;
    public Vector2 inputDire;
    float inputCoolTime;
    bool select;
    void Start()
    {
        // PauseScene が Additive でロードされた時点で呼ばれる
        Time.timeScale = 0f;
        // PauseFreezer.Freeze(pauseSceneName, strict: true);
    }

    private void Update()
    {
        InputDire();
        debugText.SetText(index);

        if (change) {return; }
        if (inputDire.y < 0)
        {
            index++;

            if (index >= 3) { index = 0; }
        }
        else if (inputDire.y > 0)
        {
            index--;
            if (index < 0) { index = 2; }

        }
        SelectMode();

    }

    // === ボタンから呼ぶ関数 ===

    // 「ゲームに戻る」
    [ContextMenu("ゲームに戻る")]

    public void OnResume()
    {
        PauseFreezer.Thaw();
        Time.timeScale = 1f;

        // PauseScene を閉じる
        var s = SceneManager.GetSceneByName(pauseSceneName);
        if (s.IsValid() && s.isLoaded) SceneManager.UnloadSceneAsync(s);
    }

    // 「ステージをリスタート」
    [ContextMenu("ステージをリスタート")]

    public void OnRestart()
    {
        PauseFreezer.Thaw();
        Time.timeScale = 1f;

        var game = FindGameScene();
        if (game.IsValid()) SceneManager.LoadScene(game.name, LoadSceneMode.Single);
    }

    // 「セレクト画面に戻る」
    [ContextMenu("セレクト画面に戻る")]
    public void OnGoSelect()
    {
        //PauseFreezer.Thaw();
        Time.timeScale = 1f;
        SceneManager.LoadScene(selectSceneName, LoadSceneMode.Single);
    }

    // === ゲームシーンを見つけるヘルパー ===
    Scene FindGameScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != pauseSceneName) return s;
        }
        return SceneManager.GetActiveScene();
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
                OnRestart();
            }
            //セレクトへ戻る
            else if (index == 2)
            {
                Debug.Log("セレクト画面に戻る");
                OnGoSelect();
            }
        }

        if (!change && Input.GetButtonDown("Menu"))
        {
            change = true;
            //ゲームに戻る
            OnResume();
        }
    }

    void InputDire()
    {
        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");

        if (inputCoolTime > 0)
        {
            inputCoolTime -= Time.unscaledDeltaTime;


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


    }
}
