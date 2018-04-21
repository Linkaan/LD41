using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

	void LateUpdate () {
        Vector3 target = Camera.main.transform.position;
        target.y = transform.position.y;
        transform.LookAt(target);
	}
}
