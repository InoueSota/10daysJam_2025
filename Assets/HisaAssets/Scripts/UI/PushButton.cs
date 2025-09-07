using UnityEngine;
using UnityEngine.UI;

public class PushButton : MonoBehaviour
{
    [SerializeField] GameObject releaseUI;
    [SerializeField] GameObject pushUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    public void Push()
    {
        releaseUI.SetActive(false);
        pushUI.SetActive(true);
    }
    public void Release()
    {
        releaseUI.SetActive(true);
        pushUI.SetActive(false);
    }
}
