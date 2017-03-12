using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CannonFire : MonoBehaviour {
	public KeyCode triggerKey;
	public GameObject spawnAttackPrefab;
	Transform fireFrom;
	public string fireSoundName;

	public bool autoFire = false;
	public float reloadDelay = 0.75f;
	float reloadLeft = 0.0f;

	public Material reloadMat;
	Light orbGlow;

	public Light shootFlash;

	private Text rechargeUI;
	private string baseReloadMsg;

	public GameObject startChargeEffect;
	public Transform chargeEffectLoc;
	public SpinBarrel spinner;
	public Transform fireParticleParent;
	private ParticleSystem[] fireParticles;

	IEnumerator strobeFireLight() {
		shootFlash.enabled = true;
		yield return new WaitForSeconds(reloadDelay * 0.3f);
		shootFlash.enabled = false;
	}

	IEnumerator fallOffLight() {
		shootFlash.enabled = true;
		shootFlash.intensity = 4.0f;
		while(shootFlash.intensity > 0.1f) {
			shootFlash.intensity -= 0.3f;
			yield return new WaitForSeconds(0.07f);
		}
		shootFlash.enabled = false;
	}

	// Use this for initialization
	void Start () {
		orbGlow = GetComponent<Light>();
		if(shootFlash) {
			shootFlash.enabled = false;
		}

		fireFrom = transform.Find("FireFrom");
		if(reloadMat) {
			reloadMat.color = Color.cyan;
			GameObject rechargeUIGO = GameObject.Find("FusionRecharge");
			if(rechargeUIGO) {
				rechargeUI = rechargeUIGO.GetComponent<Text>();
				baseReloadMsg = rechargeUI.text;
				rechargeUI.enabled = false;
			}
		}
		if(fireParticleParent) {
			fireParticles = fireParticleParent.GetComponentsInChildren<ParticleSystem>();
			for(int i = 0; i < fireParticles.Length; i++) {
				ParticleSystem.EmissionModule emitter = fireParticles[i].emission;
				emitter.enabled = false;
			}
		}
	}

	IEnumerator updateRechargeTime() {
		while(reloadLeft > 0.1f) {
			rechargeUI.text = baseReloadMsg + ": " + reloadLeft.ToString("F1");
			yield return new WaitForSeconds(0.1f);
		}
		rechargeUI.enabled = false;
	}

	IEnumerator delayedBlast() {
		if(startChargeEffect) {
			GameObject tempGO = GameObject.Instantiate(startChargeEffect, chargeEffectLoc.position, chargeEffectLoc.rotation) as GameObject;
			tempGO.transform.parent = chargeEffectLoc;
		}
		yield return new WaitForSeconds(2.0f);
		if(shootFlash) {
			StartCoroutine(fallOffLight());
		}
		if(ScreenShaker_Tank.instance) {
			ScreenShaker_Tank.instance.BlastShake(40.0f);
			ScreenShaker_Hover.instance.BlastShake(40.0f);
		}
		GameObject.Instantiate(spawnAttackPrefab, fireFrom.position, fireFrom.rotation);
	}

	void Shoot () {
		if(shootFlash) {
			StartCoroutine(strobeFireLight());
		}

		if(fireSoundName.Length > 1) {
			if(gameObject.layer == LayerMask.NameToLayer("Player")) {
				SoundSet.PlayClipByName(fireSoundName, Random.Range(0.9f, 1.0f));
			} else {
				SoundSet.PlayClipByName(fireSoundName, Random.Range(0.2f, 0.3f));
			}
		}
		GameObject.Instantiate(spawnAttackPrefab, fireFrom.position, fireFrom.rotation);
	}
	
	// Update is called once per frame
	void Update () {
		if(reloadLeft <= 0.0f && reloadMat) {
			reloadMat.color = Color.Lerp(Color.black, Color.cyan, 0.8f + 0.2f*Mathf.Cos(Time.timeSinceLevelLoad*3.0f));
		}
		if(orbGlow) {
			orbGlow.color = reloadMat.color;
			orbGlow.intensity = reloadMat.color.grayscale * 1.0f;
		}

		if(reloadLeft > 0.0f) {
			reloadLeft -= Time.deltaTime;
		}

		bool triggerNow;

		if(triggerKey == KeyCode.F15) {
			triggerNow = true;
		} else if(triggerKey == KeyCode.F14) {
			triggerNow = Random.Range(0,200)<3;
		} else if(triggerKey != KeyCode.None) {
			triggerNow = ((autoFire == false && Input.GetKeyDown(triggerKey)) ||
				(autoFire && Input.GetKey(triggerKey)));
		} else {
			triggerNow = ((autoFire == false && Input.GetMouseButtonDown(0)) ||
				(autoFire && Input.GetMouseButton(0)));
		}


		if(EndOfRoundMessage.instance.beenTriggered == false &&
		   triggerNow) {
			if(reloadLeft <= 0.0f) {
				reloadLeft += reloadDelay;
				if(reloadMat) {
					reloadMat.color = Color.black;
					rechargeUI.enabled = true;
					SoundSet.PlayClipByName("WCPowerUp", 1.0f);
					StartCoroutine(updateRechargeTime());
					StartCoroutine(delayedBlast());
				} else {
					if(spinner) {
						spinner.spinTorque(true);
						if(spinner.FastEnoughToFire()) {
							if(fireParticleParent) {
								for(int i = 0; i < fireParticles.Length; i++) {
									ParticleSystem.EmissionModule emitter = fireParticles[i].emission;
									emitter.enabled = true;
								}
							}
							Shoot();

							if(ScreenShaker.instance) {
								ScreenShaker.instance.BlastShake(0.2f);
							}
						}
					} else { // simpler when no spinning barrel
						Shoot();
					}
				}
			}
		} else {
			if(spinner) {
				spinner.spinTorque(false);
				if(fireParticleParent) {
					for(int i = 0; i < fireParticles.Length; i++) {
						ParticleSystem.EmissionModule emitter = fireParticles[i].emission;
						emitter.enabled = false;
					}
				}
			}
		}
	}
}
