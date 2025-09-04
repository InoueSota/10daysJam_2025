using UnityEngine;

public class HSVColorChanger : MonoBehaviour
{
    [SerializeField] Renderer objectRenderer;
    //public float speed = 0.5f; // グラデーションの速度
    // private float hue;

    void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
    }

   

    void Update()
    {
        // HSVのH値を時間とともに変化させる
        //hue = (Time.time * speed) % 1.0f;


    }

    //h=色相　s=彩度　v=明度
    // HSVからRGBに変換して色を変更
    public void SetHSVColor(float h, float s, float v)
    {
        Color color = Color.HSVToRGB(h, s, v);

        if (objectRenderer != null)
        {
            objectRenderer.material.color = color;
        }
    }
}
