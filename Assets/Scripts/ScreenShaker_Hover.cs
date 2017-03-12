using UnityEngine;
using System.Collections;

public class ScreenShaker_Hover : MonoBehaviour {
	public static ScreenShaker_Hover instance;
	float shakeAmt = 0.0f;
	float decayRate = 0.915f;
	float MAX_POWER = 100.0f;
	Vector3 startLocalPos;
	Quaternion startLocalRot;

	void Start () {
		instance = this;
		startLocalPos = transform.localPosition;
		startLocalRot = transform.localRotation;
	}

	void LateUpdate() {
		transform.localPosition = startLocalPos + Random.insideUnitSphere * shakeAmt * 0.0125f;
		float rotAmt = Random.Range(-1.0f,1.0f) * shakeAmt * 0.1f;
		transform.localRotation = startLocalRot;
		transform.localRotation *= Quaternion.AngleAxis(rotAmt, Vector3.up);
		transform.localRotation *= Quaternion.AngleAxis(rotAmt, Vector3.right);
	}

	public void BlastShake(float power) {
		shakeAmt += power;
		if(shakeAmt > MAX_POWER) {
			shakeAmt = MAX_POWER;
		}
	}
	
	void FixedUpdate () {
		shakeAmt *= decayRate;
	}
}
