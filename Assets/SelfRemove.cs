using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRemove : MonoBehaviour {
	public float howLong;
	// Use this for initialization
	void Start () {
		Destroy(gameObject, howLong);
	}
}
