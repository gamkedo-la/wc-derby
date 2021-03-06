﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : HoverCraftBase {
	private float targetFOV = 60.0f;
	private Vector3 camStartVect;

	public static PlayerDrive instance;

	private AkTriggerTurnLeft akTurnLeft;
	private AkTriggerTurnRight akTurnRight;

	protected override void Init () {
		instance = this;
		akTurnLeft = GetComponent<AkTriggerTurnLeft>();
		akTurnRight = GetComponent<AkTriggerTurnRight>();
		if(useCarCollisionTuning) {
			Vector3 camPosHigher = Camera.main.transform.localPosition;
			camPosHigher.y *= 1.3f;
			Camera.main.transform.localPosition = camPosHigher;

			UpdateLockIndicator uliScript = GetComponentInChildren<UpdateLockIndicator>();
			uliScript.TurnOff();
		}
		camStartVect = Camera.main.transform.position - bodyToTilt.transform.position;

		//turnLeftID = AkSoundEngine.GetIDFromString("Play_PlayerEngineTurnLeft");
		//turnRightID = AkSoundEngine.GetIDFromString("Play_PlayerEngineTurnRight");
	}

	IEnumerator unboostWaiting = null;
	IEnumerator unboost() {
		yield return new WaitForSeconds(3.0f);
		if(sprintRamming) {
			toggleSprintRamWithCameraFOVEffect();
		}
	}

	void toggleSprintRamWithCameraFOVEffect() {
		sprintRamming = !sprintRamming;
		targetFOV = ((useCarCollisionTuning ? sprintRamming : HaveEnemyHooked()) ? 77.0f : 60.0f);
		if(sprintRamming) {
			AkSoundEngine.PostEvent ("Play_PlayerEngineTurnLeft", gameObject);
		} else {
			AkSoundEngine.PostEvent("Play_PlayerEngineTurnRight", gameObject);
		}
	}

	protected override void Tick () {
		if(Input.GetKey(KeyCode.C)) {
			Camera.main.fieldOfView = 77.0f;
			Camera.main.transform.position = domeCenter + Vector3.up * domeRadius*1.3f;
			Camera.main.transform.LookAt(domeCenter);
			return;
		}

		if(Input.GetKeyDown(KeyCode.Space)) {
			toggleSprintRamWithCameraFOVEffect();
			if(sprintRamming) {
				if(unboostWaiting != null) {
					StopCoroutine(unboostWaiting);
				}
				unboostWaiting = unboost();
				StartCoroutine(unboostWaiting);
			}
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
		transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
		Vector3 cutXTilt = transform.rotation.eulerAngles;
		cutXTilt.x = 0.0f; // dunno what was causing this bug but it was bad
		transform.rotation = Quaternion.Euler(cutXTilt);
		/*Camera.main.transform.localRotation = Quaternion.AngleAxis(
			(HaveEnemyHooked() ? 2.0f : 0.15f)*Random.Range(-1.0f,1.0f)*gasControl,Vector3.forward);*/
		
		if(sprintRamming == false) {
			turnControl = Input.GetAxis("Horizontal");
			gasControl = Input.GetAxis("Vertical");
		} else if(useCarCollisionTuning) {
			turnControl = Input.GetAxis("Horizontal");
			if(Input.GetAxis("Vertical") < 0.0f) {
				sprintRamming = false;
			}
		}

		/*if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
			//AkSoundEngine.PostEvent(turnLeftID, gameObject);
			GetComponent<AkTriggerTurnLeft>().TurningLeft();
		}
		if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
			//AkSoundEngine.PostEvent(turnRightID, gameObject);
			GetComponent<AkTriggerTurnRight>().TurningRight();
		}*/

		float tiltAmt = Mathf.DeltaAngle(0.0f, bodyToTilt.eulerAngles.x);
		float maxTiltDetected = 30.0f;
		tiltAmt = Mathf.Clamp(tiltAmt, -maxTiltDetected, maxTiltDetected) / -maxTiltDetected;
		AkSoundEngine.SetRTPCValue("Player_Velocity", enginePower / ramBoostMult);
		AkSoundEngine.SetRTPCValue ("Player_Tilt", tiltAmt);
	}

	void OnTriggerEnter(Collider whichColl) {
		if(whichColl.gameObject.layer == LayerMask.NameToLayer("Item")) { 
			ItemPickup ipScript = whichColl.gameObject.GetComponent<ItemPickup>();
			switch(ipScript.powerupType) {
			case ItemPickup.ItemKind.Health:
				if(health < maxHealth) {
					ChangeHealth(maxHealth);
					AkSoundEngine.PostEvent ("Play_Spanner", gameObject);
					Destroy(whichColl.gameObject);
				}
				break;
			case ItemPickup.ItemKind.Shield:
				if(hasShield == false) {
					AkSoundEngine.PostEvent ("Play_Shield", gameObject);
					setShieldState(true);
					Destroy(whichColl.gameObject);
				}
				break;
			}
		}
	}
}
// AkSoundEngine.SetRTPCValue ("Player_Tilt", Input.GetAxis("Vertical"));