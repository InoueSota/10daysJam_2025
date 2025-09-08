using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class AudioFader : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 2.0f; // フェード時間（秒）
    float initVolume;

    public AudioSource BGM1;
   
    void Start()
    {
        if ( audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.Play();
       
        initVolume = audioSource.volume;
        FadeIn();
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, initVolume, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = initVolume;
    }

    IEnumerator FadeOutCoroutine()
    {
        float startVolume = audioSource.volume;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}
