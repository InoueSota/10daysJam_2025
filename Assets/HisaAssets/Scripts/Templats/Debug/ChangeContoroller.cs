using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeContoroller : MonoBehaviour
{
    public static bool isJoycon=true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            isJoycon = !isJoycon;
        }
    }
}
