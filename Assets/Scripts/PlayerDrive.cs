using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : HoverCraftBase {
	private float targetFOV = 60.0f;
	private Vector3 camStartVect;

	private uint turnLeftID;
	private uint turnRightID;

	protected override void Init () {
		if(useCarCollisionTuning) {
			Vector3 camPosHigher = Camera.main.transform.localPosition;
			camPosHigher.y *= 1.3f;
			Camera.main.transform.localPosition = camPosHigher;

			UpdateLockIndicator uliScript = GetComponentInChildren<UpdateLockIndicator>();
			uliScript.TurnOff();
		}
		camStartVect = Camera.main.transform.position - bodyToTilt.transform.position;

		turnLeftID = AkSoundEngine.GetIDFromString("Play_PlayerEngineTurnLeft");
		turnRightID = AkSoundEngine.GetIDFromString("Play_PlayerEngineTurnRight");
	}

	protected override void Tick () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			sprintRamming = !sprintRamming;
			targetFOV = ((useCarCollisionTuning ? sprintRamming : HaveEnemyHooked()) ? 77.0f : 60.0f);
		}
		float cameraK = 0.8f;
		Camera.main.fieldOfView = cameraK * Camera.main.fieldOfView + (1.0f-cameraK) * targetFOV;

		Vector3 projectedCamPos = transform.position - transform.forward * 4.5f * shipScale +
			Vector3.up * 0.85f * shipScale;
		projectedCamPos = HoverCraftBase.ForceIntoDome(projectedCamPos);
		Vector3 vectDiff = projectedCamPos - transform.position;
		Ray rayLine = new Ray(transform.position, vectDiff);
		RaycastHit rhInfo;
		if(Physics.Raycast(rayLine, out rhInfo, vectDiff.magnitude, HoverCraftBase.ignoreVehicleLayerMask)) {
			Camera.main.transform.position = rhInfo.point;
		} else {
			Camera.main.transform.position = projectedCamPos;
		}
			
		Camera.main.transform.localRotation = Quaternion.AngleAxis(
			(HaveEnemyHooked() ? 2.0f : 0.15f)*Random.Range(-1.0f,1.0f)*gasControl,Vector3.forward);
		
		if(sprintRamming == false) {
			turnControl = Input.GetAxis("Horizontal");
			gasControl = Input.GetAxis("Vertical");
		} else if(useCarCollisionTuning) {
			turnControl = Input.GetAxis("Horizontal");
			if(Input.GetAxis("Vertical") < 0.0f) {
				sprintRamming = false;
			}
		}

		if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
			AkSoundEngine.PostEvent(turnLeftID, gameObject);
		}
		if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
			AkSoundEngine.PostEvent(turnRightID, gameObject);
		}

		float tiltAmt = Mathf.DeltaAngle(0.0f, bodyToTilt.eulerAngles.x);
		float maxTiltDetected = 30.0f;
		tiltAmt = Mathf.Clamp(tiltAmt, -maxTiltDetected, maxTiltDetected) / -maxTiltDetected;
		AkSoundEngine.SetRTPCValue("Player_Velocity", enginePower / ramBoostMult);
		AkSoundEngine.SetRTPCValue ("Player_Tilt", tiltAmt);
	}

}
// AkSoundEngine.SetRTPCValue ("Player_Tilt", Input.GetAxis("Vertical"));