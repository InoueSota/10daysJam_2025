using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI系クラス参照のため

/// ポーズで Update を止めたい時に、ゲーム側シーンの MonoBehaviour を一括凍結/解凍する極小ユーティリティ。
public static class PauseFreezer
{
    static readonly List<(MonoBehaviour mb, bool wasEnabled)> _frozen = new();

    public static void Freeze(string pauseSceneName, bool strict = true)
    {
        if (!strict) return; // Time.timeScale=0 だけで運用するなら何もしない

        _frozen.Clear();
        var gameScene = FindGameScene(pauseSceneName);
        if (!gameScene.IsValid()) return;

        var roots = gameScene.GetRootGameObjects();
        foreach (var go in roots)
        {
            var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var m in monos)
            {
                if (!m) continue;

                // 既に無効なら「無効だった」という事実だけ記録
                if (!m.enabled)
                {
                    _frozen.Add((m, false));
                    continue;
                }

                // ポーズ中も動かしたい印（任意）
                if (m is IPauseIgnore) continue;

                // 🎯 UI 系コンポーネントは除外
                if (m is Canvas ||
                    m is GraphicRaycaster ||
                    m is CanvasScaler ||
                    m is Image||
                    m is UnityEngine.Rendering.Universal.Light2D||
                    m is UnityEngine.Rendering.Volume) continue;

                m.enabled = false;
                _frozen.Add((m, true));
            }
        }
        Debug.Log($"[PauseFreezer] Frozen behaviours: {_frozen.Count}");
    }

    public static void Thaw()
    {
        foreach (var (mb, wasEnabled) in _frozen)
        {
            if (mb) mb.enabled = wasEnabled;
        }
        _frozen.Clear();
        Debug.Log("[PauseFreezer] Thawed.");
    }

    static Scene FindGameScene(string pauseSceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != pauseSceneName) return s;
        }
        return SceneManager.GetActiveScene();
    }
}

/// ポーズ中も止めたくないコンポーネントに付ける印（任意）
public interface IPauseIgnore { }
