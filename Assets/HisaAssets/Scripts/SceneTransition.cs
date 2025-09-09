using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    private Animator animator;
    private string nextSceneName;

    // オプション: 待機秒数 (UnscaledTime, 0で無効)
    [SerializeField] private float postLoadDelay = 0.05f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        // ポーズ中でも進むように
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        DontDestroyOnLoad(gameObject); // シーン跨ぎでも消えない
    }

    /// <summary>
    /// シーン遷移開始
    /// </summary>
    public void StartTransition(string sceneName)
    {
        nextSceneName = sceneName;
        animator.SetTrigger("FadeOut"); // FadeOutアニメ再生
    }

    /// <summary>
    /// FadeOutアニメーション終わりに呼ばれる（AnimationEventから呼ぶ）
    /// </summary>
    public void OnFadeOutComplete()
    {
        StartCoroutine(LoadSceneAsyncCoroutine());
    }

    private IEnumerator LoadSceneAsyncCoroutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false; // 有効化は手動で

        // 0.9(=90%)になるまで読み込みを待機
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // シーンを有効化
        op.allowSceneActivation = true;

        // 完全にロードが終わるまで待機
        while (!op.isDone)
        {
            yield return null;
        }

        // 1フレーム待つ（ロード直後のスパイクを吸収）
        yield return null;

        // 任意で追加の待機（黒画面維持）
        if (postLoadDelay > 0f)
        {
            float t = 0f;
            while (t < postLoadDelay)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // ロードが完全に終わったタイミングでFadeIn開始
        animator.SetTrigger("FadeIn");
    }

    /// <summary>
    /// FadeInアニメーション終わりに呼ばれる（AnimationEventから呼ぶ）
    /// </summary>
    public void OnFadeInComplete()
    {
        Destroy(gameObject); // トランジション終了
    }
}
