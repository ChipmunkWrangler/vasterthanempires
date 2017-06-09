using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendDronesDecree : Decree {
	Planet origin;
	Planet target;

	public SendDronesDecree(Planet _origin, Planet _target) {
		origin = _origin;
		target = _target;
	}	

	public void Execute() {
		Debug.Log ("Execute SendDrone from " + origin.netId + " to " + target.netId + " at " + Time.time);
	}
}
