using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticURPControl : MonoBehaviour
{
    [SerializeField] Volume globalVolume; // Global Volume ���C���X�y�N�^�[����A�^�b�`

    ChromaticAberration chromatic;

    [SerializeField] float easeTime;
    public float curEaseTime;
    [SerializeField] float maxIntensity;

    bool isChromatic;

    public void SetIsChromatic(bool value) { isChromatic = value; }

    void Start()
    {
        // Volume ���� ChromaticAberration ���擾
        if (globalVolume.profile.TryGet<ChromaticAberration>(out chromatic))
        {
            chromatic.intensity.overrideState = true; // �l�̏㏑����L���ɂ���
            chromatic.intensity.value = 0f;
        }
    }

    void Update()
    {
        if (chromatic == null) { return; }
        if (isChromatic)
        {
            curEaseTime += Time.unscaledDeltaTime;
        }
        else
        {
            curEaseTime -= Time.unscaledDeltaTime;

        }
        curEaseTime=Mathf.Clamp(curEaseTime, 0f, easeTime);
        chromatic.intensity.value = Easing.OutSine(curEaseTime, easeTime, 0f, 1f);

    }
}
