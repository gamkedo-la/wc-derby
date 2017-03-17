using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : MonoBehaviour {
	Vector3 momentum;
	public Transform bodyToTilt;

	public TrailRenderer[] trList;
	public Light[] engineLights;
	public bool boostDrawing = true;

	// Use this for initialization
	void Start () {
		momentum = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * 80.0f * Time.deltaTime);
		momentum *= 0.94f;
		momentum += transform.forward * bodyToTilt.forward.y * Time.deltaTime * -10.0f;
		momentum += transform.right * bodyToTilt.right.y * Time.deltaTime * -10.0f;
		momentum += transform.forward * Input.GetAxis("Vertical") * 9.0f * Time.deltaTime;
		transform.position += momentum;

		bool wasDrawingBoost = boostDrawing;
		boostDrawing = Input.GetAxis("Vertical") > 0.2f;
		/*if(wasDrawingBoost != boostDrawing) {
				// to enable or disable instead of just scaling
		}*/
		float trailWidth = (Mathf.Max(Input.GetAxis("Vertical"), 0.0f)) * 0.5f;
		for(int i = 0; i < trList.Length; i++) {
			trList[i].widthMultiplier= trailWidth;
		}
		float engineLight = (Mathf.Max(Input.GetAxis("Vertical"), 0.0f)) * 2.0f;
		for(int i = 0; i < engineLights.Length; i++) {
			engineLights[i].intensity= engineLight;
		}


		float minHeightHere = Terrain.activeTerrain.SampleHeight(transform.position)+0.7f;
		Vector3 newPos = transform.position;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;
	}
	void FixedUpdate() {
		float minHeightHere = Terrain.activeTerrain.SampleHeight(transform.position)+0.7f;
		float goalHeightHere = minHeightHere + 1.5f;
		Vector3 newPos = transform.position;
		newPos.y += (goalHeightHere-newPos.y) * 0.24f;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;

		RaycastHit rhInfo;
		float heightForward = 0.0f;
		float hillOverheadLookdownFudgeHeight = 30.0f;
		if(Physics.Raycast(transform.position+transform.forward * 10.0f+Vector3.up*hillOverheadLookdownFudgeHeight,
			-Vector3.up*(hillOverheadLookdownFudgeHeight + 8.0f),out rhInfo)) {

			heightForward = rhInfo.point.y;
		}

		if(Physics.Raycast(transform.position,-Vector3.up*8.0f,out rhInfo)) {
			Vector3 pointAhead = transform.forward;
			if(heightForward != 0.0f) {
				pointAhead = transform.forward + Vector3.up * (heightForward - rhInfo.point.y)*0.1f;
			}
			bodyToTilt.transform.rotation = 
				Quaternion.Slerp(bodyToTilt.transform.rotation,
					Quaternion.LookRotation(pointAhead, rhInfo.normal),
					0.04f);
		}

	}
}
