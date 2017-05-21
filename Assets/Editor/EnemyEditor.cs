using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyEditor : MonoBehaviour {
	/*
	[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
	static void DrawGizmoForEnemyDrive(EnemyDrive obj, GizmoType gizmoType)
	{
		Vector3 nextWPTrackLeft = obj.myWaypoint.trackPtForOffset(-1.0f);
		Vector3 nextWPTrackRight = obj.myWaypoint.trackPtForOffset(1.0f);

		Vector3 positionLeft = Vector3.Lerp(nextWPTrackLeft, obj.prevWPTrackLeft, obj.percLeftToNextWP);
		Vector3 positionRight = Vector3.Lerp(nextWPTrackRight, obj.prevWPTrackRight, obj.percLeftToNextWP);

		Vector3 barDim = Vector3.one * 40.0f;
		float angleFromLeftEdge = AngleAroundAxis(obj.transform.position - obj.prevWPTrackLeft,
			nextWPTrackLeft - obj.prevWPTrackLeft,Vector3.up);
		float angleFromRightEdge = AngleAroundAxis(obj.transform.position - obj.prevWPTrackRight,
			nextWPTrackRight - obj.prevWPTrackRight,Vector3.up);
		Debug.Log(Mathf.RoundToInt(angleFromLeftEdge) + " " + Mathf.RoundToInt(angleFromRightEdge));
		Gizmos.color = (angleFromLeftEdge > 0.0f ? Color.red : Color.cyan);
		Gizmos.DrawCube(positionLeft, barDim);
		Gizmos.color = (angleFromRightEdge < 0.0f ? Color.red : Color.yellow);
		Gizmos.DrawCube(positionRight, barDim);
	}*/
}
