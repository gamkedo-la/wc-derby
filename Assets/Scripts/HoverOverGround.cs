using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverOverGround : MonoBehaviour {
	float hoverPointY = 0.0f;

	float heightUnderMe(Vector3 atPos) {
		float lookdownFromAboveHeight = 30.0f;
		RaycastHit rhInfo;
		if(Physics.Raycast(atPos+Vector3.up*lookdownFromAboveHeight,
			-Vector3.up*lookdownFromAboveHeight,out rhInfo,8.0f,HoverCraftBase.ignoreVehicleLayerMask)) {
			return rhInfo.point.y;
		}
		else if (Terrain.activeTerrain != null) {
			return Terrain.activeTerrain.SampleHeight(atPos);
		}
		else {
			// there may be no terrain in the scene
			return lookdownFromAboveHeight; // nothing underneath us
		}
	}

	void Start() {
		hoverPointY = heightUnderMe(transform.position);
	}

	// Update is called once per frame
	void Update () {
		Vector3 floatHeight = transform.position;
		floatHeight.y = hoverPointY + 30.0f + Mathf.Cos(Time.time) * 5.0f;
		transform.position = floatHeight;
	}
}
