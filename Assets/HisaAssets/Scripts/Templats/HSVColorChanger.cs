using UnityEngine;

public class HSVColorChanger : MonoBehaviour
{
    [SerializeField] Renderer objectRenderer;
    //public float speed = 0.5f; // �O���f�[�V�����̑��x
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
        // HSV��H�l�����ԂƂƂ��ɕω�������
        //hue = (Time.time * speed) % 1.0f;


    }

    //h=�F���@s=�ʓx�@v=���x
    // HSV����RGB�ɕϊ����ĐF��ύX
    public void SetHSVColor(float h, float s, float v)
    {
        Color color = Color.HSVToRGB(h, s, v);

        if (objectRenderer != null)
        {
            objectRenderer.material.color = color;
        }
    }
}
