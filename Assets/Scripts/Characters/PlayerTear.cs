using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTear : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;

    // ���R���|�[�l���g
    private Transform gridTransform;

    // �t���O��
    private bool isActive;
    private bool isReleaseStick;

    // Global Volume
    [SerializeField] private float fadePower;
    [SerializeField] private Volume postEffectVolume;
    private Vignette vignette;
    private float maxIntensity = 0.5f;
    private float targetIntensity = 0f;

    public void Initialize(PlayerController _controller)
    {
        isReleaseStick = true;

        // ���R���|�[�l���g�̎擾
        controller = _controller;

        // ���R���|�[�l���g�̎擾
        gridTransform = GameObject.FindGameObjectWithTag("Grid").transform;

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // �j��A�J�n
        if (!isActive && controller.IsGrounded() && Input.GetButtonDown("Special"))
        {
            if (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f)
            {
                isReleaseStick = false;
            }
            controller.SetDefault();
            targetIntensity = maxIntensity;
            isActive = true;
        }
        // �j��A�I��
        else if (isActive && Input.GetButtonDown("Special"))
        {
            controller.SetBackToNormal();
            targetIntensity = 0f;
            isActive = false;
        }

        // �w����x�b�����鏈��
        if (isActive && !isReleaseStick && Input.GetAxisRaw("Horizontal") == 0f) { isReleaseStick = true; }

        // �\���{�^���̍��E�ǂ��炩����������A���E�ǂ��炩��j��̂Ă�
        if (isActive && isReleaseStick && (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f))
        {
            // �Y������FieldObject��j�鑀����s�����A�j���邩�ǂ�����AllFieldObjectManager���Ŕ��f����
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (Input.GetAxisRaw("Horizontal") < 0f && fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
                else if (Input.GetAxisRaw("Horizontal") > 0f && fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
            }

            // �Y�����郌�C���[�ɔj������^����
            gridTransform.GetChild(1).GetComponent<PageManager>().SetTearInfomation(new(Mathf.RoundToInt(transform.position.x), 0f, 0f), new(Input.GetAxisRaw("Horizontal"), 0f, 0f));

            // �j��A�I��
            controller.SetBackToNormal();
            targetIntensity = 0f;
            isActive = false;
        }

        // Global Volume
        vignette.intensity.value += (targetIntensity - vignette.intensity.value) * (fadePower * Time.deltaTime);
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
