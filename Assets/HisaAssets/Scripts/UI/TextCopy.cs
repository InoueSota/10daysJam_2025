using UnityEngine;
using UnityEngine.UI; // Text(Legacy) 用

public class TextCopy : MonoBehaviour
{
    [Header("コピー元のテキスト")]
    [SerializeField] private Text sourceText;

    [Header("コピー先のテキスト")]
    [SerializeField] private Text targetText;

    [Header("常にコピーし続けるか？")]
    [SerializeField] private bool updateEveryFrame = false;

    void Start()
    {
        // 初回にコピー
        CopyText();
    }

    void Update()
    {
        if (updateEveryFrame)
        {
            CopyText();
        }
    }

    /// <summary>
    /// sourceText の内容を targetText にコピー
    /// </summary>
    public void CopyText()
    {
        if (sourceText != null && targetText != null)
        {
            targetText.text = sourceText.text;
        }
    }
}
