using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {
	public Waypoint[] next;

	public Waypoint randNext() {
		return next[ Random.Range(0,next.Length) ];
	}

	// -1.0f is left side of track, 1.0f is right side of track
	public Vector3 trackPtForOffset(float offsetHere) {
		Vector3 trackPerpLine = transform.right;
		float trackWidthHere = transform.localScale.x;
		return transform.position + 0.5f * trackPerpLine * trackWidthHere * offsetHere;
	}
}
