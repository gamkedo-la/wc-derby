using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinTrap : MonoBehaviour {
	public Vector3 spinAxis = Vector3.forward;
	public float spinRate = -600.0f;
	void Update () {
		transform.Rotate(spinAxis, Time.deltaTime * spinRate);
	}
}
