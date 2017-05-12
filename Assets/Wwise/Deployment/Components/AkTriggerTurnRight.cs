using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkTriggerTurnRight : AkTriggerBase {



	public void TurnRight()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}


}