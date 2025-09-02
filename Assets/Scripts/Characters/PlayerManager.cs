using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;
    private PlayerTear tear;

    // ���R���|�[�l���g
    [SerializeField] private ChangeLayerManager changeLayerManager;

    void Start()
    {
        // ���R���|�[�l���g���擾
        controller = GetComponent<PlayerController>();
        tear = GetComponent<PlayerTear>();

        tear.Initialize(controller);
    }

    void Update()
    {
        // ���C���[�ύX��������Ă��Ȃ��ꍇ�̂ݍX�V
        if (!changeLayerManager.GetIsActive())
        {
            tear.ManualUpdate();

            // �j�葀������Ă��Ȃ��ꍇ�̂ݍX�V
            if (!tear.GetIsActive())
            {
                controller.ManualUpdate();
            }
        }

        if (Input.GetButtonDown("Reset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
