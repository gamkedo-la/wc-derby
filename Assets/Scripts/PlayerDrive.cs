using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : MonoBehaviour {
	Vector3 momentum;
	public Transform bodyToTilt;

	public TrailRenderer[] trList;
	public Light[] engineLights;
	public bool boostDrawing = true;
	int ignorePlayerLayerMask;

	// Use this for initialization
	void Start () {
		momentum = Vector3.zero;
		ignorePlayerLayerMask = ~LayerMask.GetMask("Player");
	}

	float heightUnderMe(Vector3 atPos) {
		float lookdownFromAboveHeight = 50.0f;
		RaycastHit rhInfo;
		if(Physics.Raycast(atPos+Vector3.up*lookdownFromAboveHeight,
			-Vector3.up*lookdownFromAboveHeight,out rhInfo,200.0f,ignorePlayerLayerMask)) {
			return rhInfo.point.y;
		} else {
			return Terrain.activeTerrain.SampleHeight(atPos);
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * 80.0f * Time.deltaTime);
		momentum *= 0.94f;
		momentum += transform.forward * bodyToTilt.forward.y * Time.deltaTime * -7.0f;
		momentum += transform.right * bodyToTilt.right.y * Time.deltaTime * -10.0f;
	
		float enginePower;

		RaycastHit rhInfo;
		float impendingCrashDetectionNormal = 1.0f;
		if(Physics.Raycast(transform.position,
			transform.forward, out rhInfo, 10.0f, ignorePlayerLayerMask)) {
			//Debug.DrawLine(rhInfo.point, transform.position, Color.red);
			//Debug.DrawLine(rhInfo.point, rhInfo.point+rhInfo.normal*3.0f, Color.green);
			impendingCrashDetectionNormal = rhInfo.normal.y;
		}

		if(impendingCrashDetectionNormal < 0.1f) {
			momentum *= 0.5f;
			enginePower = -1.0f;
		} else {
			enginePower = Input.GetAxis("Vertical");
		}

		momentum += transform.forward * enginePower * 9.0f * Time.deltaTime;
		transform.position += momentum;

		bool wasDrawingBoost = boostDrawing;
		boostDrawing = enginePower > 0.2f;
		/*if(wasDrawingBoost != boostDrawing) {
				// to enable or disable instead of just scaling
		}*/
		float trailWidth = (Mathf.Max(enginePower, 0.0f)) * 0.5f;
		for(int i = 0; i < trList.Length; i++) {
			trList[i].widthMultiplier= trailWidth;
		}
		float engineLight = (Mathf.Max(enginePower, 0.0f)) * 2.0f;
		for(int i = 0; i < engineLights.Length; i++) {
			engineLights[i].intensity= engineLight;
		}

		float minHeightHere = heightUnderMe(transform.position)+0.9f;
		Vector3 newPos = transform.position;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;
	}
	void FixedUpdate() {
		float minHeightHere = heightUnderMe(transform.position)+0.9f;
		float goalHeightHere = minHeightHere + 1.7f;
		RaycastHit rhInfo;

		Vector3 newPos = transform.position;
		newPos.y += (goalHeightHere-newPos.y) * (goalHeightHere > newPos.y ? 3.0f : 1.0f ) * Time.deltaTime;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;

		float heightForward = 0.0f;

		heightForward = 
			Mathf.Max(
			heightUnderMe(transform.position + transform.forward * 10.0f),
			heightUnderMe(transform.position + transform.forward * 15.0f),
			heightUnderMe(transform.position + transform.forward * 25.0f));

		if(Physics.Raycast(transform.position,-Vector3.up*8.0f,out rhInfo)) {
			Vector3 pointAhead = transform.forward;
			if(heightForward != 0.0f) {
				pointAhead = transform.forward + Vector3.up * (heightForward - rhInfo.point.y)*0.1f;
			}
			bodyToTilt.transform.rotation = 
				Quaternion.Slerp(bodyToTilt.transform.rotation,
					Quaternion.LookRotation(pointAhead, rhInfo.normal),
					0.07f);
		}

	}
}
