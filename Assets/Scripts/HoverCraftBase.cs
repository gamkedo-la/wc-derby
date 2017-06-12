using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraftBase : MonoBehaviour {
	protected bool useCarCollisionTuning = true;

	static protected Vector3 domeCenter;
	static protected float domeRadius = 0.0f;
	Vector3 momentum;
	public Transform bodyToTilt;
	public GameObject shieldActiveMesh;

	public Vector3 bangBackMomentum = Vector3.zero;
	private float verticalVelocity = 0.0f;

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

	protected int maxHealth = 3;
	public int health = 3;
	protected bool hasShield = false;

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

	// waypoint info (used by player if constrained to track)
	public static List<Transform> levelWayPointList;
	public static WayPointManager waypointManager;
	protected Waypoint prevWaypoint = null;
	protected Waypoint myWaypoint = null;
	protected float myTrackLaneOffset = 0.0f;
	protected float percLeftToNextWP = 1.0f;
	protected float totalDistToNextWP = 0.0f;

	protected virtual void Init() {
		Debug.Log( gameObject.name + " is missing an Init override" );
	}
	protected virtual void Tick() {
		Debug.Log( gameObject.name + " is missing a Tick override" );
	}

	protected void randomizeTrackLaneOffset() {
		myTrackLaneOffset = Random.Range(-1.0f,1.0f);
	}

	public static void ResetStatic() {
		domeRadius = 0.0f;
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
		ignoreVehicleLayerMask = ~LayerMask.GetMask("Player","Enemy","Obstacle","Item");

		if(useCarCollisionTuning) {
			shipScale = 20.0f;
		}

		transform.localScale *= shipScale;

		Init();

		GameObject waypointMaster = GameObject.Find("AI_WayPoints");
		if (waypointMaster && waypointManager == null) {
			waypointManager = waypointMaster.GetComponent<WayPointManager>();
			levelWayPointList = new List<Transform>();
			for (int i = 0; i < waypointMaster.transform.childCount; i++) {
				Transform wpTransform = waypointMaster.transform.GetChild(i);
				levelWayPointList.Add(wpTransform);
			}
		}

		if(levelWayPointList != null) {
			int myWaypointIdx = Random.Range(0, levelWayPointList.Count);
			myWaypoint = levelWayPointList[ myWaypointIdx ].GetComponent<Waypoint>();
			Waypoint nextWP = myWaypoint.randNext();
			if(waypointManager.isOrdered == false) {
				nextWP = levelWayPointList[ Random.Range(0, levelWayPointList.Count) ].GetComponent<Waypoint>();
			}
			// start ship at random spot between nearest waypoint and next (reduce start collisions)
			transform.position =
				Vector3.Lerp(myWaypoint.transform.position,
					nextWP.transform.position, Random.Range(0.0f, 1.0f));
			transform.LookAt(nextWP.transform.position);
			randomizeTrackLaneOffset();
			totalDistToNextWP = Vector3.Distance(nextWP.trackPtForOffset(myTrackLaneOffset),
				myWaypoint.trackPtForOffset(myTrackLaneOffset));
			prevWaypoint = myWaypoint;
			myWaypoint = nextWP;
			percLeftToNextWP = 1.0f;
		} else {
			myWaypoint = null;
		}

		Light[] glowBulbs = GetComponentsInChildren<Light>();
		for(int i = 0; i < glowBulbs.Length; i++) {
			glowBulbs[i].range *= shipScale;
		}

		damage_smoke = transform.Find("damage_smoke").gameObject;
		damage_smoke.SetActive(false); // hide

		setShieldState(hasShield);
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

		if(waypointManager && waypointManager.isOrdered) {
			if(waypointManager.enforceTrackWalls) {
				Waypoint nextWP = null;
				for(int i = 0; i < HoverCraftBase.levelWayPointList.Count; i++) {
					Waypoint eachWP = HoverCraftBase.levelWayPointList[i].GetComponent<Waypoint>();
					nextWP = eachWP.pointIsAlong(transform.position);
					if(nextWP != null) {
						myWaypoint = nextWP;
						prevWaypoint = eachWP;
						break;
					}
				}
				Vector3 gotoPoint = myWaypoint.transform.position;
				Vector3 prevPoint = prevWaypoint.transform.position;

				gotoPoint.y = transform.position.y; // hack to ignore height diff (earlier was erroneously using .z)
				prevPoint.y = transform.position.y;
				Vector3 nearestPt = Vector3.Project(transform.position - prevPoint,
					(gotoPoint - prevPoint).normalized) +
					prevPoint;

				/*Vector3 showBallPt;
				if(Input.GetKey(KeyCode.Alpha1)) {
					showBallPt = gotoPoint;
				} else if(Input.GetKey(KeyCode.Alpha2)) {
					showBallPt = prevPoint;
				} else if(Input.GetKey(KeyCode.Alpha3)) {
					showBallPt = prevPoint + (gotoPoint - prevPoint).normalized * ((gotoPoint - prevPoint).magnitude * 0.5f);
				} else {
					showBallPt = nearestPt;
				}
				GameObject.Find("DebugBall").transform.position = showBallPt;*/

				float distTo = Vector3.Distance(nearestPt, gotoPoint);
				float distToPrev = Vector3.Distance(nearestPt, prevPoint);
				float totalDist = Vector3.Distance(gotoPoint, prevPoint);

				float widthHere = Mathf.Lerp(prevWaypoint.transform.localScale.x,
					myWaypoint.transform.localScale.x,
					distToPrev/totalDist) * 0.5f;
				float distFromPt = Vector3.Distance(transform.position, nearestPt);
				Debug.Log(widthHere);
				if(distFromPt > widthHere) {
					transform.position = nearestPt + widthHere * (transform.position - nearestPt).normalized;
				}
			}
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
		momentum += transform.right * bodyToTilt.right.y * Time.deltaTime * -40.0f;

		float impendingCrashDetectionNormal = 1.0f;
		/*if(Physics.Raycast(transform.position,											//Removed while working on AI
			transform.forward, out rhInfo, 10.0f*shipScale, ignoreVehicleLayerMask)) {
			//Debug.DrawLine(rhInfo.point, transform.position, Color.red);
			//Debug.DrawLine(rhInfo.point, rhInfo.point+rhInfo.normal*3.0f, Color.green);
			impendingCrashDetectionNormal = rhInfo.normal.y;
		}*/

		if (impendingCrashDetectionNormal < 0.1f
		//	|| OutOfDome(transform.position + transform.forward * gasControl * 10.0f)
		) 
		{

			momentum *= 0.5f;
			enginePower = -1.0f;
			sprintRamming = false;
		} else {
			if( (useCarCollisionTuning ? sprintRamming : HaveEnemyHooked()) ) {
				enginePower = ramBoostMult;
			} else {
				enginePower = gasControl;
			}																				//None of this will ever be called due to commenting out the part above
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
			return domeCenter + (domeRadius-3.0f) * centerDelta.normalized;
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
		verticalVelocity += (goalHeightHere > newPos.y ? 2.0f*(goalHeightHere-newPos.y) : -2.3f);
		newPos.y += verticalVelocity * Time.deltaTime;
		if(newPos.y < minHeightHere) {
			newPos.y = minHeightHere;
			if(verticalVelocity > 0.0f) {
				verticalVelocity *= -0.4f;
			}
		}
		transform.position = newPos;

		float heightForward = 0.0f;

		heightForward = 
			Mathf.Max(
				heightUnderMe(transform.position + transform.forward * 10.0f),
				heightUnderMe(transform.position + transform.forward * 15.0f),
				heightUnderMe(transform.position + transform.forward * 25.0f));

		if(Physics.Raycast(transform.position,-Vector3.up*13.0f*shipScale,out rhInfo, 13.0f*shipScale, ignoreVehicleLayerMask)) {
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

	protected void ChangeHealth(int healthChange) {
		health += healthChange;
		if(health > maxHealth) {
			health = maxHealth;
		}
		if(health < 0) {
			health = 0;
		}
		UpdateModelBasedOnHealth();
	}

	private void UpdateModelBasedOnHealth() {
		Transform modelTransform = GetComponentInChildren<Animator>().transform;

		modelTransform.Find("body__Back_Panel").gameObject.SetActive(health>2);
		modelTransform.Find("Gamkedo_Logo_top").gameObject.SetActive(health>2);
		modelTransform.Find("body__Top_Panel").gameObject.SetActive(health>2);
		modelTransform.Find("body__Right_Panel_x").gameObject.SetActive(health>2);
		modelTransform.Find("body__Left_Panel_x").gameObject.SetActive(health>2);
		modelTransform.Find("body__front_vent").gameObject.SetActive(health>2);
		damage_smoke.SetActive(health<3); // unhide the smoke particle system

		modelTransform.Find("blade").gameObject.SetActive(health>1);
		modelTransform.Find("body__rear_vent").gameObject.SetActive(health>1);
		modelTransform.Find("spring_left").gameObject.SetActive(health>1);
		modelTransform.Find("spring_right").gameObject.SetActive(health>1);

		if(health < 1) {
			gameObject.SendMessage("Destruction");
		}
	}

	protected void setShieldState(bool setTo) {
		hasShield = setTo;

		if(shieldActiveMesh) {
			shieldActiveMesh.SetActive(hasShield);
		}
	}

	void OnCollisionEnter(Collision collInfo) {
		HoverCraftBase hcbScript = collInfo.collider.GetComponentInParent<HoverCraftBase>();
		if(hcbScript) {
			float otherSpeed = hcbScript.totalActualSpeedNow;
			float selfSpeed = totalActualSpeedNow;
			if(selfSpeed > otherSpeed) {
				if(hcbScript.hasShield) {
					hcbScript.setShieldState(false);
				} else {
					hcbScript.ChangeHealth(-1);
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

	// helper function borrowed from https://forum.unity3d.com/threads/turn-left-or-right-to-face-a-point.22235/
	protected float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
		dirA = dirA - Vector3.Project(dirA, axis);
		dirB = dirB - Vector3.Project(dirB, axis);
		float angle = Vector3.Angle(dirA, dirB);
		return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
	}

}
