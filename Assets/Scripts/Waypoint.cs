using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

	[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
	static void DrawGizmoForWaypoint(Waypoint obj, GizmoType gizmoType)
	{
		Vector3 position = obj.transform.position;

		Vector3 barDim = Vector3.one * 40.0f;
		Gizmos.color = Color.red;
		Gizmos.DrawCube(position, barDim);
		barDim = Vector3.one * 40.0f;

		if(obj.next != null && obj.next.Length > 0) {
			Gizmos.color = Color.green;
			for(int i = 0; i < obj.next.Length; i++) {
				Gizmos.DrawLine(position, obj.next[i].transform.position);
			}

			Gizmos.color = Color.yellow;
			Vector3 trackRightEdge = obj.trackPtForOffset(1.0f);
			Vector3 trackLeftEdge = obj.trackPtForOffset(-1.0f);
			Gizmos.DrawLine(position, trackRightEdge);
			Gizmos.DrawLine(position, trackLeftEdge);
			for(int i=0;i<obj.next.Length;i++) {
				Waypoint nextWP = obj.next[i];

				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(nextWP.trackPtForOffset(1.0f), trackRightEdge);
				Gizmos.DrawLine(nextWP.trackPtForOffset(-1.0f), trackLeftEdge);
			}
		}

	}
}
