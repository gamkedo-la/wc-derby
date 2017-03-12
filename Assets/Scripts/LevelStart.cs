using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

public class LevelStart : MonoBehaviour {

	public void OpenStart(int thisScene) {
		if(GoldGoalTracker.allGoldTargets != null) {
			GoldGoalTracker.allGoldTargets = new List<GameObject>(); // dump outdated references
		}
		SceneManager.LoadScene(thisScene);
	}
}
