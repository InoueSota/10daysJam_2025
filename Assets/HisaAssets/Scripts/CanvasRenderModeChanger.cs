using UnityEngine;

public class CanvasRenderModeChanger : MonoBehaviour
{

    float time;
    bool change;
    void Start()
    {
       
    }

    void Update()
    {
        if (!change)
        {
            time += Time.deltaTime;

            if (time > 0.1f)
            {
                change = true;
                // アタッチされているCanvasを取得
                Canvas canvas = GetComponent<Canvas>();

                if (canvas != null)
                {
                    // レンダーモードをスクリーンスペースオーバーレイに設定
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    Debug.Log("Canvas render mode set to Screen Space - Overlay");
                }
                else
                {
                    Debug.LogWarning("Canvas component not found!");
                }
            }
        }
    }
}
