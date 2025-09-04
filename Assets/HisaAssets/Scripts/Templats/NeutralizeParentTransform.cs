using UnityEngine;

public class NeutralizeAllParentTransforms : MonoBehaviour
{
    private Vector3 initialWorldPosition;
    private Quaternion initialWorldRotation;
    private Vector3 initialWorldScale;

    [Header("true�̏ꍇ�e�̉e����ł�����")]
    [SerializeField] bool scaleFlag;
    [SerializeField] bool rotationFlag;
    [SerializeField] bool positionFlag;
 

    void Start()
    {
        // ���[���h���W�A��]�A�X�P�[����ۑ�
        initialWorldPosition = transform.position;
        initialWorldRotation = transform.rotation;
        initialWorldScale = transform.lossyScale;

      
    }

    void LateUpdate()
    {
        // ���[���h���W�A��]�A�X�P�[�����ێ�
        if (positionFlag) transform.position = initialWorldPosition;
        if (rotationFlag) transform.rotation = initialWorldRotation;
        if (scaleFlag) transform.localScale = initialWorldScale;
    }
}