using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendDronesDecree : Decree {
	Planet origin;
	Planet destination;

	public void Send(Player commander, Planet _origin, Planet _destination) {
		origin = _origin;
		destination = _destination;
		Send (commander, _origin);
	}

	override protected void Execute() {
		print ("Send drones");
	}
}
