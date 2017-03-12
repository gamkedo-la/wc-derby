using UnityEngine;
using System.Collections;

public class LightDieOff : MonoBehaviour {
	Light myLight;
	float totalDecayWait = 0.75f;

	float decayTimeLeft;
	float startRange;
	float startIntensity;
	// Use this for initialization
	void Start () {
		myLight = GetComponent<Light>();
		decayTimeLeft = totalDecayWait;
		startRange = myLight.range;
		startIntensity = myLight.intensity;
	}
	
	// Update is called once per frame
	void Update () {
		if(myLight.enabled) {
			decayTimeLeft -= Time.deltaTime;
			float powerPerc = decayTimeLeft / totalDecayWait;
			if(decayTimeLeft < 0.0f) {
				myLight.enabled = false;
			}
			myLight.range = startRange * powerPerc;
			myLight.intensity = startIntensity * powerPerc;
		}
	}
}
