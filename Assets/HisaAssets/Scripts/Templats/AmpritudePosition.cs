using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmpritudePosition : MonoBehaviour
{
    [Header("�h��ʁE����")]
    [SerializeField] float ampritude = 1f;   // �U���i�P��: �����j
    [SerializeField] float period = 0.2f;    // �h��̎����i�b�j
    [SerializeField] float easeTime = 0.5f;  // ���Đ����ԁi�b�j
    float easeT;
    public bool startEasing;

    [Header("�����ݒ�")]
    [SerializeField] bool onlyY = true;           // Y �̂ݗh�炷
    [SerializeField] Vector3 direction = Vector3.up; // onlyY=false �̂Ƃ��Ɏg���ړ�����

    [Header("���W/���Ԃ̊")]
    [SerializeField] bool useLocalPosition = true; // ���[�J�����W�œ�������
    [SerializeField] bool unscaledTime = false;    // �X���[���[�V�����̉e�����󂯂Ȃ�

    Vector3 initPos;

    void Start()
    {
        easeT = 0f;
        initPos = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        if (!startEasing) return;

        // ���ԉ��Z
        easeT += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // 0��1 �ɐ��K��
        float u = Mathf.Clamp01(easeT / Mathf.Max(0.0001f, easeTime));

        // �����G���x���[�v�iEaseOut�j�F1 �� 0 �փX���[�Y��
        float envelope = 1f - Mathf.SmoothStep(0f, 1f, u);

        // �����g
        float s = Mathf.Sin(2f * Mathf.PI * (easeT / Mathf.Max(0.0001f, period)));

        // �U�� �~ ���� �~ ����
        float offsetMag = ampritude * s * envelope;

        // ��������
        Vector3 dir = onlyY ? Vector3.up :
                      (direction.sqrMagnitude < 1e-6f ? Vector3.up : direction.normalized);

        // �ʒu�X�V
        Vector3 targetPos = initPos + dir * offsetMag;
        if (useLocalPosition) transform.localPosition = targetPos;
        else transform.position = targetPos;

        // �I������
        if (easeT >= easeTime)
        {
            startEasing = false;
            easeT = 0f;
            if (useLocalPosition) transform.localPosition = initPos;
            else transform.position = initPos;
        }
    }

    public void EaseStop()
    {
        easeT = 0f;
        startEasing = false;
        if (useLocalPosition) transform.localPosition = initPos;
        else transform.position = initPos;
    }

    [ContextMenu("start")]
    public void EaseStart()
    {
        // ���ɋN�����Ȃ烊�X�^�[�g
        if (startEasing) easeT = 0f;
        startEasing = true;

        // ��ʒu����蒼�������ꍇ�͈ȉ���L����
        // initPos = useLocalPosition ? transform.localPosition : transform.position;
    }
}
