using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {
	public Waypoint[] next;

	void Start() {
		Vector3 pointToward = Vector3.zero;
		for(int ii = 0; ii < next.Length; ii++) {
			pointToward += next[ii].transform.position;
		}
		pointToward /= next.Length;
		//transform.LookAt(pointToward);
	}

	public Waypoint randNext() {
		return next[ Random.Range(0,next.Length) ];
	}
	public Waypoint prevPoint() { // not very efficient, but only used for player and if driving backwards
		for(int i = 0; i < HoverCraftBase.levelWayPointList.Count; i++) {
			Waypoint eachWP = HoverCraftBase.levelWayPointList[i].GetComponent<Waypoint>();
			for(int ii = 0; ii < eachWP.next.Length; ii++) {
				if(eachWP.next[ii] == this) {
					return eachWP;
				}
			}
		}
		Debug.Log("No prevPoint found for " + name);
		return null;
	}
	public Waypoint pointIsAlong(Vector3 forPt) {
		if(transform.InverseTransformPoint(forPt).z < 0.0f) {
			return null;
		}
		forPt.y = transform.position.y;
		for(int ii = 0; ii < next.Length; ii++) {
			if(next[ii].transform.InverseTransformPoint(forPt).z < 0.0f) {

				Vector3 flatPt = next[ii].transform.position;
				flatPt.y = transform.position.y;
				Vector3 nearestPt = Vector3.Project(forPt - transform.position,
					(flatPt - transform.position).normalized) +
					transform.position;
				float distTo = Vector3.Distance(nearestPt, flatPt);
				float distToPrev = Vector3.Distance(nearestPt, transform.position);
				float totalDist = Vector3.Distance(flatPt, transform.position);

				float sumDiff = (totalDist) - (distTo + distToPrev);
				if(sumDiff < 1.0f && distToPrev/totalDist > 0.0f && distToPrev/totalDist < 1.0f) {
					float widthHere = Mathf.Lerp(transform.localScale.x, next[ii].transform.localScale.x,
						distToPrev/totalDist) * 0.5f;

					if(Vector3.Distance(forPt, nearestPt) < widthHere) {
						return next[ii];
					}
				}
			}
		}
		return null;
	}

	// -1.0f is left side of track, 1.0f is right side of track
	public Vector3 trackPtForOffset(float offsetHere) {
		Vector3 trackPerpLine = transform.right;
		float trackWidthHere = transform.localScale.x;
		return transform.position + 0.5f * trackPerpLine * trackWidthHere * offsetHere;
	}
}
