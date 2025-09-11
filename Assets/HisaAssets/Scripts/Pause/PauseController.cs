using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [Header("ゲーム側と区別するための名前")]
    [SerializeField] string pauseSceneName = "PauseScene";
    [SerializeField] string selectSceneName = "StageSelect";

    [SerializeField] SetTextScript debugText;
    [SerializeField] int index;
    public bool change;
    public Vector2 inputDire;
    float inputCoolTime;
    bool select;

    float inputDelay;
    bool isResume;
    public float resumeTime;

    [SerializeField] Animator[] animators;
    [SerializeField] Animator canvasAnime;
    [SerializeField] SceneTransition sceneTransitionPrefab;
    SceneTransition sceneTransition;
    [SerializeField] private PauseToggle pauseToggle;


    public bool isTechniquMenu;
    [SerializeField] Animator TechniqueCanvas;

    [SerializeField] Image hintImage;
    int hintImageIndex;
    int preHintImageIndex;
    [SerializeField]
    GameObject[] hintImageBack;
    [SerializeField]
    SetTextScript[] levelText;
    [SerializeField]
    AmpritudePosition[] ampritudePos;
    [SerializeField] AudioPlay audioplay;

    [SerializeField] SetTextScript sceNameText;
    void Start()
    {
        // PauseScene が Additive でロードされた時点で呼ばれる
        Time.timeScale = 0f;
        // PauseFreezer.Freeze(pauseSceneName, strict: true);
        animators[0].SetBool("Select", true);
        if(GameManager.hintImageStatic[0]!=null) hintImage.sprite = GameManager.hintImageStatic[0];
        hintImageBack[0].SetActive(true);
        sceNameText.SetText(StageCell.lastSelectSellName);
    }

    private void Update()
    {
        if (inputDelay < 0.8f)
        {
            inputDelay += Time.unscaledDeltaTime;
            return; 
        }

        if (isResume) {

            resumeTime += Time.unscaledDeltaTime;

            if (resumeTime > 0.533f)
            {
                OnResume();
            }
            return;


        }

        InputDire();
        //debugText.SetText(index);

        if (change) {return; }

        PauseMenuUpdate();
        TechniqueMenuUpdate();



    }

    // === ボタンから呼ぶ関数 ===

    // 「ゲームに戻る」
    [ContextMenu("ゲームに戻る")]

    public void OnResume()
    {
        PauseFreezer.Thaw();
        Time.timeScale = 1f;

        // PauseScene を閉じる
        var s = SceneManager.GetSceneByName(pauseSceneName);
        if (s.IsValid() && s.isLoaded) SceneManager.UnloadSceneAsync(s);
    }

    // 「ステージをリスタート」
    [ContextMenu("ステージをリスタート")]

    public void OnRestart()
    {
        PauseFreezer.Thaw();
        Time.timeScale = 1f;

        var game = FindGameScene();
        if (game.IsValid())
        {
            sceneTransition = Instantiate(sceneTransitionPrefab);
            sceneTransition.StartTransition(game.name);
            //SceneManager.LoadScene(game.name, LoadSceneMode.Single);
        }
    }

    // 「セレクト画面に戻る」
    [ContextMenu("セレクト画面に戻る")]
    public void OnGoSelect()
    {
        //PauseFreezer.Thaw();
        Time.timeScale = 1f;
        sceneTransition = Instantiate(sceneTransitionPrefab);
        sceneTransition.StartTransition(selectSceneName);
        //SceneManager.LoadScene(selectSceneName, LoadSceneMode.Single);
    }

    // === ゲームシーンを見つけるヘルパー ===
    Scene FindGameScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != pauseSceneName) return s;
        }
        return SceneManager.GetActiveScene();
    }

    void SelectMode()
    {
        if (change) { return; }

        if (Input.GetButtonDown("Select"))
        {
            audioplay.SE1();
            change = true;
            //ゲームに戻る
            if (index == 0)
            {
                isResume = true;
                canvasAnime.SetTrigger("PauseOut");
            }
            //ステージをリセットする
            else if (index == 1)
            {
                OnRestart();
            }
            else if (index == 2)
            {
                Debug.Log("わざへ");
                isTechniquMenu = true;
                TechniqueCanvas.SetTrigger("Stay");
                change = false;
            }
            //セレクトへ戻る
            else if (index == 3)
            {
                Debug.Log("セレクト画面に戻る");
                OnGoSelect();
            }
        }

        if (!change && Input.GetButtonDown("Menu")|| !change && Input.GetButtonDown("Back") )
        {
            audioplay.SE1();
            change = true;
            //ゲームに戻る
            isResume = true;
            canvasAnime.SetTrigger("PauseOut");
            // OnResume();
        }
    }

    void InputDire()
    {
        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");

        if (inputCoolTime > 0)
        {
            inputCoolTime -= Time.unscaledDeltaTime;


            //ボタン連打で動けるようにする
            if (inputDire.magnitude <= 0)
            {
                inputCoolTime = 0;

            }
            inputDire = Vector2.zero;
            return;
        }

        if (inputDire.magnitude > 0)
        {
            inputCoolTime = 0.3f;
        }


    }

    void PauseMenuUpdate() {
        if(isTechniquMenu) {return;}
        if (inputDire.y < 0)
        {
            animators[index].SetBool("Select", false);
            index++;

            if (index >= 4) { index = 0; }
            animators[index].SetBool("Select", true);
            audioplay.SE2();
        }
        else if (inputDire.y > 0)
        {
            animators[index].SetBool("Select", false);

            index--;
            if (index < 0) { index = 3; }
            animators[index].SetBool("Select", true);
            audioplay.SE2();
        }
        SelectMode();
    }

    void TechniqueMenuUpdate()
    {
        if (!isTechniquMenu) { return; }

        if (Input.GetButtonDown("Menu")|| Input.GetButtonDown("Back"))
        {
            isTechniquMenu = false;
            TechniqueCanvas.SetTrigger("End");
            inputDelay = 0.5f;
            audioplay.SE1();
        }

        if (inputDire.x > 0)
        {
            hintImageIndex++;
        }else if(inputDire.x < 0)
        {
            hintImageIndex--;
        }


        hintImageIndex = (int)Mathf.Clamp(hintImageIndex, 0, 2);

        if (preHintImageIndex!=hintImageIndex)
        {
            hintImageBack[preHintImageIndex].SetActive(false);
            hintImageBack[hintImageIndex].SetActive(true);
            levelText[0].SetText("レベル" + (hintImageIndex + 1));
            levelText[1].SetText("Next:" + "レベル"+ (hintImageIndex + 2));
            if (hintImageIndex == 0)
            {
                levelText[1].SetText("一手目");
            }
            else if (hintImageIndex == 1)
            {
                levelText[1].SetText("中盤");
            }
            else if (hintImageIndex == 2)
            {
                levelText[1].SetText("ゴール直前");
            }

            for (int i = 0; i < ampritudePos.Length; i++)
            {
                ampritudePos[i].EaseStart();
            }

            preHintImageIndex = hintImageIndex;

            audioplay.SE2();
            if (GameManager.hintImageStatic[hintImageIndex] != null) hintImage.sprite = GameManager.hintImageStatic[hintImageIndex];
        }
    }
}
