using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Player : NetworkBehaviour {
	[SerializeField] SendDronesDecree sendDronesDecreePrefab;
	[SerializeField] DecreeCapsule decreeCapsulePrefab;

	bool firstUpdate = true;

	public void SendDrones(Planet origin, Planet destination) {		
		SendDecreeCapsule (origin.transform.position);
		CmdSendDrones (this.netId, origin.netId, destination.netId);
	}
		
	void Start() {
		if (!isLocalPlayer) {
			gameObject.GetComponent<Collider> ().enabled = false;
		}
		GetComponent<Moveable> ().commanderId = this.netId;
		ConquerInitialPlanet ();
	}

	void Update() {
		if (firstUpdate) {
			ConquerInitialPlanet ();
			firstUpdate = false;
		}
	}

	void SendDecreeCapsule(Vector3 tgtPos) {
		DecreeCapsule decreeCapsule = (DecreeCapsule)GameObject.Instantiate (decreeCapsulePrefab);
		decreeCapsule.GoTo (GetComponent<Moveable>().GetActualPosition(), tgtPos);
	}

	void ConquerInitialPlanet() {
		foreach (GameObject planet in GameObject.FindGameObjectsWithTag ("Planet")) {
			if (planet.transform.position == transform.position) {
				Planet closestPlanet = planet.GetComponent<Planet>();
				Assert.IsTrue (closestPlanet.GetOwnerIdAt (VTEUtil.GetTime ()) == NetworkInstanceId.Invalid);
				GetComponent<Moveable>().UserSaysSetTargetPlanet (closestPlanet);
				break;
			}
		}
	}

	[Command] void CmdSendDrones(NetworkInstanceId commanderId, NetworkInstanceId originPlanetId, NetworkInstanceId targetPlanetId) {
		print ("CmdSendDrones from " + originPlanetId + " to " + targetPlanetId + " / " + GetComponent<Moveable>().GetActualPosition ());
		SendDronesDecree decree = (SendDronesDecree)GameObject.Instantiate(sendDronesDecreePrefab);
		Planet origin = NetworkServer.FindLocalObject (originPlanetId).GetComponent<Planet> ();
		Planet target = NetworkServer.FindLocalObject (targetPlanetId).GetComponent<Planet> ();
		Player commander = NetworkServer.FindLocalObject (commanderId).GetComponent<Player>();
		decree.Send (commander, origin, target);
	}

	void OnArrivedAtPlanet(Planet planet) {
		if (planet.GetOwnerIdAt (VTEUtil.GetTime ()) == NetworkInstanceId.Invalid) {
			planet.Conquer (netId, 0);
		}
	}

}
