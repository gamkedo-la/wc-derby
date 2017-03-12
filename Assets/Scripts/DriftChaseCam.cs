using UnityEngine;
using System.Collections;

public class DriftChaseCam : MonoBehaviour {
	public Transform chaseThing;
	Camera hoverCam;
	Vector3 camDest;
	Vector3 newDest;
	public float vOffset = -17.0f;
	public float distOffset = -47.0f;
	public float camDriftK = 0.84f;
	public float camTurnScale = 9.0f;

	public float camTiltOffHorizon = 0.0f;

	void Start () {
		hoverCam = GetComponent<Camera>();
		camDest = hoverCam.transform.position;
	}

	void LateUpdate() {
		camDest = newDest * (1.0f-camDriftK) +
			camDest * camDriftK;
		hoverCam.transform.rotation = chaseThing.transform.rotation * Quaternion.AngleAxis(camTiltOffHorizon,Vector3.right);
		hoverCam.transform.position = camDest;
			/* Quaternion.Slerp(hoverCam.transform.rotation,
				transform.rotation * Quaternion.AngleAxis(camTiltOffHorizon,Vector3.right), Time.deltaTime * camTurnScale);*/
	}

	void FixedUpdate() {
		newDest = chaseThing.transform.position + Vector3.up * vOffset + chaseThing.transform.forward * distOffset;
	}
}
