using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

	void LateUpdate () {
		Vector3 camAtHeight = Camera.main.transform.position;
		camAtHeight.y = transform.position.y;
		transform.LookAt(camAtHeight);
		transform.Rotate(Vector3.up, 180.0f);
	}
}
