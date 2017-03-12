using UnityEngine;
using System.Collections;

public class TimeStretch : MonoBehaviour {
	bool skippedYet = false;
	// Use this for initialization
	void Start () {
		Time.timeScale = 1.0f;
	}

	public void RestoreTimeAndDestroy() {
		Time.timeScale = 1.0f;
		Destroy(this);
	}

	IEnumerator FastSkipThenRestoreTime() {
		Time.timeScale = 100.0f;
		yield return new WaitForSeconds(22.5f-Time.realtimeSinceStartup);
		Time.timeScale = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			if(skippedYet == false) {
				skippedYet = true;
				StartCoroutine(FastSkipThenRestoreTime());
			}
		}
	}
}
