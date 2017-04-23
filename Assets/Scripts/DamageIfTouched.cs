using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIfTouched : MonoBehaviour {
	void OnCollisionEnter(Collision collInfo) {
		Destroyable destScript = collInfo.collider.GetComponentInParent<Destroyable>();
		if(destScript) {
			destScript.Destruction();
		}
	}
}
