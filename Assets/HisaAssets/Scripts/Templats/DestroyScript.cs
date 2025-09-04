using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyScript : MonoBehaviour
{
    [SerializeField]float destroyTime=3;
    // Start is called before the first frame update
    void Start()
    {
		// âπÇçƒê∂(AudioSource)
		//GetComponent<AudioSource>().Play();
	}

    // Update is called once per frame
    void Update()
    {
        destroyTime -= Time.deltaTime;
        if ( destroyTime < 0 )
        {
            Destroy( this.gameObject );
        }
    }
}
