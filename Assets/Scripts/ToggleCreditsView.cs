using UnityEngine;
using System.Collections;

public class ToggleCreditsView : MonoBehaviour {
	public GameObject titleText;
	public GameObject creditsText;

	void Start () {
		creditsText.SetActive(false);
	}
	
	public void Toggle() {
		titleText.SetActive(creditsText.activeSelf);
		creditsText.SetActive(creditsText.activeSelf == false);
	}
}
