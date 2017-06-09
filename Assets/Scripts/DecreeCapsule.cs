using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DecreeCapsule : NetworkBehaviour {
	[SerializeField] float unitsPerSec = 1f;

	Decree decree;
	Vector3 tgtPos;
	Vector3 startPos;
	float startTime;
	bool initialized;

	public void Init(Decree _decree, Vector3 _tgt) {
		decree = _decree;
		tgtPos = _tgt;
		startPos = transform.position;
		startTime = Time.time;
		initialized = true;
		print ("Send Decree from " + startPos + " to " + tgtPos + " at " + startTime);
	}

	void Update () {
		if (!initialized) {
			return;
		}
		if (isServer) {
			CheckForArrival ();
		}
		if (isClient) {
			UpdateApparentPosition ();
		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube (GetActualPosition(), new Vector3 (0.2f, 0.2f, 0.2f));
	}

	void CheckForArrival() {
		UnityEngine.Assertions.Assert.IsTrue (isServer);
		print (GetActualPosition());
		if (GetActualPosition () == tgtPos) { 
			RpcEndMovement ();
		}
	}

	void UpdateApparentPosition() {		
		float time = VTEUtil.GetApparentTime (VTEUtil.GetDistToLocalPlayer(GetActualPosition())); 
		transform.position = GetPositionAt (time);
	}

	Vector3 GetPositionAt(float time) {
		float timeRequired = Vector3.Distance(startPos, tgtPos) / unitsPerSec;
		float fractionCompleted = (time - startTime) / timeRequired;
		return Vector3.Lerp (startPos, tgtPos, fractionCompleted);
	}

	public Vector3 GetActualPosition() {
		return GetPositionAt (VTEUtil.GetApparentTime ());
	}

	[ClientRpc] void RpcEndMovement() {
		decree.Execute();
		Destroy (gameObject);
	}
}
