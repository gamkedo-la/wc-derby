using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : HoverCraftBase {
	private float targetFOV = 60.0f;

	protected override void Init () {
		if(useCarCollisionTuning) {
			Vector3 camPosHigher = Camera.main.transform.localPosition;
			camPosHigher.y *= 1.3f;
			Camera.main.transform.localPosition = camPosHigher;

			UpdateLockIndicator uliScript = GetComponentInChildren<UpdateLockIndicator>();
			uliScript.TurnOff();
		}
	}

	protected override void Tick () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			sprintRamming = !sprintRamming;
			targetFOV = ((useCarCollisionTuning ? sprintRamming : HaveEnemyHooked()) ? 77.0f : 60.0f);
		}
		float cameraK = 0.8f;
		Camera.main.fieldOfView = cameraK * Camera.main.fieldOfView + (1.0f-cameraK) * targetFOV;

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
		AkSoundEngine.SetRTPCValue("Player_Velocity", enginePower / ramBoostMult);
		AkSoundEngine.SetRTPCValue ("Player_Elevation", 50f);
	}

}
