using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinTrap : MonoBehaviour {
	void Update () {
		transform.Rotate(Vector3.forward, Time.deltaTime * -600.0f);
	}
}
