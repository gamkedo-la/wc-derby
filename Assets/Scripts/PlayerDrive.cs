using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrive : HoverCraftBase {
	protected override void Init () {
	
	}

	protected override void Tick () {
		turnControl = Input.GetAxis("Horizontal");
		gasControl = Input.GetAxis("Vertical");
	}
}
