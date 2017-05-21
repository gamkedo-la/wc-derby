using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Waypoint : MonoBehaviour {
	public Waypoint[] next;

	public Waypoint randNext() {
		return next[ Random.Range(0,next.Length) ];
	}

	[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
	static void DrawGizmoForMyScript(Waypoint obj, GizmoType gizmoType)
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
				//Vector3 barDim = Vector3.one * 40.0f;
				//Gizmos.DrawCube(transform.position, barDim);
			}

			float trackWidthHere = obj.transform.localScale.x;
			Vector3 trackPerpLine = obj.transform.right;
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(position, position+0.5f*trackPerpLine*trackWidthHere);
			Gizmos.DrawLine(position, position-0.5f*trackPerpLine*trackWidthHere);
			for(int i=0;i<obj.next.Length;i++) {
				Vector3 nextPos = obj.next[i].transform.position;
				float nextWidthHere = obj.next[i].transform.localScale.x;
				Vector3 nextPerpLine = obj.next[i].transform.right;

				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(nextPos+0.5f*nextPerpLine*nextWidthHere, position+0.5f*trackPerpLine*trackWidthHere);
				Gizmos.DrawLine(nextPos-0.5f*nextPerpLine*nextWidthHere, position-0.5f*trackPerpLine*trackWidthHere);
			}
		}

	}
}
