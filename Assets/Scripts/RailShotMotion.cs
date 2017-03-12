using UnityEngine;
using System.Collections;

public class RailShotMotion : MonoBehaviour {
	Rigidbody rb;

	public int damage = 40;

	// movement
	float maxSpeed = 2000.0f;

	public static bool isStopShotLayer(int testLayer) {
		return testLayer == LayerMask.NameToLayer("Terrain") ||
			testLayer == LayerMask.NameToLayer("RobotEatsShot");
	}

	void Start () {
		rb = GetComponent<Rigidbody>();
		int layerToIgnore = LayerMask.GetMask("Player");
		RaycastHit[] allHits = Physics.RaycastAll(transform.position, transform.up,
		                                          layerToIgnore);
		int layerToMatchBlast = LayerMask.NameToLayer("Explosive");
		for(int i = 0; i < allHits.Length; i++) {
			if( isStopShotLayer(allHits[i].collider.gameObject.layer) == false) {
				FallPiece fpScript = allHits[i].collider.GetComponent<FallPiece>();
				if(fpScript) {
					fpScript.BreakAndRelease(damage);
				} else {
					if(allHits[i].collider.gameObject.layer == LayerMask.NameToLayer("GoldPrize")) {
						// GoldGoalTracker.AddPlayerGold(allHits[i].collider.gameObject);
						// unbreakable!
					} else {
						Destroy(allHits[i].collider.gameObject);
					}
				}
			}
			if(allHits[i].collider.gameObject.layer == layerToMatchBlast) {
				PotentialExploder pe = allHits[i].collider.gameObject.GetComponent<PotentialExploder>();
				if(pe) {
					pe.BlastForce();
				}
			}
		}

		Destroy(gameObject, 6.0f);
	}

	void Update () {
		rb.velocity = transform.up * maxSpeed;
	}

	public void OnCollisionEnter(Collision hitFacts) {
		if (hitFacts.collider.gameObject.layer == LayerMask.NameToLayer ("Player")) {
			return;
		}
		if(transform.childCount > 0) {
			if( isStopShotLayer(hitFacts.collider.gameObject.layer) == false) {
				Destroy(hitFacts.collider.gameObject);
			}

			Transform effectsChild = transform.GetChild(0);
			TrailRenderer trScript = effectsChild.GetComponent<TrailRenderer>();
			Destroy(effectsChild.gameObject, trScript.time);
			effectsChild.parent = null;
			Destroy(gameObject);
		}
	}
}
