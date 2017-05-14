using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelAISettings : MonoBehaviour {


	// This file was made because I suspect that some of the settings used in EnemyDrive.cs will likely need to be tuned for each level, 
	// most notably the safety threshold for how far away the AI can "see an obstacle".  I'm also thinking this system needs to be changed
	// so that the AI can see obstacles at further distances if they are in front of it.  I think both these values should be set here
	// by the level designer, and this script should be attached to an object on every level.  The object is being renamed "AI Level Manager"
	// and will be the parent object for all avoidance/obstacle points.
	//
	// In the meantime, this seems like a great place to put a toggle to make the points visible or not.  This should make it easier for others
	// to help tweak or fine-tune their locations.   

	// Note To self: Consider making this the one-and-only place that the array of obstacles needs to be created.

	public bool avoidPointsVisible;
	private bool avoidPointsVisibilitySetting;
	[HideInInspector]public Transform[] obstacles;
	public float AIdetectionRange=110f;

	private void Start() 
	{
		avoidPointsVisibilitySetting = avoidPointsVisible;
		int obstacleCount = -1;
		foreach (Transform child in this.transform)
		{
			if (child != this.transform)
			{ obstacleCount = obstacleCount + 1; }
		}
		
		obstacles = new Transform[obstacleCount + 1];

		obstacleCount = -1;
		foreach (Transform child in this.transform)
		{
			if (child != this.transform)
			{
				obstacleCount = obstacleCount + 1;
				obstacles[obstacleCount] = child;
			}
		}
	}

	private void Update()
	{
		if (avoidPointsVisibilitySetting != avoidPointsVisible) 
		{ avoidPointsVisible = avoidPointsVisibilitySetting; toggleAvoidancePointVisibility(); }
	}

	void toggleAvoidancePointVisibility()
	{
		avoidPointsVisibilitySetting = !avoidPointsVisibilitySetting;
		avoidPointsVisible = avoidPointsVisibilitySetting;
		if (avoidPointsVisibilitySetting)
		{ 
			foreach (Transform obstacle in obstacles)
			{
				obstacle.GetComponent<Renderer>().enabled = true;
			}
		}
		else if (!avoidPointsVisibilitySetting)
		{
			foreach (Transform obstacle in obstacles)
			{
				obstacle.GetComponent<Renderer>().enabled = false;
			}
		}
	}

}