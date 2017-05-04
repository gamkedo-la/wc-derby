using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyDrive : HoverCraftBase {

	private float navSensorRange = 200f;	//maximum range of longest-range forward navigation sensors
	

	protected override void Init () {
		StartCoroutine(ChangeDir()); 
	}

	IEnumerator ChangeDir() {
		while(true) {

			gasControl = 1.0f;												//gasControl is reset
			turnControl = 0.0f;	gasControl = 1.0f;                          //turnControl is reset
			AvoidObstacles();                                               //turnControl is modified and speed may be adjusted. Lots of raycasts used.
			StrikeHoverCars();												//see if there's a hovercar in front, and activate springRam if there is (currently not working)
			FollowNextWaypoint();                                           //steer toward a particular destination.  (Raycast to it to make sure the way is clear?) (not implemented)
			DecideNextWaypoint();											// (not implemented)  (not quite sure what to do for this yet)
			
			turnControl = Mathf.Clamp(turnControl,-1.0f, 1.0f);				
			yield return new WaitForSeconds( Random.Range(0.1f, 0.2f) );
		}
	}

	


	protected override  void Tick() {
		
	}

	private void AvoidObstacles()
	{
		// Send out raycasts around hovercar.  Number of ray corresponds to vector from the number 5 on the numPad.  Ray 58, for example, is straight up (ahead), 
		// from the 5 key up above to the 2 key.  Ray47 originates slightly to the left (4 is to the left of 5), and goes straight up (toward the seven key).
		Vector3 ray58emitter = transform.position + (transform.forward * 5f);
		Vector3 ray47emitter = transform.position + (transform.forward * 5f) + (transform.right * -15.0f);
		Vector3 ray69emitter = transform.position + (transform.forward * 5f) + (transform.right * 15.0f);
		Vector3 ray57emitter = transform.position + (transform.forward * -30f) + (transform.right * -15.0f);
		Vector3 ray59emitter = transform.position + (transform.forward * -30f) + (transform.right * 15.0f);
		Vector3 ray56emitter = transform.position + (transform.forward * -30f) + (transform.right * 15.0f);
		Vector3 ray23emitter = transform.position + (transform.forward * -50f) + (transform.right * 15.0f);
		Vector3 ray54emitter = transform.position + (transform.forward * -30f) + (transform.right * -15.0f);
		Vector3 ray21emitter = transform.position + (transform.forward * -50f) + (transform.right * -15.0f);

		RaycastHit ray21hit, ray23hit, ray58hit, ray54hit, ray56hit, ray57hit, ray59hit, ray47hit, ray69hit;
		bool ray58 = Physics.Raycast(ray58emitter, transform.forward, out ray58hit, navSensorRange);
		bool ray47 = Physics.Raycast(ray47emitter, transform.forward, out ray47hit, navSensorRange);
		bool ray69 = Physics.Raycast(ray69emitter, transform.forward, out ray69hit, navSensorRange);
		bool ray57 = Physics.Raycast(ray57emitter, (Quaternion.AngleAxis(-30f, transform.up) * transform.forward), out ray57hit, navSensorRange);
		bool ray59 = Physics.Raycast(ray59emitter, (Quaternion.AngleAxis(30f, transform.up) * transform.forward), out ray59hit, navSensorRange);
		bool ray56 = Physics.Raycast(ray56emitter, transform.right, out ray56hit, navSensorRange/2);
		bool ray23 = Physics.Raycast(ray23emitter, transform.right, out ray23hit, navSensorRange/2);
		bool ray54 = Physics.Raycast(ray54emitter, -transform.right, out ray54hit, navSensorRange/2);
		bool ray21 = Physics.Raycast(ray21emitter, -transform.right, out ray21hit, navSensorRange/2);


		if (ray58) { Debug.DrawLine(ray58emitter, ray58hit.point); }
		if (ray57) { Debug.DrawLine(ray57emitter, ray57hit.point); }
		if (ray59) { Debug.DrawLine(ray59emitter, ray59hit.point); }
		if (ray47) { Debug.DrawLine(ray47emitter, ray47hit.point); }
		if (ray69) { Debug.DrawLine(ray69emitter, ray69hit.point); }
		if (ray56) { Debug.DrawLine(ray56emitter, ray56hit.point); }
		if (ray23) { Debug.DrawLine(ray23emitter, ray23hit.point); }
		if (ray54) { Debug.DrawLine(ray54emitter, ray54hit.point); }
		if (ray21) { Debug.DrawLine(ray21emitter, ray21hit.point); }

		
		//slow down when near obstacles for better handling
		float nearestCollisionPotential = navSensorRange;  
		if (ray57 && ray57hit.distance < nearestCollisionPotential) { nearestCollisionPotential = ray57hit.distance; }
		if (ray47 && ray47hit.distance < nearestCollisionPotential) { nearestCollisionPotential = ray47hit.distance; }
		if (ray69 && ray69hit.distance < nearestCollisionPotential) { nearestCollisionPotential = ray69hit.distance; }
		if (ray59 && ray59hit.distance < nearestCollisionPotential) { nearestCollisionPotential = ray59hit.distance; }
		gasControl = Mathf.Clamp(nearestCollisionPotential / navSensorRange,0.2f,1f);		// might want to change this to full brake instead of proportional brakes? 

		
		//apply a modifier to turnControl based on results of various raycasts.  
		//modifiers may add together or cancel each other out, hopefully to get the best total route
		//these numbers may have to be tweaked.  I've experimented with weighted values and flat values but can't get numbers I really like yet.
		if (ray21) { turnControl = turnControl + 0.2f; }
		if (ray54) { turnControl = turnControl + 0.2f; }
		if (ray57) { turnControl = turnControl + 0.2f; }
		//if (ray58 && ray47 && ray69 && ray58hit.normal.x < 0) { steeringForce = steeringForce + 0.5f; }		//these two had weird effects on wall corners
		//if (ray58 && ray47 && ray69 && ray58hit.normal.x >= 0) { steeringForce = steeringForce - 0.5f; }
		if (ray58 && ray47 && ray69) { turnControl = turnControl + 0.5f; }    //consider taking shortest angle toward dome center and steering toward that, instead.
		if (ray47 && !ray58) { turnControl = turnControl + 0.3f; }
		if (ray69 && !ray58) { turnControl = turnControl - 0.3f; }
		if (ray59) { turnControl = turnControl - 0.2f; }
		if (ray56) { turnControl = turnControl - 0.2f; }
		if (ray23) { turnControl = turnControl - 0.2f; }
	}

	void StrikeHoverCars()		//this doesn't seem to be working at the moment
	{
		Vector3 ray58emitter = transform.position + (transform.forward * 5f);
		RaycastHit aggressionRay58hit;
		bool aggressionRay58 = Physics.Raycast(ray58emitter, transform.forward, out aggressionRay58hit, navSensorRange * 2.5f);
		if (aggressionRay58 && aggressionRay58hit.collider.gameObject.GetComponent<Destroyable>()) { sprintRamming = true; }
		else { sprintRamming = false; }
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
