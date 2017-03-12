using UnityEngine;
using System.Collections;

public class VanishOnSpace : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy(gameObject, 15.0f);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			Destroy(this);
		}
	}
}
