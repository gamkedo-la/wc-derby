using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamTrap : MonoBehaviour {
	Quaternion baseRot;
	void Start() {
		baseRot = transform.rotation;
	}
	void Update () {
		float timeToAngle = Mathf.Cos(Time.time * 2.3f);
		// odd number goes both ways, even number would go one way
		// mainly doing this to get hammer to stall when upright & go down faster
		timeToAngle = timeToAngle * timeToAngle * timeToAngle * timeToAngle * timeToAngle;
		timeToAngle *= 75.0f;
		transform.rotation = baseRot * 
			Quaternion.AngleAxis(timeToAngle, Vector3.forward);
	}
}
