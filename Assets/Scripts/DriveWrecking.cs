using UnityEngine;
using System.Collections;

public class DriveWrecking : MonoBehaviour {
	public bool isPiloting = true;
	public GameObject respawnEffect;
	public Material dangerLightMat;
	Light dangerLightGlow;

	public float panLong;
	public float panLat;

	private float panLongV;
	private float panLatV;
	private float velFalloff = 0.92f;
	AudioClip myDroneSnd;

	private float minHeight = 25.5f;
	private float maxHeight = 290.5f;

	private Vector3 startPos;
	private Quaternion startRot;

	// Use this for initialization
	void Start () {
		dangerLightGlow = GetComponent<Light>();
		SoundSet.PlayClipByName("DroneLoop", 1.0f, true);
		startPos = transform.position;
		startRot = transform.rotation;
	}

	// Update is called once per frame
	void Update () {

		float pulseLight = Mathf.Abs(Mathf.Cos( Time.realtimeSinceStartup * 4.0f ));
		pulseLight *= pulseLight;
		dangerLightMat.color = Color.Lerp(Color.black, Color.red, pulseLight);;
		dangerLightGlow.intensity = pulseLight * 10.0f;

		if(isPiloting == false && EndOfRoundMessage.instance.beenTriggered == false) {
			/*if(Input.GetKey(KeyCode.C)) {
				// transform.position += transform.forward * Time.deltaTime * 40.0f;
				CameraControlFocus ccf = Camera.main.transform.GetComponentInParent<CameraControlFocus>();
				ccf.enabled = false;

				Transform camPos = transform.Find("PilotCamPos");
				Camera.main.transform.position = camPos.position;
				Camera.main.transform.rotation = camPos.rotation;
				Camera.main.transform.parent = transform;

				for(int i = 0; i < Camera.main.transform.childCount; i++) {
					Camera hasCamTest = Camera.main.transform.GetChild(i).GetComponent<Camera>();
					if(hasCamTest == null) {
						Camera.main.transform.GetChild(i).gameObject.SetActive(false);
					}
				}
				isPiloting = true;
			}*/
		} else if(EndOfRoundMessage.instance.beenTriggered == false) {
			// panLongV -= Input.GetAxis("Mouse Y") * 2.0f * Time.deltaTime;
			panLatV += Input.GetAxis("Mouse X") * 3.0f * Time.deltaTime;

			panLong += panLongV;
			panLat += panLatV;

			transform.position += transform.right * Input.GetAxis("Horizontal") * 12.0f * Time.deltaTime;
			transform.position += transform.forward * Input.GetAxis("Vertical") * 25.0f * Time.deltaTime;
			transform.position += Vector3.up * Input.GetAxis("Mouse Y") * 2.0f * Time.deltaTime;

			Vector3 pos = transform.position;
			float terrainHereY = Terrain.activeTerrain.SampleHeight(transform.position);

			pos.y = terrainHereY+maxHeight;
			if(pos.y < transform.position.y) {
				transform.position = pos;
			}

			pos.y = terrainHereY+minHeight;
			if(pos.y > transform.position.y) {
				transform.position = pos;
			}

			panLong = Mathf.Clamp(panLong, -29.5f, 29.5f);

			transform.rotation = Quaternion.AngleAxis(panLat, Vector3.up)
				* Quaternion.AngleAxis(panLong, Vector3.right);
		}
	}

	void RestartPos() {
		PotentialExploder myExplo = GetComponent<PotentialExploder>();
		myExplo.BlastForce(false);
		transform.position = startPos;
		transform.rotation = startRot;
		if(ScreenShaker_Hover.instance) {
			ScreenShaker_Hover.instance.BlastShake(10.0f);
			ScreenShaker_Tank.instance.BlastShake(10.0f);
		}
		GameObject tempGO = GameObject.Instantiate(respawnEffect, transform.position, Quaternion.identity) as GameObject;
		Destroy(tempGO, 10.0f);
	}

	void OnCollisionEnter(Collision collFacts) {
		if(collFacts.collider.gameObject.layer == LayerMask.NameToLayer("BldgPart") ||
			collFacts.collider.gameObject.layer == LayerMask.NameToLayer("GoldPrize") ||
			collFacts.collider.gameObject.layer == LayerMask.NameToLayer("RobotParts") ||
			collFacts.collider.gameObject.layer == LayerMask.NameToLayer("RobotEatsShot")) {

			RestartPos();
		}
	}

	void OnTriggerEnter(Collider collWith) {
		if(collWith.gameObject.layer == LayerMask.NameToLayer("OnlyBlockPlayer")) {
			transform.position += collWith.transform.right * 6.0f;
		}
		if(collWith.gameObject.layer == LayerMask.NameToLayer("RobotParts")) {
			RestartPos();
		}
	}

	void FixedUpdate() {
		panLongV *= velFalloff;
		panLatV *= velFalloff;
	}
}
