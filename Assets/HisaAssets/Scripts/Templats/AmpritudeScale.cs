using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmpritudeScale : MonoBehaviour
{
    [SerializeField] float ampritude;
    [SerializeField] float period;
    [SerializeField] float easeTime;
    float easeT;
    public bool startEasing;

    Vector3 initScale;
    [SerializeField] bool onlyY;
    [SerializeField] bool unscaledTime;//�X���[���[�V�������o�ɉe�����Ȃ��悤�ɂ���
    // Start is called before the first frame update
    void Start()
    {
        easeT = 0;
        initScale = transform.localScale;
        //initScale = new Vector3(1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!startEasing) { return; }
        if (unscaledTime)
        {
            easeT += Time.unscaledDeltaTime;
        }
        else
        {
            easeT += Time.deltaTime;
        }
        if (onlyY)
        {
            this.transform.localScale = Easing.EaseAmplitudeScaleY(initScale, easeT, easeTime, ampritude, period);

        }
        else
        {
            this.transform.localScale = Easing.EaseAmplitudeScale(initScale, easeT, easeTime, ampritude, period);

        }
        if (easeT > easeTime)
        {
            startEasing = false;
            easeT = 0;
        }
    }

    public void EaseStop()
    {
        easeT = 0;
        startEasing = false;
        this.transform.localScale = initScale;
    }
    [ContextMenu("start")]
    public void EaseStart()
    {
        //���ɋN�����Ă��烊�X�^�[�g����
        if (startEasing)
        {
            easeT = 0;
        }


        startEasing = true;
    }

}
