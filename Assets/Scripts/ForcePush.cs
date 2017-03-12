using UnityEngine;
using System.Collections;

public class ForcePush : MonoBehaviour {

	// Use this for initialization
	void Start () {
		float blastRad = 40.0f;
		Collider[] list = Physics.OverlapSphere(transform.position, blastRad);
		int playerLayer = LayerMask.NameToLayer("Player");
		int goldLayer = LayerMask.NameToLayer("GoldPrize");

		for(int i = 0; i < list.Length; i++) {
			if(list[i].gameObject.layer == goldLayer) {
				PotentialExploder peScript = list[i].gameObject.GetComponent<PotentialExploder>();
				if(peScript) {
					peScript.BlastForce(false);
				}
				GoldGoalTracker.AddPlayerGold(list[i].gameObject);
			} else {
				Rigidbody rb = list[i].GetComponent<Rigidbody>();
				if(rb && list[i].gameObject.layer != playerLayer) {
					rb.AddExplosionForce(2000.0f, transform.position, blastRad);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
