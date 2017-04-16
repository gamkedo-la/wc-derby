using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : HoverCraftBase {
	private float targetFOV = 60.0f;

	protected override void Init () {
	}

	protected override void Tick () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			sprintRamming = !sprintRamming;
			targetFOV = (HaveEnemyHooked() ? 77.0f : 60.0f);
		}
		float cameraK = 0.8f;
		Camera.main.fieldOfView = cameraK * Camera.main.fieldOfView + (1.0f-cameraK) * targetFOV;

		Camera.main.transform.localRotation = Quaternion.AngleAxis(
			(HaveEnemyHooked() ? 2.0f : 0.15f)*Random.Range(-1.0f,1.0f)*gasControl,Vector3.forward);

		if(sprintRamming == false) {
			turnControl = Input.GetAxis("Horizontal");
			gasControl = Input.GetAxis("Vertical");
            AkSoundEngine.SetRTPCValue("Player_Velocity", gasControl);
		}
	}

}
