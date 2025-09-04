using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSthick : MonoBehaviour
{
    public Vector2 leftSthick;
    public Vector2 rightSthick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        leftSthick.x = Input.GetAxis("LeftHorizontal");
        leftSthick.y = Input.GetAxis("LeftVertical");

        rightSthick.x= Input.GetAxis("RightHorizontal");
        rightSthick.y= Input.GetAxis("RightVertical");
    }
}
