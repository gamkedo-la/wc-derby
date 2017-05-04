using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraftBase : MonoBehaviour {
	protected bool useCarCollisionTuning = true;

	static protected Vector3 domeCenter;
	static protected float domeRadius = 0.0f;
	Vector3 momentum;
	public Transform bodyToTilt;

	public Vector3 bangBackMomentum = Vector3.zero;

	private float timeSinceLastPuff = 0.0f;
	private float timeBetweenPuffs = 0.6f;

	public TrailRenderer[] trList;
	public Light[] engineLights;
	public bool boostDrawing = true;
	public static int ignoreVehicleLayerMask;

	protected float turnControl = 0.0f;
	protected float gasControl = 0.7f;

	private float timeSinceHookFired = 0.0f;

	private float percHookOut = 0.0f;

	public int health = 3;

	LineRenderer cableHook;

	[HideInInspector]
	public bool sprintRamming = false;

	[HideInInspector]
	public HoverCraftBase lockFocus;
	private Vector3 endPt;

	private static GameObject hookSparkPfxPrefab;
	private static GameObject snowPuffPfxPrefab;
	private GameObject damage_smoke; // a particle system we turn on when damaged

	private Terrain theActiveTerrain;

	public float shipScale = 1.0f;
	protected Rigidbody rb;
	protected float enginePower = 0.0f;
	protected float ramBoostMult = 3.0f;
	public float totalActualSpeedNow = 0.0f;

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
		if(hookSparkPfxPrefab == null) {
			hookSparkPfxPrefab = Resources.Load("HookSpark") as GameObject;
		}

		rb = GetComponent<Rigidbody>();

		if (theActiveTerrain == null)
			theActiveTerrain = Terrain.activeTerrain;

		cableHook = GetComponent<LineRenderer>();

		if(domeRadius == 0.0f) {
			GameObject theDome = GameObject.Find("domeMeasure");
			domeCenter = theDome.transform.position;
			domeRadius = theDome.transform.localScale.y * 0.5f;
		}

		momentum = Vector3.zero;
		ignoreVehicleLayerMask = ~LayerMask.GetMask("Player","Enemy");

		if(useCarCollisionTuning) {
			shipScale = 20.0f;
		}

		Init();
		transform.localScale *= shipScale;

		Light[] glowBulbs = GetComponentsInChildren<Light>();
		for(int i = 0; i < glowBulbs.Length; i++) {
			glowBulbs[i].range *= shipScale;
		}

		damage_smoke = transform.Find("damage_smoke").gameObject;
		damage_smoke.SetActive(false); // hide
	}

	float heightUnderMe(Vector3 atPos) {
		float lookdownFromAboveHeight = 2.0f*shipScale;
		RaycastHit rhInfo;
		if(Physics.Raycast(atPos+Vector3.up*lookdownFromAboveHeight,
			-Vector3.up*lookdownFromAboveHeight,out rhInfo,8.0f*shipScale,ignoreVehicleLayerMask)) {
			return rhInfo.point.y;
		}
		else if (theActiveTerrain != null) {
			return theActiveTerrain.SampleHeight(atPos);
		}
		else {
			// there may be no terrain in the scene
			return lookdownFromAboveHeight; // nothing underneath us
		}
	}


	protected bool HaveEnemyHooked() {
		return (sprintRamming && percHookOut >= 1.0f);
	}

	// Update is called once per frame
	void Update () {
		Tick();
		float stunnedBangMagnitude = 4.0f;
		if(bangBackMomentum.magnitude > stunnedBangMagnitude) {
			turnControl = gasControl = 0.0f;
		}
		RaycastHit rhInfo;

		if(sprintRamming && useCarCollisionTuning == false) {
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

		float impendingCrashDetectionNormal = 1.0f;
		/*if(Physics.Raycast(transform.position,											//Removed while working on AI
			transform.forward, out rhInfo, 10.0f*shipScale, ignoreVehicleLayerMask)) {
			//Debug.DrawLine(rhInfo.point, transform.position, Color.red);
			//Debug.DrawLine(rhInfo.point, rhInfo.point+rhInfo.normal*3.0f, Color.green);
			impendingCrashDetectionNormal = rhInfo.normal.y;
		}*/

		if (impendingCrashDetectionNormal < 0.1f ||
			OutOfDome(transform.position + transform.forward * gasControl * 10.0f)) {
			momentum *= 0.5f;
			enginePower = -1.0f;
			sprintRamming = false;
		} else {
			if( (useCarCollisionTuning ? sprintRamming : HaveEnemyHooked()) ) {
				enginePower = ramBoostMult;
			} else {
				enginePower = gasControl;
			}
		}

		momentum += transform.forward * enginePower * 9.0f * Time.deltaTime;

		Vector3 newPos = transform.position;
		newPos += momentum;

		boostDrawing = enginePower > 0.2f;

		float trailWidth = (Mathf.Max(enginePower, 0.0f)) * 0.5f;
		for(int i = 0; i < trList.Length; i++) {
			trList[i].widthMultiplier= trailWidth;
		}
		float engineLight = (Mathf.Max(enginePower, 0.0f)) * 2.0f;
		for(int i = 0; i < engineLights.Length; i++) {
			engineLights[i].intensity= engineLight;
		}

		float minHeightHere = heightUnderMe(newPos)+0.9f;

		timeSinceLastPuff += Time.deltaTime;
		if(transform.position.y < minHeightHere) {
			if(timeSinceLastPuff > timeBetweenPuffs) {
				GameObject.Instantiate(snowPuffPfxPrefab, transform.position, Quaternion.identity, transform);
				timeSinceLastPuff = 0.0f;
			}
		}

		newPos.y = Mathf.Max(newPos.y, minHeightHere);

		newPos = ForceIntoDome(newPos);

		totalActualSpeedNow = ((newPos-transform.position).magnitude)/Time.deltaTime;

		transform.position = newPos;
	}

	public static Vector3 ForceIntoDome(Vector3 whereAt) {
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
		float minHeightHere = heightUnderMe(transform.position)+0.9f*shipScale;
		float goalHeightHere = minHeightHere + 1.7f;
		RaycastHit rhInfo;

		Vector3 newPos = transform.position;
		newPos += bangBackMomentum;
		bangBackMomentum *= 0.9f;
		newPos.y += (goalHeightHere-newPos.y) * (goalHeightHere > newPos.y ? 3.0f : 1.0f ) * Time.deltaTime;
		newPos.y = Mathf.Max(newPos.y, minHeightHere);
		transform.position = newPos;

		float heightForward = 0.0f;

		heightForward = 
			Mathf.Max(
				heightUnderMe(transform.position + transform.forward * 10.0f),
				heightUnderMe(transform.position + transform.forward * 15.0f),
				heightUnderMe(transform.position + transform.forward * 25.0f));

		if(Physics.Raycast(transform.position,-Vector3.up*8.0f*shipScale,out rhInfo, 8.0f*shipScale, ignoreVehicleLayerMask)) {
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
		if(useCarCollisionTuning) {
			return;
		}

		if(lockFocus != null && lockFocus.sprintRamming) { // break if sprinting
			lockFocus = null;
		}

		if(lockFocus == null) {
			sprintRamming = false;
		}

		if(sprintRamming) {
			timeSinceHookFired += Time.deltaTime;
			endPt = lockFocus.transform.position + Vector3.up * (-0.7f);

			float wasPercHookOut = percHookOut;
			percHookOut = Mathf.Min(timeSinceHookFired*2.0f,1.0f);
			if(wasPercHookOut < 1.0f && percHookOut >= 1.0f) {
				Vector3 posDiffNorm = (lockFocus.transform.position-transform.position).normalized;
				Vector3 sparkPt = endPt - posDiffNorm * 1.5f;
				GameObject.Instantiate(hookSparkPfxPrefab, sparkPt, 
					Quaternion.LookRotation(posDiffNorm), lockFocus.transform);
			}
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

	void OnCollisionEnter(Collision collInfo) {
		HoverCraftBase hcbScript = collInfo.collider.GetComponentInParent<HoverCraftBase>();
		if(hcbScript) {
			float otherSpeed = hcbScript.totalActualSpeedNow;
			float selfSpeed = totalActualSpeedNow;
			if(selfSpeed > otherSpeed) {
				hcbScript.health--;
				Transform modelTransform = hcbScript.GetComponentInChildren<Animator>().transform;
				switch(hcbScript.health) {
				case 2:
					modelTransform.Find("body__Back_Panel").gameObject.SetActive(false);
					modelTransform.Find("Gamkedo_Logo_top").gameObject.SetActive(false);
					modelTransform.Find("body__Top_Panel").gameObject.SetActive(false);
					modelTransform.Find("body__Right_Panel_x").gameObject.SetActive(false);
					modelTransform.Find("body__Left_Panel_x").gameObject.SetActive(false);
					modelTransform.Find("body__front_vent").gameObject.SetActive(false);
					damage_smoke.SetActive(true); // unhide the smoke particle system
					break;
				case 1:
					modelTransform.Find("blade").gameObject.SetActive(false);
					modelTransform.Find("body__rear_vent").gameObject.SetActive(false);
					modelTransform.Find("spring_left").gameObject.SetActive(false);
					modelTransform.Find("spring_right").gameObject.SetActive(false);
					break;
				case 0:
				default:
					hcbScript.gameObject.SendMessage("Destruction");
					break;
				}
				Vector3 pushVect = hcbScript.bodyToTilt.transform.position -
					bodyToTilt.transform.position;
				float bangBackPowerWinner = 10.0f;
				float bangBackPowerDamaged = 20.0f;
				float bangBackAngleMax = 60.0f;
				pushVect = pushVect.normalized;
				hcbScript.bangBackMomentum = pushVect * bangBackPowerDamaged;
				bangBackMomentum = -pushVect * bangBackPowerWinner;
				hcbScript.bodyToTilt.Rotate(Random.insideUnitCircle * bangBackAngleMax);
				hcbScript.sprintRamming = false;
				bodyToTilt.Rotate(Random.insideUnitCircle * bangBackAngleMax);
				sprintRamming = false;

				GameObject sparkGO = GameObject.Instantiate(hookSparkPfxPrefab, 
					(transform.position+hcbScript.transform.position)*0.5f,
					Quaternion.identity);
				sparkGO.transform.localScale *= 2.0f;
				GameObject snowGO = GameObject.Instantiate(snowPuffPfxPrefab, 
					(transform.position+hcbScript.transform.position)*0.5f,
					Quaternion.identity);
				snowGO.transform.localScale *= 3.35f;
				/*
				pushVect = Quaternion.AngleAxis(90.0f, Vector3.up) * pushVect;
				hcbScript.bodyToTilt.Rotate(pushVect,70.0f);
				bodyToTilt.Rotate(pushVect,-70.0f);*/
			}
		}
	}
}
