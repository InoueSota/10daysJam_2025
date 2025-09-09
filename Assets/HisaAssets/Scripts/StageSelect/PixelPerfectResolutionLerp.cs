using UnityEngine;
using UnityEngine.U2D; // PixelPerfectCamera

public class PixelPerfectResolutionLerp : MonoBehaviour
{
    [SerializeField] PixelPerfectCamera ppc;
    [SerializeField] int startX = 320;     // 開始解像度 X
    [SerializeField] int startY = 180;     // 開始解像度 Y
    [SerializeField] int goalX = 640;      // ゴール解像度 X
    [SerializeField] int goalY = 360;      // ゴール解像度 Y
    [SerializeField] float duration = 2f;  // 遷移時間(秒)

    bool isRunning = false;

    void Start()
    {
        if (ppc == null) ppc = GetComponent<PixelPerfectCamera>();
        StartCoroutine(ResolutionLerp());
    }

    System.Collections.IEnumerator ResolutionLerp()
    {
        if (isRunning) yield break;
        isRunning = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float rate = Mathf.Clamp01(t / duration);

            int curX = Mathf.RoundToInt(Mathf.Lerp(startX, goalX, rate));
            int curY = Mathf.RoundToInt(Mathf.Lerp(startY, goalY, rate));

            ppc.refResolutionX = curX;
            ppc.refResolutionY = curY;

            yield return null;
        }

        // 最終的にゴール値をセット
        ppc.refResolutionX = goalX;
        ppc.refResolutionY = goalY;

        isRunning = false;
    }
}
