using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] Text targetText;       // Legacy UI.Text
    [TextArea][SerializeField] string fullText; // ï\é¶ÇµÇΩÇ¢ï∂èÕ
    [SerializeField] float interval = 0.1f; // ï∂éöëóÇËä‘äuÅiïbÅj

    private float timer = 0f;
    private int currentIndex = 0;
    public void SetText(string text)
    {
        fullText = text;
    }
    void Start()
    {
        targetText.text = "";
    }
    void Update()
    {
        if (currentIndex < fullText.Length)
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                timer = 0f;
                currentIndex++;
                targetText.text = fullText.Substring(0, currentIndex);
            }
        }
    }
}
