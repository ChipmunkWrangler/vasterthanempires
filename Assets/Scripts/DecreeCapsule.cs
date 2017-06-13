using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DecreeCapsule : MonoBehaviour {
	public const float unitsPerSec = 1f;

	Vector3 tgtPos;
	Vector3 startPos;
	float startTime;
	bool initialized;

	public void Init(Vector3 _tgt) {
		tgtPos = _tgt;
		startPos = transform.position;
		startTime = VTEUtil.GetTime();
		initialized = true;
		print ("Send Decree from " + startPos + " to " + tgtPos);
	}

	void Update () {
		if (!initialized) {
			return;
		}
		UpdateApparentPosition ();
		if (transform.position == tgtPos) {
			Destroy (gameObject);
		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube (GetActualPosition(), new Vector3 (0.2f, 0.2f, 0.2f));
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
