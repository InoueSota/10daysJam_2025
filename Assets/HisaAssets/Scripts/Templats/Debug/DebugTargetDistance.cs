using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTargetDistance : MonoBehaviour {
	Transform target;
	public float distance;
	// Start is called before the first frame update
	void Start() {
		target = GameObject.Find("Castle").transform;
	}

	// Update is called once per frame
	void Update() {
		if(target) {
			distance = Vector3.Distance(transform.position, target.position);
		}
	}
}
