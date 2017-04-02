using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrive : HoverCraftBase {
	protected override void Init () {
		StartCoroutine(ChangeDir());
	}

	IEnumerator ChangeDir() {
		while(true) {
			Vector3 vectToCenter = domeCenter - transform.position;
			bool returnTowardMiddle = vectToCenter.magnitude > 0.7f * domeRadius;
			if(returnTowardMiddle) {
				float angFacingNow = Mathf.Atan2(transform.forward.z, transform.forward.x);
				float angFacingCenter = Mathf.Atan2(vectToCenter.z, vectToCenter.x);
				turnControl = (Mathf.DeltaAngle(angFacingNow, angFacingCenter) < 0.0f ? 1.0f : -1.0f) * Random.Range(0.5f,1.0f);
			} else {
				turnControl = Random.Range(0.0f, 1.0f) - Random.Range(0.0f, 1.0f);
			}
			gasControl = Random.Range(0.6f, 0.9f);
			yield return new WaitForSeconds( Random.Range(0.3f, 1.35f) );
		}
	}

	protected override  void Tick() {
		
	}
}
