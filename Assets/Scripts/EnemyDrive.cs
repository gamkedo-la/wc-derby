using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrive : HoverCraftBase {
	protected override void Init () {
		StartCoroutine(ChangeDir());
	}

	IEnumerator ChangeDir() {
		while(true) {
			turnControl = Random.Range(0.0f, 1.0f) - Random.Range(0.0f, 1.0f);
			gasControl = Random.Range(0.6f, 0.9f);
			yield return new WaitForSeconds( Random.Range(0.5f, 3.0f) );
		}
	}

	protected override  void Tick() {
		
	}
}
