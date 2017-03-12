using UnityEngine;
using System.Collections;

public class TintFog : MonoBehaviour {
	Color tintFrom;
	public Color tintTo;

	float densityFrom;
	float densityTo = 0.008f;

	float fadePerc;

	Light mainLight;
	Color lightTintFrom;

	ParticleSystem camDust;
	Color dustTintFrom;

	// Use this for initialization
	void Start () {
		camDust = (GameObject.Find("CamDust") as GameObject).GetComponent<ParticleSystem>();
		dustTintFrom = camDust.startColor;

		mainLight = (GameObject.Find("MainLight") as GameObject).GetComponent<Light>();
		lightTintFrom = mainLight.color;

		Camera.main.backgroundColor = tintTo;
		fadePerc = 0.0f;
		tintFrom = RenderSettings.fogColor;
		densityFrom = RenderSettings.fogDensity;
	}

	void Update() {
		fadePerc += Time.deltaTime * 2.0f;

		if(fadePerc > 1.0f) {
			fadePerc = 1.0f;
		}
		camDust.startColor = Color.Lerp(dustTintFrom,tintTo,fadePerc);
		mainLight.color = Color.Lerp(lightTintFrom,tintTo,fadePerc);
		RenderSettings.fogColor = Color.Lerp(tintFrom, tintTo, fadePerc);
		RenderSettings.fogDensity = Mathf.Lerp(densityFrom,densityTo,fadePerc);

		if(fadePerc > 1.0f) {
			enabled = false;
		}
	}
	
}
