

public class AkTriggerTurnLeft : AkTriggerBase {


	public void TurningLeft()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}


}