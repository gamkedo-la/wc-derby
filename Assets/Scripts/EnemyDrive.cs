﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyDrive : HoverCraftBase {

	private bool pathIsClear = true;
	private bool showLinesInSceneView = true;
	private float obstacleSafetyThreshold;
	private Transform[] obstacles;
	private LevelAISettings levelAIAvoidanceManager;
	private float randomTurningDecisionMaker = 1f;
	public enum AIMode
	{
		FollowTrack,
		ShortTermOverride
	};
	private AIMode AInow = AIMode.FollowTrack;

	private static List<Transform> levelWayPointList;
	private static WayPointManager waypointManager;
	private int myWaypoint = -1;

	private bool isAttackingPlayer = false;

	private float attackSightRange = 300.0f;

	private static int uniqueID = 0; // just to number at time of spawn for easier identification

	public static void ResetStatics() {
		levelWayPointList = null;
		waypointManager = null;
		uniqueID = 0;
	}

	protected override void Init() {
		
		name = "Enemy#" + (uniqueID++);
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
			myWaypoint = Random.Range(0, levelWayPointList.Count);
			int nextWP = myWaypoint + 1;
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
		} else {
			myWaypoint = -1;
		}

		GameObject obstacleList = GameObject.FindGameObjectWithTag("ObstacleList");
		if(obstacleList) {
			levelAIAvoidanceManager = obstacleList.GetComponent<LevelAISettings>();
			if(levelAIAvoidanceManager) {
				obstacleSafetyThreshold = levelAIAvoidanceManager.AIdetectionRange;
				this.obstacles = levelAIAvoidanceManager.obstacles;
			}
		} else {
			levelAIAvoidanceManager = null;
			this.obstacles = null;
		}

		isAttackingPlayer = false;

		StartCoroutine(AIbehavior());
	}


	protected override void Tick()
	{
		if(isAttackingPlayer && PlayerDrive.instance) {
			SteerTowardPoint(PlayerDrive.instance.transform.position);
		} else if(waypointManager && waypointManager.isOrdered) {
			SteerTowardPoint(levelWayPointList[myWaypoint].position);
		}
	}

	void OnTriggerEnter(Collider collInfo) {
		HoverCraftBase hcbScript = collInfo.GetComponentInParent<HoverCraftBase>();
		if (hcbScript) {
			sprintRamming = true; // brief! will be overridden/forgotten on next AI update in 0.2-0.5 sec
								  // Debug.Log(name + " attempting to ram " + collInfo.name);
		}
	}

	IEnumerator AIbehavior() {
		while (true) {

			// is there any track to try following? if so, default to try that
			// (avoidance functions below can override this until next AI rethinking)
			if (myWaypoint != -1) {
				AInow = AIMode.FollowTrack;
			}
			if (Random.Range(1, 6) == 1) { randomTurningDecisionMaker = randomTurningDecisionMaker * -1; }
			ResetDefaultDrivingControls();
			Vector3 nextWaypoint = FollowNextWaypoint();
			Vector3 safetyPoint = AvoidObstacles();
			Vector3 pathToSteerToward = (safetyPoint - transform.position) + (nextWaypoint - transform.position);
			ShowDebugLines(transform.position, nextWaypoint, Color.yellow);
			ShowDebugLines(transform.position, safetyPoint, Color.blue);
			ShowDebugLines(transform.position, (transform.position + pathToSteerToward), Color.green);

			if(levelAIAvoidanceManager == null && // level doesn't have obstacles defined (currently hilly level)
				isAttackingPlayer == false && // enemy isn't already attacking player
				PlayerDrive.instance) { // player exists

				RaycastHit rhInfo;
				Vector3 vectorToPlayer = PlayerDrive.instance.transform.position - transform.position;
				if(vectorToPlayer.magnitude < attackSightRange) {
					Ray hereToPlayer = new Ray(transform.position, vectorToPlayer);
					if(Physics.Raycast(hereToPlayer, out rhInfo, ignoreVehicleLayerMask) == false) { // unobstructed
						isAttackingPlayer = true;
					}
				}
			}
			
			if (transform.InverseTransformPoint(safetyPoint).x < -0.5f) { turnControl = turnControl - 1f; }
			if (transform.InverseTransformPoint(safetyPoint).x > 0.5f) { turnControl = turnControl + 1f; }
			if (pathIsClear == false && turnControl < 0.1f && transform.InverseTransformPoint(safetyPoint).z < 0) { turnControl = randomTurningDecisionMaker; }
			if (pathIsClear == false && transform.InverseTransformPoint(safetyPoint).z < 0) { gasControl = 0.1f;} else { gasControl = 1f; }
			
			
			/*if(AInow == AIMode.ShortTermOverride) {
				Debug.Log("AI " + name + " is temporarily deviating from following track");
			}*/


			yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
		}
	}

	


	private void ResetDefaultDrivingControls()
	{
		gasControl = 0.5f;
		turnControl = 0.0f;
		sprintRamming = false;
	}



	/*
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
	*/



	private void ShowDebugLines(Vector3 startPoint, Vector3 endPoint, Color color)
	{
		if (showLinesInSceneView)
		{
		Debug.DrawLine(startPoint, endPoint, color);                //All debug lines are centralized here so we can turn this on and off by adjusting the bool
		}
	}

	private void OnDrawGizmos()
	{
		if (showLinesInSceneView)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, obstacleSafetyThreshold);
		}
	}

	/*												// We can read this list from the AI Level Manager now instead of doing it each time an enemy is spawned.
	private void ReadListOfObstacles()
	{
		int obstacleCount = -1;
		foreach (Transform child in listOfObstacles.transform)
		{
			if (child != listOfObstacles.transform)
			{ obstacleCount = obstacleCount + 1; }
		}
		obstacles = new Transform[obstacleCount+1];
		obstacleCount = -1;
		foreach (Transform child in listOfObstacles.transform)
		{
			if (child != listOfObstacles.transform) 
			{ 
				obstacleCount = obstacleCount + 1;
				obstacles[obstacleCount] = child; 
			}
		}
	}*/


	private Vector3 AvoidObstacles()
	{
		Vector3 vectorToDestination = Vector3.zero; //transform.forward * obstacleSafetyThreshold;
		Vector3 destinationPoint = transform.position + vectorToDestination;
		pathIsClear = true;
		if(obstacles != null) {
			foreach(Transform obstacle in obstacles) {
				float obstacleDistance = Vector3.Distance(transform.position, obstacle.position);
				if(obstacleDistance < obstacleSafetyThreshold) {
					pathIsClear = false;
					Vector3 vectorToObstacle = obstacle.position - transform.position;
					Vector3 obstaclePoint = transform.position + vectorToObstacle;
					ShowDebugLines(transform.position, obstaclePoint, Color.red);
					Vector3 dirAwayFromObstacle = -vectorToObstacle.normalized;
					Vector3 avoidancePoint = transform.position + (dirAwayFromObstacle * (obstacleSafetyThreshold - obstacleDistance));
					Vector3 vectorToAvoidancePoint = avoidancePoint - transform.position;
					Vector3 newDestinationVector = vectorToDestination + vectorToAvoidancePoint;
					vectorToDestination = newDestinationVector;
					destinationPoint = transform.position + vectorToDestination;
				}
			}
		}
		return destinationPoint;
	}
	
	
	

	// helper function borrowed from https://forum.unity3d.com/threads/turn-left-or-right-to-face-a-point.22235/
	static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
		dirA = dirA - Vector3.Project(dirA, axis);
		dirB = dirB - Vector3.Project(dirB, axis);
		float angle = Vector3.Angle(dirA, dirB);
		return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
	}
	
	Vector3 FollowNextWaypoint()
	{ // returns a Waypoint
		if(myWaypoint == -1 || // no waypoints were found in level
			AInow != AIMode.FollowTrack || // some other behavior is overriding control
			levelWayPointList == null) { // no waypoints defined  
			return Vector3.zero; 
		}

		Vector3 gotoPoint = levelWayPointList[myWaypoint].position;

		gotoPoint.y = transform.position.y; // hack to ignore height diff (earlier was erroneously using .z)
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
		return gotoPoint;
	}

	// currently only aims for waypoint in ordered track maps, but could also point to targeted craft, or generated destination
	void SteerTowardPoint(Vector3 driveToPt) {
		float turnAmt = AngleAroundAxis(transform.forward,
			driveToPt - transform.position,Vector3.up);
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
		ShowDebugLines(transform.position, driveToPt, Color.cyan);
	}
	
}
