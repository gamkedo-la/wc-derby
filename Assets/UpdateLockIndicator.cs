using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLockIndicator : MonoBehaviour {
	public Image lockBox;
	public ScatterSpawn allEnemies;
	public HoverCraftBase forCraft;

	// Use this for initialization
	void Start () {
		// lockFocus = GameObject.Find("domeMeasure").transform; // easy to use as stable target for UI testing
	}
	
	// Update is called once per frame
	void LateUpdate () {
		float distInFront;

		if(forCraft.sprintRamming == false) {
			forCraft.lockFocus = allEnemies.nearestAheadOf(transform);
		}

		if(forCraft.lockFocus != null) {
			distInFront = transform.InverseTransformPoint(forCraft.lockFocus.position).z;
		} else {
			distInFront = -100.0f;
		}

		if(distInFront > 0.0f) {
			lockBox.rectTransform.position = Camera.main.WorldToScreenPoint(forCraft.lockFocus.position);
			if(lockBox.enabled == false) {
				lockBox.enabled = true;
			}
		} else if(lockBox.enabled) {
			lockBox.enabled = false;
		}
	}
}
