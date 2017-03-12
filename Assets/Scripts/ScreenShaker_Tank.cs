using UnityEngine;
using System.Collections;

public class ScreenShaker_Tank : MonoBehaviour {
	public Transform myTankRef;

	public static ScreenShaker_Tank instance;
	float shakeAmt = 0.0f;
	float decayRate = 0.87f;
	float MAX_POWER = 30.0f;
	Vector3 startLocalPos;
	Quaternion startLocalRot;
	int whiskerMask;

	public float startDist;
	Vector3 relDir;

	void Start () {
		instance = this;
		startLocalPos = transform.localPosition;
		startLocalRot = transform.localRotation;

		startDist = startLocalPos.magnitude;
		relDir = startLocalPos.normalized;

		whiskerMask = ~LayerMask.GetMask("OnlyBlockPlayer");
	}

	void Update() {

		float poleLen;
		RaycastHit rhInfo;
		if(Physics.Raycast(myTankRef.transform.position,
			myTankRef.transform.TransformDirection( relDir ),
			out rhInfo, startDist, whiskerMask)) {
			poleLen = Vector3.Distance(myTankRef.transform.position,rhInfo.point);
		} else {
			poleLen = startDist;
		}

		startLocalPos = relDir*poleLen + Vector3.up*(startDist-poleLen)*0.2f;

		transform.localPosition = startLocalPos + Random.insideUnitSphere * shakeAmt * 0.0025f;
		float rotAmt = Random.Range(-1.0f,1.0f) * shakeAmt * 0.02f;
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
