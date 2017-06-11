using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour {
	public static void ResetStaticsAndLoadScene(string sceneName) {
		EnemyDrive.ResetStatics();
		HoverCraftBase.ResetStatic();
		SceneManager.LoadScene(sceneName);
	}
}
