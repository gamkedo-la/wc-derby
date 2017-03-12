using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndOfRoundMessage : MonoBehaviour {

	public GameObject panel;
	public bool isCombatMode = false;
	public bool beenTriggered = false;
	public static EndOfRoundMessage instance;
	public Text headline;
	public int hiveCount = 0;

	// Use this for initialization
	void Start () {
		instance = this;
		panel.SetActive(false);
	}

	IEnumerator WaitThenShowMessage(string endHeadline){
		Debug.Log ("Round is over but letting things fade out");
		yield return new WaitForSeconds (3.0f);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		headline.text = endHeadline;
		panel.SetActive(true);

	}

	public void DefeatedHive() {
		hiveCount--;
	}

	public void ResetScene(){
		Application.LoadLevel (Application.loadedLevelName);
	}

	public void OpenPanel(string endHeadline){
		if (beenTriggered == false) {
			StartCoroutine (WaitThenShowMessage (endHeadline));
			beenTriggered = true;
		}
	}
}
