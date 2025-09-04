using UnityEngine;
using UnityEngine.UI;

public class GaugeScript : MonoBehaviour
{
    Image gaugeUI;
    public void SetRatio(float ratio)
    {
        float curScale = Mathf.Clamp01(ratio);
        gaugeUI.fillAmount = curScale;

    }

    public void SetColor(Color set)
    {
        gaugeUI.color = set;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gaugeUI = GetComponent<Image>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
