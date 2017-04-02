using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerconeSpin : MonoBehaviour {
	public float spinRate;
	void Update () {
		transform.Rotate(Vector3.forward, Time.deltaTime * spinRate);
	}
}
