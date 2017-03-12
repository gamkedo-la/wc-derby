using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateUpDownHover : MonoBehaviour {
	Vector3 parentRelStart;
	// Use this for initialization
	void Start () {
		parentRelStart = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = parentRelStart +
			Vector3.up * Mathf.Cos(Time.time) * 1.0f +
			Vector3.up * Mathf.Cos(Time.time*0.6f) * 0.7f;
	}
}
