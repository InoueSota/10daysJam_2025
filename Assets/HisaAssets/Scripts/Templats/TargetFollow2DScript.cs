using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollow2DScript : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float easeT = 0.3f;//値が小さいほど早い
    [SerializeField] bool getX = true;
    [SerializeField] bool getY = true;
    [SerializeField] float zPos = -10;

    private Vector2 velocity = Vector3.zero; // SmoothDampで必要な内部速度
    public void SetTarget(Transform set) { target = set; }
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

    public void SetTargetImmediately(Transform targetTransform)
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
        Vector2 newPos = Vector2.SmoothDamp(transform.position, target.position, ref velocity, easeT);
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
