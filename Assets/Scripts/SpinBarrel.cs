using UnityEngine;
using System.Collections;

public class SpinBarrel : MonoBehaviour {
	float spinPower = 0.0f;
	float slowDownDecay = 0.98f;
	float minSpinToFire = 500.0f;
	bool spinningUp = false;

	// Use this for initialization
	void Start () {
		spinningUp = false;
	}
	
	// Update is called once per frame
	void Update () {
		/*if(spinPower > 10.0f) {
			Debug.Log(Mathf.Round(spinPower));
		}*/
		transform.Rotate(0.0f, 0.0f, -spinPower * Time.deltaTime);
	}

	public bool FastEnoughToFire() {
		return spinPower >= minSpinToFire;
	}

	public void spinTorque(bool powered) {
		spinningUp = powered;
	}

	void FixedUpdate() {
		if(spinningUp) {
			spinPower += 30.0f;
			float maxSpin = minSpinToFire;
			if(spinPower > maxSpin) {
				spinPower = maxSpin;
			}
		} else {
			spinPower *= slowDownDecay;
		}
	}
}
