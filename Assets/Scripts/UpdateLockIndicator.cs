using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLockIndicator : MonoBehaviour {
	public bool isPlayerSoUseUI = false;
	private Image lockBox;
	private ScatterSpawn allEnemies;
	private HoverCraftBase forCraft;

	// Use this for initialization
	void Awake () {
		if(isPlayerSoUseUI) {
			lockBox = GameObject.Find("RamLockIndicator").GetComponent<Image>();
			if(lockBox) {
				lockBox.enabled = false;
			}
			allEnemies = GameObject.Find("EnemySpawnerAndListMgmt").GetComponent<ScatterSpawn>();
		} else {
			allEnemies = GameObject.Find("PlayerSpawnerAndTargetLookup").GetComponent<ScatterSpawn>();
		}
	}

	public void TurnOff() {
		if(lockBox) {
			lockBox.enabled = false;
		}
		this.enabled = false;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if(forCraft == null) {
			if(isPlayerSoUseUI) {
				GameObject playerCraft = GameObject.Find("PlayerCraft(Clone)");
				if(playerCraft) {
					forCraft = (HoverCraftBase)playerCraft.GetComponent<PlayerDrive>();
				}
			}
			else {
				forCraft = (HoverCraftBase)GetComponent<EnemyDrive>();
			}
			if(forCraft == null) {
				return;
			}
		}

		float distInFront;

		if(forCraft.sprintRamming == false) {
			forCraft.lockFocus = allEnemies.nearestAheadOf(transform);
		}

		if(forCraft.lockFocus != null) {
			distInFront = transform.InverseTransformPoint(forCraft.lockFocus.transform.position).z;
		} else {
			distInFront = -100.0f;
		}

		if(isPlayerSoUseUI) {
			if(distInFront > 0.0f) {
				lockBox.rectTransform.position = Camera.main.WorldToScreenPoint(forCraft.lockFocus.transform.position);
				if(lockBox.enabled == false) {
					lockBox.enabled = true;
				}
			} else if(isPlayerSoUseUI && lockBox.enabled) {
				lockBox.enabled = false;
			}
		}
	}
}
