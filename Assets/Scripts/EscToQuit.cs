using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscToQuit : MonoBehaviour {
	string titleSceneName = "titlescreen";

	void Update() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(SceneManager.GetActiveScene().name == titleSceneName) {
				Application.Quit();
			} else {
				SceneChange.ResetStaticsAndLoadScene("titlescreen");
			}
		}
	}
}
