using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugVel : MonoBehaviour
{
    Rigidbody rb;
    Rigidbody2D rb2D;
    public Vector3 vel;
    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        rb2D=GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb) { vel = rb.linearVelocity; }
        if (rb2D) { vel = rb2D.linearVelocity; }
    }
}
