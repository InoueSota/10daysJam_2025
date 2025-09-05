using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private PlayerController controller;
    private PlayerCut cut;

    // �q�R���|�[�l���g
    [SerializeField] private DeathEffectSpawner deathEffectSpawner;

    // ���R���|�[�l���g
    private UndoManager undoManager;
    private Camera mainCamera;

    void Start()
    {
        // ���R���|�[�l���g���擾
        controller = GetComponent<PlayerController>();
        cut = GetComponent<PlayerCut>();

        // ���R���|�[�l���g���擾
        undoManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UndoManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // �X�^�b�N���Ă��Ȃ��Ƃ�
        if (!controller.GetIsStacking())
        {
            cut.ManualUpdate();
        }
        controller.ManualUpdate();

        // ���S����
        DeathChecker();

        // Undo
        if (Input.GetButtonDown("Undo")) { undoManager.Undo(); }

        // Reset
        if (Input.GetButtonDown("Reset")) { undoManager.ResetToInitialState(); }
    }

    /// <summary>
    /// ���S����
    /// </summary>
    void DeathChecker()
    {
        // �v���C���[�̈ʒu���r���[�|�[�g���W�ɕϊ�
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // ��ʓ��`�F�b�N�i0�`1�͈̔́j
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            // ���S�ӏ��ɃG�t�F�N�g���o��
            deathEffectSpawner.SpawnEffect(transform.position);

            undoManager.Undo();
        }
    }
}
