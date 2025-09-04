using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SetTextScript : MonoBehaviour
{
    [SerializeField, Header("êîéöÇÃëOÇ…í«â¡Ç∑ÇÈåæót")] public string frontString;
    [SerializeField, Header("êîéöÇÃå„Ç…í«â¡Ç∑ÇÈåæót")] public string backString;

    Text m_Text;
    // Start is called before the first frame update
    void Awake()
    {
        m_Text = GetComponent<Text>();
    }

    public void SetText(string text)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        //float raitoNum = raito * 100;
        m_Text.text = frontString + text + backString;
    }
    public void SetText(int raito)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        //float raitoNum = raito * 100;
        m_Text.text = frontString + raito.ToString() + backString;
    }
    public void SetText(float raito)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        //float raitoNum = raito * 100;
        m_Text.text = frontString + raito.ToString("f3") + backString;
    }
    public void SetText(float value, string format)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        //float raitoNum = raito * 100;
        m_Text.text = frontString + value.ToString(format) + backString;

    }

    public void CountText(float value)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();

        float result = Mathf.Floor(value); // 3.0
        m_Text.text = frontString + result.ToString() + backString;

    }
    public void TimerText(float value)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        float result = 0;
        if (value > 1)
        {
            result = Mathf.Floor(value); // 3.0
            m_Text.text = frontString + result.ToString() + backString;
        }
        else
        {
            string resultText = value.ToString("0.#");
            if (resultText.StartsWith("0."))
                resultText = resultText.Substring(1); // ".25"
            m_Text.text = frontString + resultText + backString;
        }


    }

    public void TimerMinSec(float time)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        m_Text.text = frontString + string.Format("{0:00}:{1:00}", minutes, seconds) + backString;
    }

    public void SetColor(Color color)
    {
        if (m_Text == null) m_Text = GetComponent<Text>();
        m_Text.color = color;

    }
    // Update is called once per frame
    void Update()
    {

    }
}
