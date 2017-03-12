using UnityEngine;
using System.Collections;

public class SelfDestructIfStationary : MonoBehaviour {
	Vector3 wasPos;
	int stillFrames = 10;
	// Use this for initialization
	void Start () {
		wasPos = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if( Vector3.Distance(wasPos, transform.position) < 0.01f) {
			stillFrames--;
			if(stillFrames < 0) {
				Destroy(gameObject);
			}
		}
		wasPos = transform.position;
	}
}
