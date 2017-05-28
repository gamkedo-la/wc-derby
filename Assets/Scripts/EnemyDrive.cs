using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrive : HoverCraftBase {

	private const float maxHandlingTurnAngle = 80f;
	private bool pathIsClear = true;
	private bool showLinesInSceneView = true;
	private float obstacleSafetyThreshold;
	private Transform[] obstacles;
	private LevelAISettings levelAIAvoidanceManager;
	private float randomTurningDecisionMaker = 1f;
	[SerializeField] private GameObject headlights;  //assigned in inspector

	public enum AIMode
	{
		FollowTrack,
		ShortTermOverride
	};
	private AIMode AInow = AIMode.FollowTrack;

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
			SteerTowardPoint(myWaypoint.trackPtForOffset(myTrackLaneOffset));

			// override if going outside track
			if(waypointManager.enforceTrackWalls) {
				Vector3 nextWPTrackLeft = myWaypoint.trackPtForOffset(-1.0f);
				Vector3 nextWPTrackRight = myWaypoint.trackPtForOffset(1.0f);

				Vector3 prevWPTrackLeft = prevWaypoint.trackPtForOffset(-1.0f);
				Vector3 prevWPTrackRight = prevWaypoint.trackPtForOffset(1.0f);

				Vector3 positionLeft = Vector3.Lerp(nextWPTrackLeft, prevWPTrackLeft, percLeftToNextWP);
				Vector3 positionRight = Vector3.Lerp(nextWPTrackRight, prevWPTrackRight, percLeftToNextWP);

				float angleFromLeftEdge = AngleAroundAxis(transform.position - prevWPTrackLeft,
					nextWPTrackLeft - prevWPTrackLeft,Vector3.up);
				if(angleFromLeftEdge > 0.0f) {
					SteerTowardPoint(positionLeft);
				}
				float angleFromRightEdge = AngleAroundAxis(transform.position - prevWPTrackRight,
					nextWPTrackRight - prevWPTrackRight,Vector3.up);
				if(angleFromRightEdge < 0.0f) {
					SteerTowardPoint(positionRight);
				}				
			}
		}

		
		
	}

	void OnTriggerEnter(Collider collInfo) {
		HoverCraftBase hcbScript = collInfo.GetComponentInParent<HoverCraftBase>();
		if (hcbScript) {
			sprintRamming = true; // brief! will be overridden/forgotten on next AI update in 0.2-0.5 sec
								  // Debug.Log(name + " attempting to ram " + collInfo.name);
		}
	}

	void ShineHeadlights (bool headlightStatus)
	{
		if (headlights && headlights.activeSelf != headlightStatus)
		{
			headlights.SetActive(headlightStatus);
		}
	}

	IEnumerator AIbehavior() {
		while (true) {

			// is there any track to try following? if so, default to try that
			// (avoidance functions below can override this until next AI rethinking)
			if (myWaypoint != null) {
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

			float rightTurnAmount = Vector3.Angle(pathToSteerToward, transform.forward);
			rightTurnAmount = rightTurnAmount / 80;
			rightTurnAmount = Mathf.Clamp(rightTurnAmount, 0, maxHandlingTurnAngle);
			float leftTurnAmount = -rightTurnAmount;
			if (pathToSteerToward.x < -0.001f) { turnControl = turnControl - leftTurnAmount; }
			if (pathToSteerToward.x > 0.001f) { turnControl = turnControl + rightTurnAmount; }
			if (pathIsClear == false && turnControl < 0.001f && pathToSteerToward.z < 0) { turnControl = randomTurningDecisionMaker; }
			if (pathIsClear == false && pathToSteerToward.z < 0) { gasControl = 0.1f; } else { gasControl = 1f; }

			ShineHeadlights(isAttackingPlayer);

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
	
	Vector3 FollowNextWaypoint()
	{ // returns a Waypoint
		if(myWaypoint == null || // no waypoints were found in level
			AInow != AIMode.FollowTrack || // some other behavior is overriding control
			levelWayPointList == null) { // no waypoints defined  
			return Vector3.zero; 
		}

		Vector3 gotoPoint = myWaypoint.trackPtForOffset(myTrackLaneOffset);

		gotoPoint.y = transform.position.y; // hack to ignore height diff (earlier was erroneously using .z)
		float distTo = Vector3.Distance(transform.position, gotoPoint);
		float closeEnoughToWaypoint = 140.0f;
		percLeftToNextWP = distTo / totalDistToNextWP;

		if(distTo < closeEnoughToWaypoint) {
			if(waypointManager.isOrdered == false) {
				myWaypoint = levelWayPointList[Random.Range(0, levelWayPointList.Count)].GetComponent<Waypoint>();
			} else {
				prevWaypoint = myWaypoint;
				myWaypoint = myWaypoint.randNext();
				randomizeTrackLaneOffset();
				totalDistToNextWP = Vector3.Distance(transform.position, myWaypoint.trackPtForOffset(myTrackLaneOffset));
				percLeftToNextWP = 1.0f;
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
