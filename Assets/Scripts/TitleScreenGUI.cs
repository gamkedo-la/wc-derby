// a very simple animated menu 
// used in titlescreen.unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenGUI : MonoBehaviour {

	void Start () {
		//Debug.Log("Starting Titlescreen...");
	}
	
	// Update is called once per frame
	//void Update () { }

	// menu button click events:

	public void guiHover(int whichOne) {
		AkSoundEngine.PostEvent ("Play_MenuBeep", gameObject);
		//Debug.Log("guiHover " + whichOne);
	}
	public void guiStartLevel(string levelName) {
		AkSoundEngine.PostEvent ("Play_MenuBeep", gameObject);
		//Debug.Log("guiPressLevel " + levelName);
		SceneManager.LoadScene(levelName);
	}
	public void guiPressBack() {
		AkSoundEngine.PostEvent ("Play_MenuBeep", gameObject);
		//Debug.Log("guiPressBack");
	}

}
