using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEffectScript: MonoBehaviour
{

    [SerializeField, Header("ターゲット")] Transform target;

    public void SetTarget(Transform targetObj) { target = targetObj; }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        //transform.position = (1.0f - 15f*Time.deltaTime) * transform.position + target.position *15f * Time.deltaTime;
        transform.position=target.position;
    }
}
