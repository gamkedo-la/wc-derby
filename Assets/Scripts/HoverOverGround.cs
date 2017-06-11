using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverOverGround : MonoBehaviour {

	float heightUnderMe(Vector3 atPos) {
		float lookdownFromAboveHeight = 2.0f;
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

	// Update is called once per frame
	void Update () {
		Vector3 floatHeight = transform.position;
		floatHeight.y = heightUnderMe(transform.position) + 20.0f + Mathf.Cos(Time.time) * 10.0f;
		transform.position = floatHeight;
	}
}
