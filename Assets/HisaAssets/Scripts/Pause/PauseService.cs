// PauseService.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PauseService
{
    public static bool IsPaused { get; private set; }
    static float _prevTimeScale = 1f;
    static readonly List<MonoBehaviour> _disabledBehaviours = new();
    static string _pauseSceneName = "PauseScene"; // ←作るシーン名
    static bool _strictStopUpdate = true;         // true: Updateも止めたい（enabled切る）

    /// <summary>必要なら変更（例: 起動時に設定）</summary>
    public static void Configure(string pauseSceneName, bool strictStopUpdate)
    {
        _pauseSceneName = pauseSceneName;
        _strictStopUpdate = strictStopUpdate;
    }

    public static void TogglePause(MonoBehaviour caller = null)
    {
        if (IsPaused) Resume(caller);
        else Pause(caller);
    }

    public static async void Pause(MonoBehaviour caller = null)
    {
        if (IsPaused) return;
        IsPaused = true;

        // 1) 時間停止（物理・アニメ等を止める）
        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // 2) 厳密に止めたい場合は、現在アクティブシーンのMonoBehaviourをenabled=falseに
        _disabledBehaviours.Clear();
        if (_strictStopUpdate)
        {
            var active = SceneManager.GetActiveScene();
            var roots = active.GetRootGameObjects();

            // 例外ホワイトリスト：描画/カメラ/オーディオ/ポスト等は動かしたいケースあり
            bool IsWhitelisted(Component c) =>
                c is Camera || c is AudioListener || c.GetType().Name.Contains("Volume") ||
                c.GetComponent<IPauseIgnore>() != null;

            foreach (var go in roots)
            {
                var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var m in monos)
                {
                    if (!m) continue;
                    if (IsWhitelisted(m)) continue;
                    if (!m.enabled) continue;
                    m.enabled = false;
                    _disabledBehaviours.Add(m);
                }
            }
        }

        // 3) ポーズ用シーンをAdditiveでロード
        var op = SceneManager.LoadSceneAsync(_pauseSceneName, LoadSceneMode.Additive);
        if (op != null)
        {
            while (!op.isDone) await System.Threading.Tasks.Task.Yield();
            // フォーカスをポーズシーンに（UIの自動選択などに有利）
            var pauseScene = SceneManager.GetSceneByName(_pauseSceneName);
            if (pauseScene.IsValid()) SceneManager.SetActiveScene(pauseScene);
        }
    }

    public static async void Resume(MonoBehaviour caller = null)
    {
        if (!IsPaused) return;
        IsPaused = false;

        // 1) ポーズ用シーンをアンロード
        var pause = SceneManager.GetSceneByName(_pauseSceneName);
        if (pause.IsValid() && pause.isLoaded)
        {
            var op = SceneManager.UnloadSceneAsync(pause);
            if (op != null)
            {
                while (!op.isDone) await System.Threading.Tasks.Task.Yield();
            }
        }

        // 2) 停止前のMonoBehaviourを再有効化
        if (_strictStopUpdate)
        {
            foreach (var m in _disabledBehaviours)
            {
                if (m) m.enabled = true;
            }
            _disabledBehaviours.Clear();
        }

        // 3) 時間再開
        AudioListener.pause = false;
        Time.timeScale = _prevTimeScale;

        // 元のゲームシーンをアクティブに戻す（任意）
        var gameScene = SceneManager.GetActiveScene();
        if (gameScene.IsValid()) SceneManager.SetActiveScene(gameScene);
    }
}

/// <summary>
/// 「ポーズ中も止めたくない」コンポーネントに付ける印（カメラ制御の見た目だけなど）
/// </summary>
//public interface IPauseIgnore { }
