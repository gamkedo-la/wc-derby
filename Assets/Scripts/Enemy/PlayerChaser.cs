using UnityEngine;
using System.Collections;

public class PlayerChaser : MonoBehaviour {
	public float chaseToRangeMin = 35.0f;
	public float chaseToRangeMax = 50.0f;
	public float speedMin = 40.0f;
	public float speedMax = 55.0f;

	public bool isGrounded = false;

	float tooClose = 25.0f;
	float chaseToRange = 30.0f; // based on min and max above, varies per enemy
	float driveSpeed = 50.0f;
	Vector3 aimTargetPos;
	float startY = 0.0f;
	Vector3 flyToPt;

	void newGoal() {
		Vector3 randGoalOffset;
		randGoalOffset = Random.onUnitSphere;
		if(isGrounded) {
			randGoalOffset.y = startY;
		} else {
			randGoalOffset.y *= 0.4f; // compress vertical offset range
		}

		if(isGrounded == false) {
			if(randGoalOffset.y < 0.0f) {
				randGoalOffset.y = -randGoalOffset.y;
			}
			randGoalOffset += Vector3.up * 0.6f;
		}

		flyToPt = Camera.main.transform.position
			+ randGoalOffset.normalized * chaseToRange;
	}

	IEnumerator changeFlyToPt() {
		while(true) {
			newGoal();
			yield return new WaitForSeconds( Random.Range(2.5f,6.0f) );
		}
	}

	// Use this for initialization
	void Start () {
		startY = transform.position.y;
		chaseToRange = Random.Range(chaseToRangeMin, chaseToRangeMax);
		driveSpeed = Random.Range(speedMin, speedMax);

		aimTargetPos = Camera.main.transform.position;
		StartCoroutine(changeFlyToPt());
	}

	// Update is called once per frame
	void Update () {
		if(Vector3.Distance(transform.position, Camera.main.transform.position) < tooClose) {
			newGoal();
			transform.position += transform.forward * driveSpeed * Time.deltaTime;
		} else if(Vector3.Distance(transform.position, flyToPt) > chaseToRange) {
			Vector3 lookDiff = flyToPt - transform.position;
			if(isGrounded) {
				lookDiff.y = 0.0f;
			}
			transform.rotation = Quaternion.Slerp(transform.rotation,
				Quaternion.LookRotation(lookDiff), Time.deltaTime * 1.0f);
			transform.position += transform.forward * driveSpeed * Time.deltaTime;
		} else {
			Vector3 lookDiff = aimTargetPos - transform.position;
			if(isGrounded) {
				lookDiff.y = 0.0f;
			}
			transform.rotation = Quaternion.Slerp(transform.rotation,
				Quaternion.LookRotation(lookDiff), Time.deltaTime * 1.0f);
		}

		if(isGrounded) {
			Vector3 pos = transform.position;
			pos.y = Terrain.activeTerrain.SampleHeight(transform.position)+15.5f;
			if(pos.y > transform.position.y) {
				transform.position += pos - transform.position + Vector3.down*8.0f;
			}
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			Destroy (gameObject);
		}
	}

	void FixedUpdate() {
		float chaseK = 0.992f;

		aimTargetPos = aimTargetPos * chaseK +
			Camera.main.transform.position * (1.0f - chaseK);
		aimTargetPos.y = transform.position.y; // to remain planer in base pivot
	}
}
