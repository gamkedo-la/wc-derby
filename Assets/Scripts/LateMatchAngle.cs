using UnityEngine;
using System.Collections;

public class LateMatchAngle : MonoBehaviour {
	void LateUpdate () {
		transform.rotation = Camera.main.transform.rotation;
	}
}
