using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkTriggerTurnLeft : AkTriggerBase
{


	public void TurnLeft()
	{ 
		if(triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}



}