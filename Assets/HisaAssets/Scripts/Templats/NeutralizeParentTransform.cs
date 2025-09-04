using UnityEngine;

public class NeutralizeAllParentTransforms : MonoBehaviour
{
    private Vector3 initialWorldPosition;
    private Quaternion initialWorldRotation;
    private Vector3 initialWorldScale;

    [Header("trueの場合親の影響を打ち消す")]
    [SerializeField] bool scaleFlag;
    [SerializeField] bool rotationFlag;
    [SerializeField] bool positionFlag;
 

    void Start()
    {
        // ワールド座標、回転、スケールを保存
        initialWorldPosition = transform.position;
        initialWorldRotation = transform.rotation;
        initialWorldScale = transform.lossyScale;

      
    }

    void LateUpdate()
    {
        // ワールド座標、回転、スケールを維持
        if (positionFlag) transform.position = initialWorldPosition;
        if (rotationFlag) transform.rotation = initialWorldRotation;
        if (scaleFlag) transform.localScale = initialWorldScale;
    }
}