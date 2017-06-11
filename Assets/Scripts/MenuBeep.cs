using UnityEngine;
using System.Collections;

public class MenuBeep : MonoBehaviour 
{

		public void onClick() 
	{
		AkSoundEngine.PostEvent ("Play_MenuBeep", gameObject);
	}
				
	}
