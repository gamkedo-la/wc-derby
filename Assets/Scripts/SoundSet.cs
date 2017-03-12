using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundSet : MonoBehaviour {
	public static Dictionary<string, AudioClip> snds = new Dictionary<string, AudioClip>();

	public static AudioSource PlayClipByName(string sndName, float atVol = 1.0f, bool loopIt = false) {
		AudioClip clip;
		if(snds.ContainsKey(sndName)) {
			clip = snds[sndName];
		} else {
			snds[sndName] = Resources.Load(sndName, typeof(AudioClip)) as AudioClip;
			if(snds[sndName]) {
				clip = snds[sndName];
			} else {
				Debug.Log("SOUND RESOURCE NOT FOUND: "+ sndName);
				return null;
			}
		}

		GameObject tempGO = new GameObject("TempAudio"); // create the temp object
		tempGO.transform.position = Camera.main.transform.position;
		tempGO.transform.parent = Camera.main.transform;

		AudioSource aSource = tempGO.AddComponent<AudioSource>() as AudioSource; // add an audio source
		aSource.clip = clip; // define the clip
		aSource.volume = atVol;
		aSource.pitch = Random.Range(0.7f,1.4f);
		aSource.loop = loopIt;
		// set other aSource properties here, if desired
		aSource.Play(); // start the sound
		if(loopIt == false) {
			Destroy(tempGO, clip.length/aSource.pitch); // destroy object after clip duration
		}
		return aSource;
	}
}
