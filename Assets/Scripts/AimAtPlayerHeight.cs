using UnityEngine;
using System.Collections;

public class AimAtPlayerHeight : MonoBehaviour {
	Vector3 aimTargetPos;
	Transform target;

	// Use this for initialization
	void Start () {
		target = Camera.main.transform.parent;
		aimTargetPos = target.position;
	}

	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.Slerp(transform.rotation,
			Quaternion.LookRotation(aimTargetPos-transform.position), Time.deltaTime * 4.0f);
	}

	void FixedUpdate() {
		float chaseK = 0.965f;

		aimTargetPos = aimTargetPos * chaseK +
			(target.position+Vector3.up*5.0f) * (1.0f - chaseK);
	}
}
