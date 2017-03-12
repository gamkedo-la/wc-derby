using UnityEngine;
using System.Collections;

public class PushPulseMG : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine(gunShot());
	}
	
	IEnumerator gunShot() {
		while(true) {
			yield return new WaitForSeconds(0.25f);
			RaycastHit rhInfo;
			if(Physics.Raycast(transform.position, transform.forward, out rhInfo, 90.0f)) {
				Rigidbody rb = rhInfo.collider.GetComponent<Rigidbody>();
				if(rb) {
					rb.AddForceAtPosition(transform.forward * 3000.0f, rhInfo.point);
				}
			}
		}
	}
}
