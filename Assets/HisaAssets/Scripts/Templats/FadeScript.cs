using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeScript : MonoBehaviour
{
    //SpriteRenderer spriteRenderer;

    [SerializeField, Header("�J�ڃV�[����")] string sceneName;
    Image myrenderer;
    [SerializeField, Header("����")] float totalTime;
    [SerializeField, Header("�t�F�[�h�A�E�g(�V�[����؂�ւ���)")] bool fadeOut;

    AsyncOperation asyncLoad; // �񓯊����[�h�p

    bool fadeCompleted = false;
    float delayTime;
    public void SetSceneName(string name)
    {
        sceneName = name;

    }
    public void SetDelayTime(float value)
    {
        delayTime = value;

    }
    public void SetTotalTime(float value)
    {
        totalTime = value;
    }
    public void SetFadeOut(bool value)
    {
        fadeOut = value;
    }
    public float currentTime;
    public Color32 initColor;
    public Color32 currentColor;
    // Start is called before the first frame update
    void Start()
    {
        myrenderer = gameObject.transform.GetChild(0).GetComponent<Image>();

        //initColor = renderer.color;
        currentColor = initColor;
        if (fadeOut)
        {
            currentColor.a = 0;
        }
        else
        {
            currentColor.a = 255;

        }

        if (fadeOut && !string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(DelayLoadStart());
        }
        currentTime = 0;
        myrenderer.color = currentColor;
    }

    // Update is called once per frame
    void Update()
    {
        delayTime -= Time.unscaledDeltaTime;
        if (delayTime > 0) { return; }
        currentTime += Time.unscaledDeltaTime;
        if (fadeOut)
        {
            currentColor.a = (byte)Easing.InSine(currentTime, totalTime, 0f, 255f);
        }
        else
        {
            currentColor.a = (byte)Easing.InSine(currentTime, totalTime, 255f, 0f);

        }
        myrenderer.color = currentColor;
        if (currentTime > totalTime)
        {
            if (fadeOut && asyncLoad != null && asyncLoad.progress >= 0.9f && !fadeCompleted)
            {
                asyncLoad.allowSceneActivation = true;
                fadeCompleted = true;
            }
            if (!fadeOut && currentTime > totalTime + 0.5f) Destroy(this.gameObject);
        }

    }

    IEnumerator LoadSceneAsync(string scene)
    {
        asyncLoad = SceneManager.LoadSceneAsync(scene);
        asyncLoad.allowSceneActivation = false;
        yield return null;
    }
    IEnumerator DelayLoadStart()
    {
        System.GC.Collect();               // �K�v�Ȃ�GC
        yield return null;                // �������̌��
        yield return new WaitForSeconds(0.3f); // �t�F�[�h���n�܂��Ă��班���҂�
        StartCoroutine(LoadSceneAsync(sceneName));
    }
}
