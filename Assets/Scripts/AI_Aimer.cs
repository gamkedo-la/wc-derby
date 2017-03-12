using UnityEngine;
using System.Collections;

public class AI_Aimer : MonoBehaviour {

	public Transform bodyTransform;
	public Transform leftArmWep;
	public Transform rightArmWep;

	float maxAimRange = 700.0f;

	int aimerMask;
	private Transform target;

	// Use this for initialization
	void Start () {
		target = Camera.main.transform.parent;
	}

	// Update is called once per frame
	void Update () {
		if( EndOfRoundMessage.instance.beenTriggered ) {
			return;
		}

		Vector3 goalPt;
		Quaternion targetDir;
		goalPt = target.position;

		Vector3 relPos = bodyTransform.InverseTransformPoint(goalPt);
		float nearestAim = 8.0f;
		if(relPos.z < nearestAim) {
			relPos.z = nearestAim;
			goalPt = bodyTransform.TransformPoint(relPos);
		}

		targetDir = Quaternion.LookRotation(goalPt - leftArmWep.position);
		leftArmWep.rotation = Quaternion.Slerp(leftArmWep.rotation, targetDir, Time.deltaTime * 3.0f);
		targetDir = Quaternion.LookRotation(goalPt - rightArmWep.position);
		rightArmWep.rotation = Quaternion.Slerp(rightArmWep.rotation, targetDir, Time.deltaTime * 3.0f);

	}
}
