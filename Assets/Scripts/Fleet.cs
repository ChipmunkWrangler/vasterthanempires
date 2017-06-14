using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fleet : NetworkBehaviour {
	public void Init(NetworkInstanceId commanderId, int _size, Planet destination) {
		GetComponent<Moveable>().RpcStartMovement (destination.netId, transform.position);
		GetComponent<Moveable> ().Init(commanderId);
	}
}
