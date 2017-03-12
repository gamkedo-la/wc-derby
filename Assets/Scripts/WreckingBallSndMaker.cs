using UnityEngine;
using System.Collections;

public class WreckingBallSndMaker : MonoBehaviour {
	float timeBetween = 0.0f;

	void OnCollisionEnter(Collision collFacts) {
		if(timeBetween > 0.0f) {
			return;
		}
		float hitForce = collFacts.relativeVelocity.magnitude;
		if(hitForce > 7.0f) {
			if(ScreenShaker_Hover.instance) {
				ScreenShaker_Hover.instance.BlastShake(hitForce * 1.4f);
				ScreenShaker_Tank.instance.BlastShake(hitForce * 0.9f);
			}
			float hitVol = (hitForce - 7.0f)/10.0f;
			hitVol = Mathf.Min(hitVol, 1.0f);
			SoundSet.PlayClipByName("BallImpact", hitVol);
			SoundSet.PlayClipByName("SmashBuilding", hitVol);
			timeBetween = Random.Range(0.25f,0.35f);
		}
	}

	void Update() {
		if(timeBetween > 0.0f) {
			timeBetween -= Time.deltaTime;
		}
	}
}
