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
	public void guiStartLevel(string levelName) {
		Debug.Log("guiPressLevel " + levelName);
		guiAudio.Play();
		SceneManager.LoadSceneAsync(levelName);
	}
	public void guiPressBack() {
		Debug.Log("guiPressBack");
		guiAudio.Play();
	}

}
