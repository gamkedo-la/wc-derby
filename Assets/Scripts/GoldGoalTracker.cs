using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GoldGoalTracker : MonoBehaviour {
	public static int numberTotal = 0;
	public static int numberPlayerGot = 0;
	public static int numberEnemiesGot = 0;
	public static List<GameObject> allGoldTargets = new List<GameObject>();
	public static Text scoreToShow;

	// Use this for initialization
	void Start () {
		numberTotal = 0;
		numberPlayerGot = 0;
		numberEnemiesGot = 0;
		allGoldTargets = new List<GameObject>();
		scoreToShow = gameObject.GetComponent<Text>();
//		UpdateScoreText();
		if(scoreToShow) {
			scoreToShow.text = "";
		}
	}

	public static void AddPlayerGold(GameObject goldBox) {
		numberPlayerGot++;
		allGoldTargets.Remove (goldBox);
		Destroy(goldBox);
//		Debug.Log ("Player removed: count of gold targets: " + allGoldTargets.Count);
		UpdateScoreText();
	}

	public static void AddEnemyGold(GameObject goldBox) {
		numberEnemiesGot++;
		allGoldTargets.Remove (goldBox);
		Destroy(goldBox);
//		Debug.Log ("Enemy removed: count of gold targets: " + allGoldTargets.Count);
		UpdateScoreText();
	}

	public static void AddTargetGoldTalley(GameObject goldBox) {
		numberTotal++;
		goldBox.name = "Gold_" + numberTotal;
		allGoldTargets.Add (goldBox);
//		Debug.Log ("Added: count of gold targets: " + allGoldTargets.Count);
		UpdateScoreText();
	}

	public static GameObject NearestTargetToPoint(Vector3 toPoint){
		float closestDistanceFound = 1000000.0f;
		GameObject nearestMatch = null;
		foreach (GameObject goldGO in allGoldTargets) {
			float distToCompare = Vector3.Distance (toPoint, goldGO.transform.position);
			if (distToCompare < closestDistanceFound) {
				closestDistanceFound = distToCompare;
				nearestMatch = goldGO;
			}
		}
		return nearestMatch;
	}

	static void UpdateScoreText() {
		int blocksLeft = (numberTotal - numberEnemiesGot - numberPlayerGot);
		if(scoreToShow) {
			scoreToShow.text = ""+numberPlayerGot+" collected by you\n"+blocksLeft+" reactor"+(blocksLeft==1 ? "" : "s")+" remaining" 
				+ "\n" + numberEnemiesGot + " in enemy hands";
		}
		if (blocksLeft == 0) {
			string headline = "";
			if (numberPlayerGot > numberEnemiesGot) {
				headline = "You won! You collected more reactors than the Juggernaut!";
			} else if (numberEnemiesGot > numberPlayerGot) {
				headline = "You lost! The Juggernaut collected more reactors!";
			} else {
				headline = "It was a draw! Try to get more reactors next time!";
			}
			EndOfRoundMessage.instance.OpenPanel (headline);
		}
	}
}
