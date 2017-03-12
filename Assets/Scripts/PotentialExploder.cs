using UnityEngine;
using System.Collections;

public class PotentialExploder : MonoBehaviour {
	// explosion
	public float range = 8.0f;
	float power = 2500.0f;
	float upwardsPowerBoost = 3.0f;
	public GameObject fireParticles;
	public GameObject smokeParticles;
	public int damage = 10;

	public int hitPoints = 20;
	private int maxHP = 20;
	public bool hiveKingPiece = false;
	public bool onlySoundIfKing = false;

	int explodeLayer;

	void Start() {
		explodeLayer = LayerMask.NameToLayer("Explosive");
		maxHP = hitPoints;
	}

	public string HealthPerc() {
		int fakePerc = 100 * hitPoints / maxHP;
		if(fakePerc < 0) {
			fakePerc = 0;
		}
		return ""+fakePerc+"%";
	}

	public void ApplyDamage(int thisMany) {
		hitPoints -= thisMany;
		if(hitPoints < 0) {
			hitPoints = 0;
			BlastForce();
		}
	}

	public bool didBlast = false;
	public void BlastForce(bool doRemove=true) {
		if(didBlast) {
			return;
		}
		if(ScreenShaker.instance != null) {
			ScreenShaker.instance.BlastShake(damage);
		}
		if(hiveKingPiece) {
			EndOfRoundMessage.instance.DefeatedHive();
		}
		if(gameObject.layer == LayerMask.NameToLayer("Player") &&
			EndOfRoundMessage.instance.isCombatMode) {
			doRemove = false;
			Renderer[] allRend = gameObject.GetComponentsInChildren<Renderer>();
			for(int i = 0; i < allRend.Length; i++) {
				allRend[i].enabled = false;
			}
		}
		didBlast = true;
		if(onlySoundIfKing == false || hiveKingPiece) {
			SoundSet.PlayClipByName("Explosion with Metal Debris", Random.Range(0.4f, 0.7f));
		}
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, range);

		foreach (Collider hit in colliders) {
			Rigidbody rb = hit.GetComponent<Rigidbody>();
			if(rb != null) {
				rb.AddExplosionForce(power, explosionPos, range, upwardsPowerBoost);

				FallPiece fpScript = rb.GetComponent<FallPiece>();
				if(fpScript) {
					fpScript.BreakAndRelease(damage);
				} else {
					PotentialExploder peScript = rb.GetComponent<PotentialExploder>();
					if(peScript && peScript != this) {
						peScript.ApplyDamage(damage);
					}
				}

				/*if(rb.gameObject.layer == explodeLayer) {
					PotentialExploder pe = rb.gameObject.GetComponent<PotentialExploder>();
					if(pe) {
						pe.BlastForce();
					}
				}*/
			}
		}
		if (fireParticles) {
			GameObject fire = Instantiate (fireParticles, gameObject.transform.position, Quaternion.identity) as GameObject;
			Destroy (fire, 10);
		}
		if (smokeParticles) {
			GameObject smoke = Instantiate (smokeParticles, gameObject.transform.position, Quaternion.identity) as GameObject;
			Destroy (smoke, 10);
		}
		if(doRemove) {
			Destroy(gameObject);
		} else {
			StartCoroutine(clearReblast());
		}
	}

	IEnumerator clearReblast() {
		yield return new WaitForSeconds(0.2f);
		didBlast = false;
	}
}
