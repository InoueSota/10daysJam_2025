using UnityEngine;
using UnityEngine.UI; // TextLegacy用

public class RareTextChanger : MonoBehaviour
{
    [Header("表示するテキストコンポーネント")]
    [SerializeField] private Text targetText;

    [Header("通常の定型文")]
    [SerializeField] private string normalMessage = "こんにちは！";

    [Header("レアメッセージ候補（複数可）")]
    [SerializeField] private string[] rareMessages;

    [Header("レアメッセージが出る確率 (0～1)")]
    [Range(0f, 1f)]
    [SerializeField] private float rareChance = 0.05f; // 5%で出る

    void Awake()
    {
        ShowMessage();
    }

    /// <summary>
    /// テキストを設定する
    /// </summary>
    public void ShowMessage()
    {
        if (rareMessages.Length > 0 && Random.value < rareChance)
        {
            // レアメッセージの中からランダムに選択
            targetText.text = rareMessages[Random.Range(0, rareMessages.Length)];
        }
        else
        {
            // 通常の定型文
            targetText.text = normalMessage;
        }
    }
}
