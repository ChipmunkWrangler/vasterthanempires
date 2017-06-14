using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SendDronesDecree : Decree {
	[SerializeField] Fleet fleetPrefab;

	Planet origin;
	Planet destination;
	Player commander;

	public void Send(Player _commander, Planet _origin, Planet _destination) {
		origin = _origin;
		destination = _destination;
		commander = _commander;
		Send (commander, _origin);
	}

	 override protected void Execute() {		
		print ("Send drones");
		Fleet fleet = GameObject.Instantiate (fleetPrefab, origin.transform.position, Quaternion.identity);
		NetworkServer.Spawn (fleet.gameObject);
		fleet.Init(commander.netId, origin.RemoveDrones (), destination);
	}
				
}
