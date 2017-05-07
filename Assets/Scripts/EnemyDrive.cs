using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyDrive : HoverCraftBase {

	private bool showLinesInSceneView=true;
	private float maxWhiskerRange = 100;
	private float aggressionRange = 500;
	private int obstacleLayer = 10;
	private float distanceToClosestObstacle;
	Vector3 closestObstacle;
	bool obstacleDanger = false;
	int emitterCycle = 0;

	private Transform forwardEmitter;
	private Transform foreRightEmitter;
	private Transform foreLeftEmitter;
	private Transform sideRightEmitter;
	private Transform sideLeftEmitter;
	private Transform rearLeftEmitter;
	private Transform rearRightEmitter;
	private Transform rearEmitter;

	public enum AIMode
	{
		FollowTrack,
		ShortTermOverride
	};
	private AIMode AInow = AIMode.ShortTermOverride;

	private static List<Transform> levelWayPointList;
	private static WayPointManager waypointManager;
	private int myWaypoint = -1;

	private static int uniqueID=0; // just to number at time of spawn for easier identification

	protected override void Init() {
		name = "Enemy#" + (uniqueID++);
		GameObject waypointMaster = GameObject.Find("AI_WayPoints");
		if(waypointMaster && levelWayPointList == null) {
			waypointManager = waypointMaster.GetComponent<WayPointManager>();
			levelWayPointList = new List<Transform>();
			for(int i=0;i<waypointMaster.transform.childCount;i++) {
				Transform wpTransform = waypointMaster.transform.GetChild(i);
				levelWayPointList.Add(wpTransform);
			}
		}

		if(levelWayPointList != null) {
			myWaypoint = Random.Range(0, levelWayPointList.Count);
			int nextWP = myWaypoint+1;
			if(waypointManager.isOrdered == false) {
				nextWP = Random.Range(0, levelWayPointList.Count);
			} else if(nextWP >= levelWayPointList.Count) {
				nextWP = 0;
			}
			// start ship at random spot between nearest waypoint and next (reduce collisions)
			transform.position =
				Vector3.Lerp(levelWayPointList[myWaypoint].position, levelWayPointList[nextWP].position, Random.Range(0.0f, 1.0f));
			// and point toward the next waypoint
			transform.LookAt(levelWayPointList[nextWP].position);
			myWaypoint = nextWP;

		}

		forwardEmitter = transform.FindChild("Raycast Emitters").FindChild("Forward Emitter");
		foreRightEmitter = transform.FindChild("Raycast Emitters").FindChild("Fore-Right Emitter");
		foreLeftEmitter = transform.FindChild("Raycast Emitters").FindChild("Fore-Left Emitter");
		sideRightEmitter = transform.FindChild("Raycast Emitters").FindChild("Side-Right Emitter");
		sideLeftEmitter = transform.FindChild("Raycast Emitters").FindChild("Side-Left Emitter");
		rearRightEmitter = transform.FindChild("Raycast Emitters").FindChild("Rear-Right Emitter");
		rearLeftEmitter = transform.FindChild("Raycast Emitters").FindChild("Rear-Left Emitter");
		rearEmitter = transform.FindChild("Raycast Emitters").FindChild("Rear Emitter");
		distanceToClosestObstacle = maxWhiskerRange;
		StartCoroutine(AIbehavior());
	}


	protected override void Tick()
	{
		if (obstacleDanger) { ShowDebugLines(transform.position, closestObstacle, Color.red); }

		FollowNextWaypoint();
	}

	void OnTriggerEnter(Collider collInfo) {
		HoverCraftBase hcbScript = collInfo.GetComponentInParent<HoverCraftBase>();
		if(hcbScript) {
			sprintRamming = true; // brief! will be overridden/forgotten on next AI update in 0.2-0.5 sec
			// Debug.Log(name + " attempting to ram " + collInfo.name);
		}
	}

	IEnumerator AIbehavior() {
		while (true) {

			// is there any track to try following? if so, default to try that
			// (avoidance functions below can override this until next AI rethinking)
			if(myWaypoint != -1) {
				AInow = AIMode.FollowTrack;
			}

			ResetDefaultDrivingControls();
			CheckForNearbyObstacles();
			AvoidNearbyObstacles();
			AdjustSpeedToAvoidObstacles();

			/*if(AInow == AIMode.ShortTermOverride) {
				Debug.Log("AI " + name + " is temporarily deviating from following track");
			}*/

			//StrikeHoverCars();								//see if there's a hovercar in front, and activate sprintRam if there is (currently not working)
			// FollowNextWaypoint(); // function called in Tick (higher freq than AIbehavior updates)

			//turnControl = Mathf.Clamp(turnControl, -1.0f, 1.0f);
			yield return new WaitForSeconds(Random.Range(0.20f,0.50f));
		}
	}

	
	private void ResetDefaultDrivingControls()
	{
		gasControl = 1.0f;
		turnControl = 0.0f;
		sprintRamming = false;
		if (!obstacleDanger) { distanceToClosestObstacle = Mathf.Infinity; closestObstacle = Vector3.zero; }
	}

	
	private void CheckForNearbyObstacles()
	{
		
		//emitterCycle = Random.Range(1,9);
		emitterCycle = emitterCycle + 1;
		if (emitterCycle > 8) { emitterCycle = 1; }
		if (emitterCycle == 1) { ActivateEmitter(forwardEmitter, maxWhiskerRange, obstacleLayer); }
		if (emitterCycle == 2) { ActivateEmitter(foreRightEmitter, maxWhiskerRange, obstacleLayer); }
		if (emitterCycle == 3) { ActivateEmitter(foreLeftEmitter, maxWhiskerRange, obstacleLayer); }
		if (emitterCycle == 4) { ActivateEmitter(sideRightEmitter, maxWhiskerRange, obstacleLayer); }
		if (emitterCycle == 5) { ActivateEmitter(sideLeftEmitter, maxWhiskerRange, obstacleLayer);}
		if (emitterCycle == 6) { ActivateEmitter(rearLeftEmitter, maxWhiskerRange, obstacleLayer);}
		if (emitterCycle == 7) { ActivateEmitter(rearRightEmitter, maxWhiskerRange, obstacleLayer);}
		if (emitterCycle == 8) { ActivateEmitter(rearEmitter, maxWhiskerRange, obstacleLayer);}
		
		
		/*
		ActivateEmitter(forwardEmitter, whiskerRange, obstacleLayer); 
		ActivateEmitter(foreRightEmitter, whiskerRange, obstacleLayer); 
		ActivateEmitter(foreLeftEmitter, whiskerRange, obstacleLayer); 
		ActivateEmitter(sideRightEmitter, whiskerRange, obstacleLayer); 
		ActivateEmitter(sideLeftEmitter, whiskerRange, obstacleLayer);
		ActivateEmitter(rearLeftEmitter, whiskerRange, obstacleLayer); 
		ActivateEmitter(rearRightEmitter, whiskerRange, obstacleLayer); 
		ActivateEmitter(rearEmitter, whiskerRange, obstacleLayer);
		*/

	}

	private void ActivateEmitter(Transform emitterLocation, float whiskerRange, int layerToSearchFor)
	{

		bool emitForwardBeams = false;
		bool emitLeftBeams = false;
		bool emitRightBeams = false;
		bool emitRearBeams = false;

		if (emitterLocation == forwardEmitter || emitterLocation == foreRightEmitter || emitterLocation == foreLeftEmitter)		{ emitForwardBeams = true; }
		if (emitterLocation == foreLeftEmitter || emitterLocation == sideLeftEmitter || emitterLocation == rearLeftEmitter)		{ emitLeftBeams = true; }
		if (emitterLocation == foreRightEmitter || emitterLocation == sideRightEmitter || emitterLocation == rearRightEmitter)	{ emitRightBeams = true; }
		if (emitterLocation == rearEmitter || emitterLocation == rearLeftEmitter || emitterLocation == rearRightEmitter)		{ emitRearBeams = true; }

		if (emitForwardBeams == true)							{ CheckEmissions (emitterLocation, whiskerRange, layerToSearchFor, transform.forward); }
		if (emitRearBeams == true)								{ CheckEmissions (emitterLocation, whiskerRange/4, layerToSearchFor, -transform.forward); }
		if (emitRightBeams == true)								{ CheckEmissions (emitterLocation, whiskerRange/2, layerToSearchFor, transform.right); }
		if (emitLeftBeams == true)								{ CheckEmissions (emitterLocation, whiskerRange/2, layerToSearchFor, -transform.right); }
		if (emitRightBeams == true && emitRearBeams == true)	{ CheckEmissions (emitterLocation, whiskerRange/2, layerToSearchFor, (transform.right - transform.forward).normalized); } 
		if (emitRightBeams == true && emitForwardBeams == true)	{ CheckEmissions (emitterLocation, whiskerRange/2, layerToSearchFor, (transform.right + transform.forward).normalized); }
		if (emitLeftBeams == true && emitForwardBeams == true)	{ CheckEmissions (emitterLocation, whiskerRange/2, layerToSearchFor, (-transform.right + transform.forward).normalized); }
		if (emitLeftBeams == true && emitRearBeams == true)		{ CheckEmissions (emitterLocation, whiskerRange/2, layerToSearchFor, (-transform.right - transform.forward).normalized); }
	
	}
		
	private void CheckEmissions( Transform emitterLocation, float beamRange, int layerToSearchFor, Vector3 emissionDirection)
	{
		RaycastHit hit;
		if (Physics.Raycast(emitterLocation.position, emissionDirection, out hit, beamRange, 1 << layerToSearchFor))
		{
			ShowDebugLines(emitterLocation.position, hit.point, Color.white);
			if (distanceToClosestObstacle > hit.distance)
			{
				obstacleDanger = true;
				distanceToClosestObstacle = hit.distance;
				closestObstacle = hit.point;
				ShowDebugLines(emitterLocation.position, hit.point, Color.yellow);
			}
		}
	}



	private void AdjustSpeedToAvoidObstacles()
	{
		RaycastHit hitFore;
		if (Physics.Raycast(forwardEmitter.position, transform.forward, out hitFore, maxWhiskerRange, 1 << obstacleLayer)) 
		{ 
			ShowDebugLines(forwardEmitter.position, hitFore.point, Color.blue);
			gasControl = Mathf.Clamp((hitFore.distance / maxWhiskerRange), 0.1f, 1.0f);
			AInow = AIMode.ShortTermOverride;
		}
		else { gasControl = 1.0f; }

	}



	private void ShowDebugLines(Vector3 emitterLoc, Vector3 hitPoint, Color color)
	{
		if (showLinesInSceneView)
		{
		Debug.DrawLine(emitterLoc, hitPoint, color);                //All debug lines are centralized here so we can turn this on and off by adjusting the bool
		}
	}


	private void AvoidNearbyObstacles()
	{
		if (obstacleDanger) 
		{
			distanceToClosestObstacle = Vector3.Distance(closestObstacle, transform.position);
			if (distanceToClosestObstacle > maxWhiskerRange) 
			{ 
				obstacleDanger = false; 
			}
		}
		if (obstacleDanger == false) { return; }

		Vector3 vectorAwayFromObstacle = transform.position - closestObstacle;
		Vector3 avoidancePoint = transform.position + vectorAwayFromObstacle;
		ShowDebugLines(transform.position, avoidancePoint, Color.green);		
		//float angleTowardAvoidanceVector = Vector3.Angle(vectorAwayFromObstacle, transform.forward);
		if (transform.InverseTransformPoint(avoidancePoint).x > 0.1f) {
			turnControl = 1f;
			AInow = AIMode.ShortTermOverride;
		} else if (transform.InverseTransformPoint(avoidancePoint).x < -0.1f) {
			turnControl = -1f;
			AInow = AIMode.ShortTermOverride;
		}
		
		//turnControl = Mathf.Clamp(angleTowardAvoidanceVector / 45f, -1f, 1f);
	}


	void StrikeHoverCars()		//this doesn't seem to be working at the moment
	{
		Vector3 ray58emitter = transform.position + (transform.forward * 5f);
		RaycastHit aggressionRay58hit;
		bool aggressionRay58 = Physics.Raycast(ray58emitter, transform.forward, out aggressionRay58hit, aggressionRange);
		
		if (aggressionRay58 && aggressionRay58hit.collider.gameObject.GetComponent<Destroyable>()) 
		{
			Debug.DrawLine(ray58emitter, aggressionRay58hit.point, Color.red);
			sprintRamming = true; 
		}
		else 
		{ 
			sprintRamming = false; 
		}
	}

	// helper function borrowed from https://forum.unity3d.com/threads/turn-left-or-right-to-face-a-point.22235/
	static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
		dirA = dirA - Vector3.Project(dirA, axis);
		dirB = dirB - Vector3.Project(dirB, axis);
		float angle = Vector3.Angle(dirA, dirB);
		return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
	}

	void FollowNextWaypoint()
	{ 
		if(myWaypoint == -1 || // no waypoints were found in level
			AInow != AIMode.FollowTrack) { // some other behavior is overriding control
			return; 
		}

		Vector3 gotoPoint = levelWayPointList[myWaypoint].position;
		gotoPoint.z = transform.position.z; // hack to ignore height diff
		float distTo = Vector3.Distance(transform.position, gotoPoint);
		float closeEnoughToWaypoint = 100.0f;
		if(distTo < closeEnoughToWaypoint) {
			if(waypointManager.isOrdered == false) {
				myWaypoint = Random.Range(0, levelWayPointList.Count);
			} else {
				myWaypoint++;
				if(myWaypoint >= levelWayPointList.Count) {
					myWaypoint = 0;
				}
			}
		}

		float turnAmt = AngleAroundAxis(transform.forward,
			levelWayPointList[myWaypoint].position - transform.position,Vector3.up);
		float angDeltaForGentleTurn = 10.0f;
		float angDeltaForSharpTurn = 30.0f;
		float gentleTurn = 0.5f;
		float sharpTurn = 1.0f;
		float gentleTurnEnginePower = 0.9f;
		float sharpTurnEnginePower = 0.6f;

		if(turnAmt < -angDeltaForSharpTurn) {
			turnControl = -sharpTurn;
			gasControl = sharpTurnEnginePower;
		} else if(turnAmt > angDeltaForSharpTurn) {
			turnControl = sharpTurn;
			gasControl = sharpTurnEnginePower;
		} else if(turnAmt < -angDeltaForGentleTurn) {
			turnControl = -gentleTurn;
			gasControl = gentleTurnEnginePower;
		} else if(turnAmt > angDeltaForGentleTurn) {
			turnControl = gentleTurn;
			gasControl = gentleTurnEnginePower;
		} else {
			turnControl = 0.0f;
			gasControl = 1.0f;
		}
		ShowDebugLines(transform.position, levelWayPointList[myWaypoint].position, Color.red);

	}

}
