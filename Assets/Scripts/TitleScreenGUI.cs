// a very simple animated menu 
// used in titlescreen.unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenGUI : MonoBehaviour {

	private AudioSource guiAudio;

	void Start () {
		Debug.Log("Starting Titlescreen...");
		guiAudio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	//void Update () { }

	// menu button click events:

	public void guiHover(int whichOne) {
		Debug.Log("guiHover " + whichOne);
		guiAudio.Play();
	}
	public void guiPressStart() {
		Debug.Log("guiPressStart");
		SceneManager.LoadSceneAsync("MainPlay");
		guiAudio.Play();
	}
	public void guiPressHelp() {
		Debug.Log("guiPressHelp");
		guiAudio.Play();
	}
	public void guiPressCredits() {
		Debug.Log("guiPressCredits");
		guiAudio.Play();
	}
	public void guiPressLevel(int whichOne) {
		Debug.Log("guiPressLevel " + whichOne);
		guiAudio.Play();
	}
	public void guiPressBack() {
		Debug.Log("guiPressBack");
		guiAudio.Play();
	}
	public void guiPressConfig() {
		Debug.Log("guiPressConfig");
		guiAudio.Play();
	}

}
