using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterSpawn : MonoBehaviour {
	public GameObject[] prefabList;
	public int howMany;

	private List<GameObject> spawnedList = new List<GameObject>();

	// Use this for initialization
	void Start () {
		GameObject theDome = GameObject.Find("domeMeasure");
		Vector3 domeCenter = theDome.transform.position;
		float domeRadius = theDome.transform.localScale.y * 0.5f;

		for(int i = 0; i < howMany; i++) {
			int randPick = Mathf.FloorToInt(Random.Range(0, prefabList.Length));
			float randAng = Random.Range(0.0f, 2.0f * Mathf.PI);
			float randScatterDist = Random.Range(0.0f, domeRadius);
			GameObject nextSpawned = GameObject.Instantiate(prefabList[randPick],
				domeCenter
				+ Vector3.right * Mathf.Cos(randAng)*randScatterDist
				+ Vector3.forward * Mathf.Sin(randAng)*randScatterDist, 
				Quaternion.AngleAxis(randAng+Mathf.PI,Vector3.up)); // point inward at first
			spawnedList.Add(nextSpawned);
		}
	}
}
