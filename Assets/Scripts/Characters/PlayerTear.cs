using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTear : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;

    // �t���O��
    private bool isActive;

    // Global Volume
    [SerializeField] private float fadePower;
    [SerializeField] private Volume postEffectVolume;
    private Vignette vignette;
    private float maxIntensity = 0.5f;
    private float targetIntensity = 0f;

    public void Initialize(PlayerController _controller)
    {
        // ���R���|�[�l���g�̎擾
        controller = _controller;

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // �j��A�J�n
        if (!isActive && controller.IsGrounded() && Input.GetButtonDown("Special"))
        {
            controller.SetDefault();
            targetIntensity = maxIntensity;
            isActive = true;
        }

        // �\���{�^���̍��E�ǂ��炩����������A���E�ǂ��炩��j��̂Ă�
        if (isActive && (Input.GetAxisRaw("Horizontal2") < 0f || Input.GetAxisRaw("Horizontal2") > 0f))
        {
            // �Y������FieldObject��j�鑀����s�����A�j���邩�ǂ�����AllFieldObjectManager���Ŕ��f����
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (Input.GetAxisRaw("Horizontal2") < 0f && fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
                else if (Input.GetAxisRaw("Horizontal2") > 0f && fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
            }

            targetIntensity = 0f;
            isActive = false;
        }

        // Global Volume
        vignette.intensity.value += (targetIntensity - vignette.intensity.value) * (fadePower * Time.deltaTime);
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
