using UnityEngine;
using System.Collections;

public class CannonBallImpulse : MonoBehaviour {
	int damage; // randomly given low value in init
	public float damageMult = 1.0f;
	// Use this for initialization
	void Start () {
		damage = (int)(Random.Range(1, 3) * damageMult);

		Rigidbody rb = GetComponent<Rigidbody>();
		rb.AddForce((Quaternion.AngleAxis(Random.Range(-4.0f,4.0f),Vector3.right)*
			Quaternion.AngleAxis(Random.Range(-4.0f,4.0f),Vector3.up)*
			transform.up) * 1000.0f);
		Destroy(gameObject, 7.0f); // just so they don't pile up forever
	}

	public void OnCollisionEnter(Collision hitFacts) {
		if(transform.childCount > 0) {
			Transform effectsChild = transform.GetChild(0);
			ParticleSystem psScript = effectsChild.GetComponent<ParticleSystem>();
			ParticleSystem.EmissionModule emitter = psScript.emission;
			emitter.enabled = true;
			psScript.Emit(200);
			Destroy(effectsChild.gameObject, psScript.startLifetime);
			effectsChild.parent = null;
			effectsChild.transform.position = hitFacts.contacts[0].point;
			Destroy(gameObject);

			if(hitFacts.collider.gameObject.layer == LayerMask.NameToLayer("GoldPrize")) {
				// GoldGoalTracker.AddPlayerGold(hitFacts.collider.gameObject);
				return; // unbreakable!
			}

			FallPiece fpScript = hitFacts.collider.GetComponent<FallPiece>();
			if(fpScript) {
				fpScript.BreakAndRelease(damage);
			} else {
				PotentialExploder peScript = hitFacts.collider.GetComponent<PotentialExploder>();
				if(peScript) {
					peScript.ApplyDamage(damage);
				}
			}
		}
	}
}
