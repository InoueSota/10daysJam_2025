using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FPSCounter : MonoBehaviour
{
     Text fpsText;

    private float deltaTime = 0.0f;
    private float minFPS = float.MaxValue;
    private float maxFPS = float.MinValue;
    private List<float> fpsHistory = new List<float>();

    private void Start()
    {
        fpsText = GetComponent<Text>();
    }
    void Update()
    {
        // FPS 計算
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // Min, Max, Average 計算
        minFPS = Mathf.Min(minFPS, fps);
        maxFPS = Mathf.Max(maxFPS, fps);
        fpsHistory.Add(fps);

        float averageFPS = 0;
        if (fpsHistory.Count > 0)
        {
            float sum = 0;
            foreach (var f in fpsHistory)
                sum += f;
            averageFPS = sum / fpsHistory.Count;
        }

        // FPS 表示更新
        fpsText.text = $"Now: {fps:000.0}fps\nMin: {minFPS:000.0}fps\nMax: {maxFPS:000.0}fps\nAvg: {averageFPS:000.0}fps\nReset:key[0]";

        // Rキーでリセット
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetFPS();
        }
    }

    void ResetFPS()
    {
        minFPS = float.MaxValue;
        maxFPS = float.MinValue;
        fpsHistory.Clear();
    }
}
