using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YtoZPosition : MonoBehaviour
{
    [SerializeField] float coefficient;
    Vector3 newPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         newPos=this.transform.localPosition;
        newPos.z = this.transform.localPosition.y * coefficient;
        this.transform.localPosition = newPos;
    }
}
