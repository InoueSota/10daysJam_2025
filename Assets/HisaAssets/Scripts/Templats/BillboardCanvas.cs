using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Transform camTransform;
    [SerializeField] bool rockY;

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (rockY)
        {
            Vector3 targetPosition = new Vector3(camTransform.position.x, transform.position.y, camTransform.position.z);
            transform.LookAt(targetPosition, Vector3.up);
        }
        else
        {
            Vector3 direction = transform.position - camTransform.position;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
	}
}
