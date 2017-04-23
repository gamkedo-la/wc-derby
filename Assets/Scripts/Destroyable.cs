using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour {
	public GameObject deathEffectGO;

	public void Destruction() {
		float scaleBy = 1.0f;
		PlayerDrive pdScript = GetComponent<PlayerDrive>();
		if(pdScript) {
			scaleBy = pdScript.shipScale;
		}
		EnemyDrive edScript = GetComponent<EnemyDrive>();
		if(edScript) {
			scaleBy = edScript.shipScale;
		}

		GameObject effectGO = GameObject.Instantiate(deathEffectGO, transform.position, Quaternion.identity);
		effectGO.transform.localScale *= scaleBy;
		if(GetComponentInChildren<Camera>() != null) {
			Camera.main.transform.SetParent(null);
		}
		Destroy(gameObject);
	}
}
