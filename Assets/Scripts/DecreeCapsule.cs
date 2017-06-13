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
	bool executed;

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
			if (transform.position == tgtPos) {
				Destroy (gameObject);
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube (GetActualPosition(), new Vector3 (0.2f, 0.2f, 0.2f));
	}

	void CheckForArrival() {
		UnityEngine.Assertions.Assert.IsTrue (isServer);
		if (!executed && GetActualPosition () == tgtPos) { 
			decree.Execute();
			executed = true;
		}
	}

	void UpdateApparentPosition() {		
		float time = VTEUtil.GetApparentTime (startPos, tgtPos, startTime, unitsPerSec, VTEUtil.GetLocalPlayer().GetActualPosition());
		transform.position = GetPositionAt (time);
	}

	Vector3 GetPositionAt(float time) {
		if (time == 0) {
			return VTEUtil.OFFSCREEN;
		}
		float timeRequired = Vector3.Distance(startPos, tgtPos) / unitsPerSec;
		float fractionCompleted = (time - startTime) / timeRequired;
		return Vector3.Lerp (startPos, tgtPos, fractionCompleted);
	}

	public Vector3 GetActualPosition() {
		return GetPositionAt (VTEUtil.GetTime ());
	}
}
