using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// ポーズで Update を止めたい時に、ゲーム側シーンの MonoBehaviour を一括凍結/解凍する極小ユーティリティ。
public static class PauseFreezer
{
    // 凍結した対象と、凍結前の enabled 状態を記録
    static readonly List<(MonoBehaviour mb, bool wasEnabled)> _frozen = new();

    /// pauseSceneName: Additive で重ねるポーズ用シーン名（ゲーム側を判別するために使う）
    /// strict: true ならゲーム側 MonoBehaviour を enabled=false にして Update を実質停止
    public static void Freeze(string pauseSceneName, bool strict = true)
    {
        if (!strict) return; // Time.timeScale=0 だけで運用するなら何もしない

        _frozen.Clear();
        var gameScene = FindGameScene(pauseSceneName);
        if (!gameScene.IsValid()) return;

        var roots = gameScene.GetRootGameObjects();
        foreach (var go in roots)
        {
            // 非アクティブも含めて拾う（true）
            var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var m in monos)
            {
                if (!m) continue;
                // 既に無効なら「無効だった」という事実だけ記録し、触らない
                if (!m.enabled)
                {
                    _frozen.Add((m, false));
                    continue;
                }
                // ポーズ中でも動かしたいものは除外したい場合、印用のインターフェイスにする
                if (m is IPauseIgnore) continue;

                m.enabled = false;
                _frozen.Add((m, true));
            }
        }
        // 参考：ここでは SetActiveScene は触らない（UIはポーズ側で操作）
        Debug.Log($"[PauseFreezer] Frozen behaviours: {_frozen.Count}");
    }

    public static void Thaw()
    {
        // 凍結前の状態に確実に戻す
        foreach (var (mb, wasEnabled) in _frozen)
        {
            if (mb) mb.enabled = wasEnabled; // 破棄済み(null)はスキップ
        }
        _frozen.Clear();
        Debug.Log("[PauseFreezer] Thawed.");
    }

    static Scene FindGameScene(string pauseSceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != pauseSceneName) return s; // PauseScene 以外をゲーム側とみなす
        }
        return SceneManager.GetActiveScene();
    }
}

/// ポーズ中も止めたくないコンポーネントに付ける印（任意）
public interface IPauseIgnore { }
