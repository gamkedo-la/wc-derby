using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraftBase : MonoBehaviour {
	static protected Vector3 domeCenter;
	static protected float domeRadius = 0.0f;

	public GameObject deathEffectGO;
	Vector3 momentum;
	public Transform bodyToTilt;

	private float timeSinceLastPuff = 0.0f;
	private float timeBetweenPuffs = 0.6f;

	public TrailRenderer[] trList;
	public Light[] engineLights;
	public bool boostDrawing = true;
	int ignoreVehicleLayerMask;

	protected float turnControl = 0.0f;
	protected float gasControl = 0.7f;

	private float timeSinceHookFired = 0.0f;

	private float percHookOut = 0.0f;

	LineRenderer cableHook;

	[HideInInspector]
	public bool sprintRamming = false;

	[HideInInspector]
	public HoverCraftBase lockFocus;
	private Vector3 endPt;

	private static GameObject snowPuffPfxPrefab;

	protected virtual void Init() {
		Debug.Log( gameObject.name + " is missing an Init override" );
	}
	protected virtual void Tick() {
		Debug.Log( gameObject.name + " is missing a Tick override" );
	}

	// Use this for initialization
	void Start () {
		if(snowPuffPfxPrefab == null) {
			snowPuffPfxPrefab = Resources.Load("SnowPuff") as GameObject;
		}
		cableHook = GetComponent<LineRenderer>();

		if(domeRadius == 0.0f) {
			GameObject theDome = GameObject.Find("domeMeasure");
			domeCenter = theDome.transform.position;
			domeRadius = theDome.transform.localScale.y * 0.5f;
		}

		momentum = Vector3.zero;
		ignoreVehicleLayerMask = ~LayerMask.GetMask("Player","Enemy");
		Init();
	}

	float heightUnderMe(Vector3 atPos) {
		float lookdownFromAboveHeight = 50.0f;
		RaycastHit rhInfo;
		if(Physics.Raycast(atPos+Vector3.up*lookdownFromAboveHeight,
			-Vector3.up*lookdownFromAboveHeight,out rhInfo,200.0f,ignoreVehicleLayerMask)) {
			return rhInfo.point.y;
		} else {
			return Terrain.activeTerrain.SampleHeight(atPos);
		}
	}

	public void Destruction() {
		GameObject.Instantiate(deathEffectGO, transform.position, Quaternion.identity);
		if(GetComponentInChildren<Camera>() != null) {
			Camera.main.transform.SetParent(null);
		}
		Destroy(gameObject);
	}

	protected bool HaveEnemyHooked() {
		return (sprintRamming && percHookOut >= 1.0f);
	}

	// Update is called once per frame
	void Update () {
		Tick();
		RaycastHit rhInfo;

		if(sprintRamming) {
			if(lockFocus == null) {
				sprintRamming = false;
			} else {
				Vector3 posDiff = (lockFocus.transform.position-transform.position);
				if(Physics.Raycast(transform.position+Vector3.up*2.0f, // give a little headroom
					posDiff,out rhInfo,posDiff.magnitude,ignoreVehicleLayerMask)) {
					lockFocus = null; // LOS blocked, break line
					sprintRamming = false;
				} else if(percHookOut < 1.0f) {
					float angFacingNow = Mathf.Atan2(transform.forward.z, transform.forward.x);
					Vector3 vectToFocus = lockFocus.transform.position - transform.position;
					float angFacingFocus = Mathf.Atan2(vectToFocus.z, vectToFocus.x);
					turnControl = Mathf.DeltaAngle(angFacingNow, angFacingFocus) * -3.0f;
				} else {
					Vector3 heightMatched = lockFocus.transform.position;
					heightMatched.y = transform.position.y;
					transform.LookAt(heightMatched);
					turnControl = 0.0f;
					float distTo = (lockFocus.transform.position - transform.position).magnitude;
					if(distTo < 14.0f) {
						lockFocus.SendMessage("Destruction");
						lockFocus = null;
						sprintRamming = false;
					}
				}
			}
		}

		transform.Rotate(Vector3.up, turnControl * 80.0f * Time.deltaTime);
		momentum *= 0.94f;
		momentum += transform.forward * bodyToTilt.forward.y * Time.deltaTime * -7.0f;
		momentum += transform.right * bodyToTilt.right.y * Time.deltaTime * -10.0f;

		float enginePower;

		float impendingCrashDetectionNormal = 1.0f;
		if(Physics.Raycast(transform.position,
			transform.forward, out rhInfo, 10.0f, ignoreVehicleLayerMask)) {
			//Debug.DrawLine(rhInfo.point, transform.position, Color.red);
			//Debug.DrawLine(rhInfo.point, rhInfo.point+rhInfo.normal*3.0f, Color.green);
			impendingCrashDetectionNormal = rhInfo.normal.y;
		}

		if(impendingCrashDetectionNormal < 0.1f ||
			OutOfDome(transform.position + transform.forward * gasControl * 10.0f)) {
			momentum *= 0.5f;
			enginePower = -1.0f;
			sprintRamming = false;
		} else {
			if( HaveEnemyHooked() ) {
				enginePower = 2.0f;
			} else {
				enginePower = gasControl;
			}
		}

		momentum += transform.forward * enginePower * 9.0f * Time.deltaTime;
		transform.position += momentum;

		boostDrawing = enginePower > 0.2f;

		float trailWidth = (Mathf.Max(enginePower, 0.0f)) * 0.5f;
		for(int i = 0; i < trList.Length; i++) {
			trList[i].widthMultiplier= trailWidth;
		}
		float engineLight = (Mathf.Max(enginePower, 0.0f)) * 2.0f;
		for(int i = 0; i < engineLights.Length; i++) {
			engineLights[i].intensity= engineLight;
		}

		float minHeightHere = heightUnderMe(transform.position)+0.9f;

		timeSinceLastPuff += Time.deltaTime;
		if(transform.position.y < minHeightHere) {
			if(timeSinceLastPuff > timeBetweenPuffs) {
				// Debug.Log("snow puffed by "+name);
				GameObject.Instantiate(snowPuffPfxPrefab, transform.position, Quaternion.identity, transform);
				timeSinceLastPuff = 0.0f;
			}
		}

		Vector3 newPos = transform.position;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;

		transform.position = ForceIntoDome(transform.position);
	}

	Vector3 ForceIntoDome(Vector3 whereAt) {
		Vector3 centerDelta = (whereAt - domeCenter);
		if(centerDelta.magnitude > domeRadius) {
			return domeCenter + domeRadius * centerDelta.normalized;
		} else {
			return whereAt;
		}
	}

	bool OutOfDome(Vector3 whereAt) {
		Vector3 centerDelta = (whereAt - domeCenter);
		return (centerDelta.magnitude > domeRadius);
	}

	void FixedUpdate() {
		float minHeightHere = heightUnderMe(transform.position)+0.9f;
		float goalHeightHere = minHeightHere + 1.7f;
		RaycastHit rhInfo;

		Vector3 newPos = transform.position;
		newPos.y += (goalHeightHere-newPos.y) * (goalHeightHere > newPos.y ? 3.0f : 1.0f ) * Time.deltaTime;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;

		float heightForward = 0.0f;

		heightForward = 
			Mathf.Max(
				heightUnderMe(transform.position + transform.forward * 10.0f),
				heightUnderMe(transform.position + transform.forward * 15.0f),
				heightUnderMe(transform.position + transform.forward * 25.0f));

		if(Physics.Raycast(transform.position,-Vector3.up*8.0f,out rhInfo)) {
			Vector3 pointAhead = transform.forward;
			if(heightForward != 0.0f) {
				pointAhead = transform.forward + Vector3.up * (heightForward - rhInfo.point.y)*0.1f;
			}
			bodyToTilt.transform.rotation = 
				Quaternion.Slerp(bodyToTilt.transform.rotation,
					Quaternion.LookRotation(pointAhead, rhInfo.normal),
					0.07f);
		}

		if(sprintRamming == false && cableHook.enabled) {
			timeSinceHookFired = 0.0f;
			percHookOut *= 0.65f;
			if(percHookOut < 0.03f) {
				cableHook.enabled = false;
			}
		}
	}

	void LateUpdate() {
		if(lockFocus != null && lockFocus.sprintRamming) { // break if sprinting
			lockFocus = null;
		}

		if(lockFocus == null) {
			sprintRamming = false;
		}

		if(sprintRamming) {
			timeSinceHookFired += Time.deltaTime;
			percHookOut = Mathf.Min(timeSinceHookFired*2.0f,1.0f);
			endPt = lockFocus.transform.position + Vector3.up * (-0.7f);
			if(cableHook.enabled == false) { 
				cableHook.enabled = true;
			}
		}

		if(cableHook.enabled) {
			cableHook.SetPosition(0, transform.position + Vector3.up*(-0.6f));
			Vector3 hookPt = transform.position * (1.0f - percHookOut) +
				endPt * percHookOut;
			cableHook.SetPosition(1, hookPt);
		}
	}
}
