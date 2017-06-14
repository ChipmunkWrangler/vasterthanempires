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
		RpcUpdateSize (GetNewSize());
		GetComponent<Moveable> ().commanderId = commanderId;
		GetComponent<Moveable>().RpcStartMovement (destination.netId, transform.position);
	}

	[ClientRpc] void RpcUpdateSize(float newScale) {
		transform.localScale = Vector3.one * newScale;
	}

	float GetNewSize() {
		return baseScale * Mathf.Pow (numDrones, 1f / 3f);
	}
		
}
