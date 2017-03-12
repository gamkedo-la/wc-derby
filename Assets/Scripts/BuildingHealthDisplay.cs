using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildingHealthDisplay : MonoBehaviour {
	public PotentialExploder Player;

	private bool roundSummaryShownYet = false;
	private Text display;

	private int skipFirstFrames = 3;

	// Use this for initialization
	void Start () {
		display = GetComponent<Text>();
	}

	string ShowIfAround(string baseName, PotentialExploder thisBldg) {
		if(thisBldg == null) {
			return baseName+" was lost!";
		} else {
			return baseName + ": " + thisBldg.HealthPerc();
		}
	}
	
	// Update is called once per frame
	void Update () {
		return; // TURN OFF FOR DEMO

		if(skipFirstFrames > 0) { // allows hive counter to populate
			skipFirstFrames--;
			return;
		}
		if(roundSummaryShownYet) {
			return;
		}

		int secSurvived =
			Mathf.RoundToInt(Time.realtimeSinceStartup);
		string timeSurvived="";
		if(secSurvived > 60) {
			int minutes = (int)(secSurvived / 60);
			timeSurvived += ""+minutes + ":";
			secSurvived -= minutes * 60;
		}
		if(secSurvived < 10) {
			timeSurvived += "0";
		}
		timeSurvived += ""+secSurvived;

		display.text = ShowIfAround("Player Health", Player) +"\n"+
			"HIVES REMAINING: "+EndOfRoundMessage.instance.hiveCount +"\n" +
			"Time taken: " + timeSurvived;

		if(Player.didBlast ||
			EndOfRoundMessage.instance.hiveCount <= 0) {
			roundSummaryShownYet = true;
			if(Player.didBlast) {
				EndOfRoundMessage.instance.OpenPanel("You lost, but survived for: " + timeSurvived);
			} else {
				EndOfRoundMessage.instance.OpenPanel("You won! All hives cleared in: " + timeSurvived);
			}
		}
	}
}
