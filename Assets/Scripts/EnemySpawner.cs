using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {
	public int maxCount = 3;
	public GameObject enemyPrefab;
	Transform SpawnFrom;
	public string fireSoundName;

	public float waitMin = 2.0f;
	public float waitMax = 4.0f;
	float reloadLeft = 0.0f;
	public List<GameObject> spawned;

	// Use this for initialization
	void Start () {
		spawned = new List<GameObject>();
		SpawnFrom = transform.Find("SpawnFrom");

		StartCoroutine(Spawn());
	}
		
	IEnumerator Spawn () {
		while(true) {
			yield return new WaitForSeconds( Random.Range(waitMin,waitMax) );

			spawned.RemoveAll(o => o == null);

			if(EndOfRoundMessage.instance.beenTriggered == false &&
				spawned.Count < maxCount) {
				if(fireSoundName.Length > 1) {
					SoundSet.PlayClipByName(fireSoundName, Random.Range(0.9f, 1.0f));
				}
				spawned.Add(GameObject.Instantiate(enemyPrefab, SpawnFrom.position, SpawnFrom.rotation)
					as GameObject);
			}
		}
	}

}
