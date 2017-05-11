using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spotlight : MonoBehaviour {

	Vector3 centralVector;
	float dir = 1f;
	float rotationSpeed = 5f;
	
	private void Start() 
	{
		Vector3 primaryFocusPoint = transform.position + transform.forward;
		centralVector = primaryFocusPoint - transform.position;
	}

	private void Update()
	{
		if (Vector3.Angle(transform.forward, centralVector) > 15f)
		{
			dir = dir * -1f;		
		}
		transform.Rotate(0,dir * Time.deltaTime * rotationSpeed, 0);

	}



}