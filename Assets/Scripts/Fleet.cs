using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fleet : NetworkBehaviour {
	const float baseScale = 0.1f;

	List<DroneEvent> droneEvents;

	public void Init(NetworkInstanceId commanderId, int numDrones, Planet destination) {
		AddDroneEvent (commanderId, numDrones);
		GetComponent<Moveable> ().commanderId = commanderId;
		GetComponent<Moveable>().RpcStartMovement (destination.netId, transform.position);
	}

	void Update() {
		if (isClient) {
			UpdateSize();
		}
	}

	void UpdateSize() {
		float apparentTime = GetComponent<Moveable> ().GetApparentTime ();
		transform.localScale = Vector3.one * GetScaleAt (apparentTime);
	}

	float GetScaleAt(float time) {
		DroneEvent lastDroneChange = droneEvents.FindLast( droneEvent => droneEvent.time <= time );
		if (lastDroneChange == null) {
			return 0;
		}
		return baseScale * Mathf.Pow (lastDroneChange.numDrones, 1f / 3f);
	}

	void AddDroneEvent(NetworkInstanceId ownerId, int numDrones) {
		if (droneEvents == null) {
			droneEvents = new List<DroneEvent> ();
		}
		droneEvents.Add(new DroneEvent(ownerId, numDrones));
		RpcAddDroneEvent (ownerId, numDrones);
	}

	void OnArrivedAtPlanet(Planet planet) {
		DroneEvent lastDroneChange = droneEvents [droneEvents.Count - 1];
		print ("OnArrivedAtPlanet fleet of size  " + lastDroneChange.numDrones);
		if (planet.GetOwnerIdAt (VTEUtil.GetTime ()) == lastDroneChange.ownerId) {
			planet.RpcAddDrones (lastDroneChange.numDrones);
		} else if (planet.GetNumDrones () < lastDroneChange.numDrones) {
			planet.Conquer (lastDroneChange.ownerId, lastDroneChange.numDrones - planet.GetNumDrones ());
		} else {
			planet.RpcAddDrones (-lastDroneChange.numDrones);
		}
		AddDroneEvent(lastDroneChange.ownerId, 0);
	}

	[ClientRpc] void RpcAddDroneEvent(NetworkInstanceId ownerId, int numDrones) {
		if (droneEvents == null) {
			droneEvents = new List<DroneEvent> ();
		}		
		droneEvents.Add (new DroneEvent (ownerId, numDrones)); // duplicate this on client and server manually, since SyncVar doesn't work
	}
		
}
