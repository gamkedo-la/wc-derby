using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroll : MonoBehaviour {
	Material myMat;
	// Use this for initialization
	void Start () {
		myMat = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		myMat.SetTextureOffset("_MainTex",Vector2.up * Time.time * 0.25f +
			Vector2.right * Time.time * 1.0f);
	}
}
