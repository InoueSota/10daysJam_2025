using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StageCell;

public class StageSelectManager : MonoBehaviour
{

    //public StageCell curSelectStage;
    public Vector2 inputDire = Vector2.zero;
    //[SerializeField] TargetFollow2DScript cameraFollow;
    [SerializeField] AreaManager[] areaManagers;
    [SerializeField] Transform areaPixelCameraTransform;

    bool stageChangeFlag;
    float stageChangeCT = 0.5f;//�X�e�[�W�J�ڂ��󂯕t����܂ł̎��ԁB�Z������ƁA�A�ł��Ȃ���V�[���J�ڂ������Ƀo�O��\����
    float curStageChangeCT;
    public float inputCoolTime;
    [SerializeField] SmoothDampRotate areaPixelCamera;

    [SerializeField, Header("�X�e�[�W�A�G���A�I���̃A�j���[�V����")] Animator[] selectAnime;

    // [SerializeField] SpriteRenderer curVisualStageImage;
    //[SerializeField] AmpritudePosition imageAmpritude;

    int curSelectAreaIndex;
    int preSelectAreaIndex = -1;

    bool areaSelect;


    bool debugActive;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        areaSelect = true;


        SaveData save = SaveSystem.Load(1) ?? new SaveData();//�Z�[�u���������ޏ���
        SaveUtil.SetCleared(save, "Area1", "Area1Stage1", ClearDirection.Right, true);//�G���A1�̃X�e�[�W1���E�����ɃN���A����
        SaveSystem.Save(save, 1);//�Z�[�u
    }

    // Update is called once per frame
    void Update()
    {
        ChangeScene();
        InputDire();

        if (areaSelect)
        {
            AreaSelect();

        }
        else
        {
            StageSelect();
        }

        //ChangeCell();

#if UNITY_EDITOR
        DebugUpdate();
#endif

    }

    void InputDire()
    {
        inputDire = Vector2.zero;

        if (inputCoolTime > 0)
        {
            inputCoolTime -= Time.deltaTime;
            return;
        }

        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");
        if (inputDire.magnitude > 0)
        {
            inputCoolTime = 0.3f;
        }
    }

    void AreaSelect()
    {
        if (inputDire.x > 0)
        {
            curSelectAreaIndex++;

        }
        else if (inputDire.x < 0)
        {
            curSelectAreaIndex--;
        }

        if (curSelectAreaIndex >= areaManagers.Length)
        {
            curSelectAreaIndex = 0;
        }
        else if (curSelectAreaIndex < 0)
        {
            curSelectAreaIndex = areaManagers.Length - 1;
        }




        //�G���A��؂�ւ�����
        if (preSelectAreaIndex != curSelectAreaIndex)
        {
            if (preSelectAreaIndex >= 0 && preSelectAreaIndex < areaManagers.Length) areaManagers[preSelectAreaIndex].AreaSelectAnime("BackAreaSelect");//�O�̃A�j���[�V������Stop��Ԃɂ���
            areaManagers[curSelectAreaIndex].AreaSelectAnime("ChangeArea");//���̃A�j���[�V�����͍Đ�����

            areaPixelCamera.StartRotation(90f * curSelectAreaIndex);

            preSelectAreaIndex = curSelectAreaIndex;
            areaManagers[curSelectAreaIndex].ClearEffect();
        }

        if (Input.GetButtonDown("Select"))
        {
            areaSelect = false;
            areaManagers[curSelectAreaIndex].AreaSelectAnime(true);
            areaManagers[curSelectAreaIndex].SetSelectActive(true);
        }

    }

    void StageSelect()
    {
        if (Input.GetButtonDown("Back"))
        {
            areaSelect = true;
            areaManagers[curSelectAreaIndex].AreaSelectAnime(false);
            areaManagers[curSelectAreaIndex].SetSelectActive(false);
        }
        areaManagers[curSelectAreaIndex].ChangeCell(inputDire);
    }



    void ChangeScene()
    {
        if (stageChangeFlag) { return; }

        if (curStageChangeCT < 0.5f)
        {
            curStageChangeCT += Time.deltaTime;
            return;
        }

        //�V�[���J��
        //�X�e�[�W�ɓ��鎞
        if (!areaSelect)
        {
            if (Input.GetButtonDown("Select"))
            {
                Debug.Log("�Z���N�g");

                SceneManager.LoadScene(areaManagers[curSelectAreaIndex].GetCellStageName());
                stageChangeFlag = true;
            }
        }
        if (Input.GetButtonDown("Back"))
        {
            stageChangeFlag = true;
            Debug.Log("�o�b�N");
        }

    }

    [ContextMenu("�G���A�Z���N�g")]
    public void AreaSelectAnime()
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", false);
        }
    }

    [ContextMenu("�X�e�[�W�Z���N�g")]
    public void StageSelectAnime()
    {
        for (int i = 0; i < selectAnime.Length; i++)
        {
            selectAnime[i].SetBool("StageSelect", true);
        }
    }

    void DebugUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            debugActive = !debugActive;
            for (int i = 0; i < selectAnime.Length; i++)
            {
                selectAnime[i].SetBool("StageSelect", debugActive);
            }
        }
    }

    [ContextMenu("�Z�[�u�폜")]
    void SaveDelete()
    {
        SaveSystem.Delete(1);

    }

    [ContextMenu("StageDateReset")]
    public void StageDateReset()
    {
        var g = GameBootstrap.Graph as EditableJsonStageGraph;
        if (g == null) { Debug.LogError("EditableJsonStageGraph ���������� or �^�Ⴂ�ł�"); return; }

        g.BeginCapture();

        g.SaveOverrideDelta();
        g.EndCapture();
    }
}
