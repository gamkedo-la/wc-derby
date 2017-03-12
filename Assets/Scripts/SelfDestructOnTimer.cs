using UnityEngine;
using System.Collections;

public class SelfDestructOnTimer : MonoBehaviour {
	public float lifeTime = 10.0f;
	// Use this for initialization
	void Start () {
		Destroy(gameObject, lifeTime);
	}	
}
