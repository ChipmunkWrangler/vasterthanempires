using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DroneEvent {
	public NetworkInstanceId ownerId { get; private set; }
	public float time { get; private set; }
	public int numDrones { get; private set; }
	public DroneEvent(NetworkInstanceId _ownerId, int newNumDrones) { 
		ownerId = _ownerId;
		numDrones = newNumDrones;
		time = VTEUtil.GetTime();
	}
}
