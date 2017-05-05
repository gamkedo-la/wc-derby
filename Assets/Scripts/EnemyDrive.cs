using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyDrive : HoverCraftBase {

	private float whiskerRange = 75;
	private float aggressionRange = 500;
	private int obstacleLayer = 10;

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
		StartCoroutine(AIbehavior());
	}


	protected override void Tick()
	{

	}

	IEnumerator AIbehavior() {
		while (true) {							
			ResetDefaultDrivingControls();
			AvoidObstacles();									//send out raycasts and adjust basic driving controls to avoid nearby obstacles
			//StrikeHoverCars();								//see if there's a hovercar in front, and activate springRam if there is (currently not working)
			FollowNextWaypoint();                              //steer toward a particular destination.  (Raycast to it to make sure the way is clear?) (not implemented)
			DecideNextWaypoint();                              // (not implemented)  (not quite sure what to do for this yet)

			turnControl = Mathf.Clamp(turnControl, -1.0f, 1.0f);
			yield return new WaitForSeconds(Random.Range(0.2f, 0.4f));
		}
	}

	
	private void ResetDefaultDrivingControls()
	{
		gasControl = 1.0f;
		turnControl = 0.0f;
		sprintRamming = false;
	}

	
	private void AvoidObstacles()
	{
		/*int randomEmitter = Random.Range(1, 9);
		if (randomEmitter == 1) { ActivateEmitter(forwardEmitter, whiskerRange, obstacleLayer); }
		if (randomEmitter == 2) { ActivateEmitter(foreRightEmitter, whiskerRange, obstacleLayer); }
		if (randomEmitter == 3) { ActivateEmitter(foreLeftEmitter, whiskerRange, obstacleLayer); }
		if (randomEmitter == 4) { ActivateEmitter(sideRightEmitter, whiskerRange, obstacleLayer); }
		if (randomEmitter == 5) { ActivateEmitter(sideLeftEmitter, whiskerRange, obstacleLayer);}
		if (randomEmitter == 6) { ActivateEmitter(rearLeftEmitter, whiskerRange, obstacleLayer);}
		if (randomEmitter == 7) { ActivateEmitter(rearRightEmitter, whiskerRange, obstacleLayer);}
		if (randomEmitter == 8) { ActivateEmitter(rearEmitter, whiskerRange, obstacleLayer);}
		*/

		ActivateEmitter(forwardEmitter, whiskerRange, obstacleLayer, 0.1f); 
		ActivateEmitter(foreRightEmitter, whiskerRange, obstacleLayer, 0.1f); 
		ActivateEmitter(foreLeftEmitter, whiskerRange, obstacleLayer, 0.1f); 
		ActivateEmitter(sideRightEmitter, whiskerRange, obstacleLayer, 0.1f); 
		ActivateEmitter(sideLeftEmitter, whiskerRange, obstacleLayer, 0.1f); 
		ActivateEmitter(rearLeftEmitter, whiskerRange, obstacleLayer, null); 
		ActivateEmitter(rearRightEmitter, whiskerRange, obstacleLayer, null); 
		ActivateEmitter(rearEmitter, whiskerRange, obstacleLayer, null); 
	}

	void ActivateEmitter(Transform emitterLocation, float whiskerRange, int layerToSearchFor, float? newSpeedIfHitDetected)
	{
		RaycastHit hit;
		bool forwardEmitterLoc = false;
		bool leftEmitterLoc = false;
		bool rightEmitterLoc = false;
		bool rearEmitterLoc = false;

		if (emitterLocation == forwardEmitter || emitterLocation == foreRightEmitter || emitterLocation == foreLeftEmitter) { forwardEmitterLoc = true; }
		if (emitterLocation == foreLeftEmitter || emitterLocation == sideLeftEmitter || emitterLocation == rearLeftEmitter) { leftEmitterLoc = true; }
		if (emitterLocation == foreRightEmitter || emitterLocation == sideRightEmitter || emitterLocation == rearRightEmitter) { rightEmitterLoc = true; }
		if (emitterLocation == rearEmitter || emitterLocation == rearLeftEmitter || emitterLocation == rearRightEmitter) { rearEmitterLoc = true; }

		
		if (forwardEmitterLoc == true) 
		{
			if (Physics.Raycast(emitterLocation.position, transform.forward, out hit, whiskerRange, 1 << layerToSearchFor))
			{ 
				ShowDebugLines(emitterLocation.position, hit.point);
				if (rightEmitterLoc) 
				{ 
					turnControl = turnControl - 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				} 
			 
				if (leftEmitterLoc) 
				{ 
					turnControl = turnControl + 0.8f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				} 
				if (!leftEmitterLoc && !rightEmitterLoc)
				{
					Vector3 vectorToOrigin = (Vector3.zero - transform.position);
					if (vectorToOrigin.x >= 0) 
					{ 
						turnControl = turnControl + 1f;
						if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
					}
					if (vectorToOrigin.x < 0) 
					{ 
						turnControl = turnControl - 1f;
						if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
					}
				}
			} 				
		}

		if (rearEmitterLoc == true) 
		{
			if (Physics.Raycast(emitterLocation.position, -transform.forward, out hit, whiskerRange, 1 << layerToSearchFor))
			{
				ShowDebugLines(emitterLocation.position, hit.point);
				if (rightEmitterLoc)
				{
					turnControl = turnControl - 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				}
				if (leftEmitterLoc)
				{
					turnControl = turnControl + 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				}
			}
		}

		if (leftEmitterLoc == true) 
		{
			if (Physics.Raycast(emitterLocation.position, -transform.right, out hit, whiskerRange, 1 << layerToSearchFor)) 
			{
				ShowDebugLines(emitterLocation.position, hit.point);
				turnControl = turnControl + 1f;
				if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
			}
		}

		if (rightEmitterLoc == true) 
		{
			if (Physics.Raycast(emitterLocation.position, transform.right, out hit, whiskerRange, 1 << layerToSearchFor)) 
			{
				ShowDebugLines(emitterLocation.position, hit.point);
				turnControl = turnControl - 1f;
				if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
			}
		}

		if (rightEmitterLoc == true && rearEmitterLoc == true) 
		{
				Vector3 diagnolRearRightDir = (transform.right - transform.forward).normalized;
				if (Physics.Raycast(emitterLocation.position, diagnolRearRightDir, out hit, whiskerRange, 1 << layerToSearchFor))
				{
					ShowDebugLines(emitterLocation.position, hit.point);
					turnControl = turnControl - 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				}
		}

		if (rightEmitterLoc == true && forwardEmitterLoc == true) 
		{
				Vector3 diagnolForwardRightDir = (transform.right + transform.forward).normalized;
				if (Physics.Raycast(emitterLocation.position, diagnolForwardRightDir, out hit, whiskerRange, 1 << layerToSearchFor))
				{
					ShowDebugLines(emitterLocation.position, hit.point);
					turnControl = turnControl - 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				}
		}

		if (leftEmitterLoc == true && forwardEmitterLoc == true) 
		{
				Vector3 diagnolForwardLeftDir = (-transform.right + transform.forward).normalized;
				if (Physics.Raycast(emitterLocation.position, diagnolForwardLeftDir, out hit, whiskerRange, 1 << layerToSearchFor))
				{
					ShowDebugLines(emitterLocation.position, hit.point);
					turnControl = turnControl + 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				}
		}

		if (leftEmitterLoc == true && rearEmitterLoc == true) 
		{
				Vector3 diagnolRearLeftDir = (-transform.right - transform.forward).normalized;
				if (Physics.Raycast(emitterLocation.position, diagnolRearLeftDir, out hit, whiskerRange, 1 << layerToSearchFor))
				{
					ShowDebugLines(emitterLocation.position, hit.point); 
					turnControl = turnControl + 1f;
					if (newSpeedIfHitDetected != null) { gasControl = (float)newSpeedIfHitDetected; }
				}
		}
	}
	
	private void ShowDebugLines(Vector3 emitterLocation, Vector3 hitPoint)
	{
		Debug.DrawLine(emitterLocation, hitPoint);	
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
