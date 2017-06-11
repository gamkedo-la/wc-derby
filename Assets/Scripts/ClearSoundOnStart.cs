using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSoundOnStart : MonoBehaviour {
	void Awake () {
		AkSoundEngine.StopAll();		
	}
}
