using UnityEngine;

public class PlayerTear : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;

    // �t���O��
    private bool isActive;

    public void Initialize(PlayerController _controller)
    {
        // ���R���|�[�l���g�̎擾
        controller = _controller;
    }

    void Update()
    {
        // �j��A�J�n
        if (!isActive && controller.IsGrounded() && Input.GetButtonDown("Special"))
        {
            controller.SetDefault();
            isActive = true;
        }

        // �\���{�^���̍��E�ǂ��炩����������A���E�ǂ��炩��j��̂Ă�
        if (isActive && (Input.GetAxis("Horizontal2") < 0f || Input.GetAxis("Horizontal2") > 0f))
        {
            // �Y������FieldObject��j�鑀����s�����A�j���邩�ǂ�����AllFieldObjectManager���Ŕ��f����
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (Input.GetAxis("Horizontal2") < 0f && fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
                else if (Input.GetAxis("Horizontal2") > 0f && fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
            }

            isActive = false;
        }
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
