using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Decree : NetworkBehaviour {
	protected abstract void Execute ();

	protected void Send(Player commander, Planet origin) {
		StartCoroutine(ExecuteDelayed(commander, origin));
	}

	IEnumerator ExecuteDelayed(NetworkBehaviour commander, Planet planet) {
		Vector3 startPos = commander.transform.position;
		Vector3 tgtPos = planet.transform.position;
		float travelTime = Vector2.Distance(startPos, tgtPos) / DecreeCapsule.unitsPerSec;
		yield return new WaitForSeconds (travelTime);
//		if (planet.GetOwnerIdAt(VTEUtil.GetTime()) == commander.netId) {
			Execute();
//		}
		Destroy (gameObject);
	}
}
