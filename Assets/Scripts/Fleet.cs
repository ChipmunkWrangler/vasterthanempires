using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fleet : NetworkBehaviour {
	[SyncVar] int numDrones;
	float baseScale;

	public void Init(NetworkInstanceId commanderId, int _numDrones, Planet destination) {
		numDrones = _numDrones;
		baseScale = transform.localScale.x;
		UpdateSize ();
		GetComponent<Moveable> ().commanderId = commanderId;
		GetComponent<Moveable>().RpcStartMovement (destination.netId, transform.position);
	}

	void UpdateSize() {
		transform.localScale = Vector3.one * baseScale * Mathf.Pow (numDrones, 1f / 3f);
	}
		
}
