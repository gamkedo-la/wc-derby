using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallPieceManager : MonoBehaviour {

	public List<FallPiece> pieces = new List<FallPiece>();
	public int baseLimbHitpoints = 200;
	public bool canBeDestroyed = false;
	// Use this for initialization

	public void RegisterPieceWithManager(FallPiece piece){
		pieces.Add (piece);
	}

	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
