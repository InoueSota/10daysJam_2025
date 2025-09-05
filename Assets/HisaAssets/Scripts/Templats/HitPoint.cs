using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class HitPoint : MonoBehaviour
{
    public float maxHealth = 100f;
    public float curHelth;
    float curGaugeRatio;

    //bool curFrameHited;  // ����frame�ōU�����󂯂���

    private bool isDisplayDamageUI;

    [SerializeField, Range(0f, 1f)] List<float> divisionRatio = new List<float> { 1f };

    [SerializeField] GaugeScript hitPointGauge;
    [SerializeField] GaugeScript hitPointDiffGauge;
    [SerializeField] float diffGaugeSubTime;//�_���[�W�̍���������܂ł̎���
    float curDiffTime;
    float diffGaugeChangeTime = 0.7f;//�����̃Q�[�W����������
    float curdiffGaugeChangeTime;
    float diffHealth;//�_���[�W���󂯂�O��HP��ۑ�����p
    [SerializeField] GameObject[] divisionUIs;
    int curDivision;
    int preDivision;
    [SerializeField, Header("HP��0�ɂȂ������I�������I�u�W�F�N�g��Destroy����")]
    public List<GameObject> selectDestroyObj;
    [SerializeField] ShakeScript targetShake;
    [SerializeField] AmpritudeScale amplitude;
    [SerializeField] GameObject damageeffect;
    float damageEffectCooltime;

    //[SerializeField] TowerScript towarScript;
    public float SetGetHelth
    {
        set
        {
            maxHealth = value;
            curHelth = maxHealth;
        }
        get { return curHelth; }
    }

    float GetDivisionRaito()
    {
        for (int i = divisionRatio.Count - 1; i >= 0; i--)
        {
            if (curHelth <= maxHealth * divisionRatio[i])
            {
                curDivision = i;
                break;
            }
        }
        if (curDivision >= divisionRatio.Count - 1)
        {
            return curHelth / (maxHealth * divisionRatio[curDivision]);
        }
        //�P��ƂP�O�̍��������݂̊���
        float curDivisionRatio = divisionRatio[curDivision] - divisionRatio[curDivision + 1];

        float diffRatio = maxHealth * divisionRatio[curDivision + 1];
        return (curHelth - diffRatio) / (maxHealth * curDivisionRatio);

    }
    //�ʃN���X��divisionRatio��ݒ�o����悤�ɂ���B
    public void SetDivisionRatio(float[] source)
    {
        divisionRatio.Clear();

        for (int i = 0; i < source.Length; i++)
        {
            divisionRatio.Add(source[i]);
        }
    }
    float GetCurDivisionMaxRatio()
    {
        int division = 0;

        for (int i = divisionRatio.Count - 1; i >= 0; i--)
        {
            if (curHelth <= maxHealth * divisionRatio[i])
            {
                division = i;
                break;
            }
        }
        if (division >= divisionRatio.Count - 1)
        {
            return (maxHealth * divisionRatio[division]);
        }
        //�P��ƂP�O�̍��������݂̊���
        float curDivisionRatio = divisionRatio[division] - divisionRatio[division + 1];

        return (maxHealth * curDivisionRatio);
    }

    private void Start()
    {
        isDisplayDamageUI = true;
        //curFrameHited = false;
        curHelth = maxHealth;
        if (selectDestroyObj == null)
        {
            //selectDestroyObj.Add(this.gameObject);
        }
        if (hitPointGauge != null) hitPointGauge.SetRatio(GetDivisionRaito());
    }

    private void Update()
    {
        damageEffectCooltime -= Time.unscaledDeltaTime;
        DiffGauge();
        if (divisionUIs != null)
        {

            if (curDivision <= divisionUIs.Length && curDivision != preDivision)
            {
                divisionUIs[preDivision].SetActive(false);
            }
            preDivision = curDivision;


        }
    }

    [ContextMenu("10�_���[�W��^����")]
    void DebugDamage()
    {
        TakeDamage(1);
    }

    public void TakeDamage(float damage)
    {
        //curFrameHited = true;

        //�ҋ@�J�n�O�ɂɕω��O��HP��ۑ����Ă���
        if (curDiffTime <= 0 && curdiffGaugeChangeTime <= 0)
        {
            diffHealth = GetDivisionRaito();
            curDiffTime = diffGaugeSubTime;
            curdiffGaugeChangeTime = diffGaugeChangeTime;
        }
        curHelth -= damage;
        //Debug.Log($"{gameObject.name} took {damage} damage. Remaining HP: {curHelth}");
        if (hitPointGauge != null)
        {
            hitPointGauge.SetRatio(GetDivisionRaito());
        }

        if (isDisplayDamageUI)
        {
            //PopDamageUI(damage);
        }

        Die();
        if (targetShake != null)
        {
            targetShake.ShakeStart();

        }
        if (amplitude != null)
        {
            amplitude.EaseStart();

        }
        if (damageEffectCooltime <= 0 && damageeffect != null)
        {
            damageEffectCooltime = 0.1f;
            Instantiate(damageeffect, transform.position, Quaternion.identity);
        }
    }
    void DiffGauge()
    {
        if (hitPointDiffGauge == null) { return; }
        if (curdiffGaugeChangeTime <= 0) { return; }
        if (curDiffTime > 0)
        {
            curDiffTime -= Time.deltaTime;

            return;
        }

        curdiffGaugeChangeTime -= Time.deltaTime;
        float curRaito = Easing.OutQuad(curdiffGaugeChangeTime, diffGaugeChangeTime, GetDivisionRaito(), diffHealth);


        hitPointDiffGauge.SetRatio(curRaito);

    }
    public bool Die()
    {
        if (curHelth <= 0)
        {
            Debug.Log($"{transform.name} died.");
            foreach (GameObject obj in selectDestroyObj)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            // ���X�g���N���A���Ă����i�C�Ӂj
            selectDestroyObj.Clear();
            return true;


        }


        return false;
    }



    ///// <summary>
    ///// �|�b�vUI�̏���
    ///// </summary>
    //private void PopDamageUI(float value) {
    //	if (owner == null || damageText == null) {
    //		Debug.Log("ower or text is Null");
    //		return;
    //	}

    //	// UI���쐬
    //	var ui = Instantiate(damageText);

    //	// ���ꂼ��̐e��ύX
    //	GameObject parentObj = GameObject.Find("DamageUICanvas");
    //	ui.transform.SetParent(parentObj.transform);

    //	// screen��̍��W�ɕύX
    //	Vector3 screenPos = Camera.main.WorldToScreenPoint(owner.transform.position);
    //	Vector2 localPos;
    //	Vector2 offset = new Vector2(Random.Range(-30.0f, 30.0f), Random.Range(-30.0f, 30.0f));
    //	RectTransform canvasRect = parentObj.GetComponent<Canvas>().GetComponent<RectTransform>();
    //	if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos,
    //		parentObj.GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
    //		out localPos)) {
    //		ui.GetComponent<RectTransform>().anchoredPosition = localPos + offset;
    //	}

    //	DamageUI damageUIScript = ui.GetComponent<DamageUI>();
    //	if(damageUIScript != null) {
    //		damageUIScript.SetTarget(owner.transform.position);
    //		damageUIScript.SetOffset(offset);
    //	}

    //	// Text�̕�����ݒ肷��
    //	ui.SetText(value, "f0");

    //	// �傫���𒲐�
    //	float scaleRaito = value / maxHealth;
    //	scaleRaito += 1.0f;
    //	ui.GetComponent<RectTransform>().localScale *= scaleRaito;

    //	// �F�ύX����
    //	if (value >= 30) {
    //		ui.SetColor(Color.yellow);
    //	} else {
    //		ui.SetColor(Color.black);
    //	}
    //}

    public void SetIsDisplayDamageUI(bool flag)
    {
        isDisplayDamageUI = flag;
    }
}
