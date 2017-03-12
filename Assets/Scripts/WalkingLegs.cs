using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkingLegs : MonoBehaviour {
	public Transform leftLeg;
	public Transform rightLeg;

	public Transform[] upperBody;

	bool lastSteppedLeft = true;

	private Vector3 localRelLeft;
	private Vector3 localRelRight;
	private List<Vector3> localUpperBody = new List<Vector3>();

	private Vector3 prevPos;
	private float distWalkedTotal;
	private float walkVertPerc = 0.0f;
	float footShakeMult = 3.0f;

	// Use this for initialization
	void Start () {
		localRelLeft = leftLeg.localPosition;
		localRelRight = rightLeg.localPosition;
		prevPos = transform.position;
		for(int i = 0; i < upperBody.Length; i++) {
			localUpperBody.Add(upperBody[i].localPosition);
		}
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < upperBody.Length; i++) {
			if(upperBody[i] != null) {
				upperBody[i].localPosition = localUpperBody[i] +
				Vector3.up * Mathf.Cos(Time.timeSinceLevelLoad * 1.4f + (i * 0.1f)) * 0.1f;
			}
		}

		float latestDist = Vector3.Distance(transform.position, prevPos);
		float newVertPerc = 0.0f;
		if(latestDist > 0.2f) {
			newVertPerc = 0.6f;
		}
		float vertPercK = 0.15f;
		walkVertPerc = newVertPerc * vertPercK + walkVertPerc * (1.0f-vertPercK);

		distWalkedTotal += Vector3.Distance(transform.position, prevPos);
		prevPos = transform.position;

		Vector3 leftOffset = 
			Quaternion.AngleAxis(distWalkedTotal * 20.0f, Vector3.right) * Vector3.up * 0.3f;
		Vector3 rightOffset = 
			Quaternion.AngleAxis(distWalkedTotal*20.0f
				+180.0f, Vector3.right) * Vector3.up * 0.3f;
		if(leftOffset.y > 0.0f) {
			leftOffset.y *= walkVertPerc;
		} else {
			if(lastSteppedLeft) {
				if(ScreenShaker_Tank.instance) {
					ScreenShaker_Tank.instance.BlastShake(1.0f * footShakeMult);
					ScreenShaker_Hover.instance.BlastShake(0.5f * footShakeMult);
				}
				SoundSet.PlayClipByName("robostep", 0.15f);
				lastSteppedLeft = false;
			}
			leftOffset.y = 0.0f;
		}
		if(rightOffset.y > 0.0f) {
			rightOffset.y *= walkVertPerc;
		} else {
			if(lastSteppedLeft==false) {
				if(ScreenShaker_Tank.instance) {
					ScreenShaker_Tank.instance.BlastShake(1.0f * footShakeMult);
					ScreenShaker_Hover.instance.BlastShake(0.5f * footShakeMult);
				}
				SoundSet.PlayClipByName("robostep", 0.15f);
				lastSteppedLeft = true;
			}
			rightOffset.y = 0.0f;
		}

		leftLeg.localPosition = localRelLeft + leftOffset;
		rightLeg.localPosition = localRelRight + rightOffset;
	}
}
