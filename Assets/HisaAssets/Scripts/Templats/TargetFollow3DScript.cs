using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollow3DScript : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float easeT = 20f;
    [SerializeField] bool getX;
    [SerializeField] bool getY;
    [SerializeField] bool getZ;
    [SerializeField] float yPos;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 newPos = target.position;

        if (!getX)
        {

            newPos.x = transform.position.x;
        }
        if (!getY)
        {

            newPos.y = transform.position.y;
        }
        else
        {
            newPos.y = yPos;

        }
        if (!getZ)
        {

            newPos.z = transform.position.z;
        }
        transform.position = new Vector3(newPos.x, newPos.y, newPos.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 newPos = Vector3.Lerp(transform.position, target.position, easeT * Time.deltaTime);
        //getÇµÇ»Ç¢èÍçáå≥ÇÃà íu(transform.position)Çë„ì¸Ç∑ÇÈ
        if (!getX)
        {

            newPos.x = transform.position.x;
        }
        if (!getY)
        {

            newPos.y = transform.position.y;
        }
        else
        {
            newPos.y = yPos;

        }
        if (!getZ)
        {

            newPos.z = transform.position.z;
        }
        transform.position = new Vector3(newPos.x, newPos.y, newPos.z);
    }

}
