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


	protected override void Init() {
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
	}

	

	IEnumerator AIbehavior() {
		while (true) {							
			ResetDefaultDrivingControls();
			CheckForNearbyObstacles();
			AvoidNearbyObstacles();
			AdjustSpeedToAvoidObstacles();
			//StrikeHoverCars();								//see if there's a hovercar in front, and activate springRam if there is (currently not working)
			//FollowNextWaypoint();                              //steer toward a particular destination.  (Raycast to it to make sure the way is clear?) (not implemented)
			//DecideNextWaypoint();                              // (not implemented)  (not quite sure what to do for this yet)

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
		if (transform.InverseTransformPoint(avoidancePoint).x > 0.1f) { turnControl = 1f; }
		else if (transform.InverseTransformPoint(avoidancePoint).x < -0.1f) { turnControl = -1f; }
		
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

	void FollowNextWaypoint()
	{ 
		//nothing implemented for this behavior yet
	}

	void DecideNextWaypoint()
	{ 
		//nothing implemented for htis behavior yet
		//I imagine will need an enum here to describe the AI's current objectives, like "hunting", "evading", or "baiting".
		//We can assign these objectives to different colored enemies or simply have the AI cycle between them.
	}

}
