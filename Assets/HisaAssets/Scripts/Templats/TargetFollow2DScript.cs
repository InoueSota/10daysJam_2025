using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollow2DScript : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float easeT = 20f;
    [SerializeField] bool getX;
    [SerializeField] bool getY;
    [SerializeField] float zPos;

    // Start is called before the first frame update
    void Start()
    {
        if (target == null) { return; }
        Vector2 newPos = target.position;
        if (!getX)
        {

            newPos.x = transform.position.x;
        }
        if (!getY)
        {

            newPos.y = transform.position.y;
        }
        transform.position = new Vector3(newPos.x, newPos.y, zPos);
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
        if (target == null) { return; }
        Vector2 newPos = target.position;
        if (!getX)
        {

            newPos.x = transform.position.x;
        }
        if (!getY)
        {

            newPos.y = transform.position.y;
        }
        transform.position = new Vector3(newPos.x, newPos.y, zPos);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) { return; }
        Vector2 newPos = Vector2.Lerp(transform.position, target.position, easeT * Time.unscaledDeltaTime);
        if (!getX)
        {

            newPos.x = transform.position.x;
        }
        if (!getY)
        {

            newPos.y = transform.position.y;
        }
        transform.position = new Vector3(newPos.x, newPos.y, zPos);
    }

}
