using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Player : NetworkBehaviour {
	[SerializeField] SendDronesDecree sendDronesDecreePrefab;
	[SerializeField] DecreeCapsule decreeCapsulePrefab;

	public void SendDrones(Planet origin, Planet destination) {		
		SendDecreeCapsule (origin.transform.position);
		CmdSendDrones (this.netId, origin.netId, destination.netId);
	}
		
	void Start() {
		if (!isLocalPlayer) {
			gameObject.GetComponent<Collider> ().enabled = false;
		}
		GetComponent<Moveable> ().Init(this.netId);
	}

	void SendDecreeCapsule(Vector3 tgtPos) {
		DecreeCapsule decreeCapsule = (DecreeCapsule)GameObject.Instantiate (decreeCapsulePrefab);
		decreeCapsule.GoTo (GetComponent<Moveable>().GetActualPosition(), tgtPos);
	}

	[Command] void CmdSendDrones(NetworkInstanceId commanderId, NetworkInstanceId originPlanetId, NetworkInstanceId targetPlanetId) {
		print ("CmdSendDrones from " + originPlanetId + " to " + targetPlanetId + " / " + GetComponent<Moveable>().GetActualPosition ());
		SendDronesDecree decree = (SendDronesDecree)GameObject.Instantiate(sendDronesDecreePrefab);
		Planet origin = NetworkServer.FindLocalObject (originPlanetId).GetComponent<Planet> ();
		Planet target = NetworkServer.FindLocalObject (targetPlanetId).GetComponent<Planet> ();
		Player commander = NetworkServer.FindLocalObject (commanderId).GetComponent<Player>();
		decree.Send (commander, origin, target);
	}
}
