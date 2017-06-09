﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Player : NetworkBehaviour {
	class MovementEvent {
		public bool done;
		public Vector3 startPos { get; private set; }
		public Vector3 tgtPos { get; private set; }
		public float time { get; private set; }
		public Planet tgtPlanet { get; private set; }
		public MovementEvent(Vector3 _startPos, Vector3 _tgtPos, Planet _tgtPlanet) { 
			startPos = _startPos;
			tgtPos = _tgtPos;
			time = VTEUtil.GetApparentTime();
			tgtPlanet = _tgtPlanet;
			done = _startPos == _tgtPos;
		}
		override public string ToString() {
			return "startPos = " + startPos.ToString () + " tgtPos " + tgtPos.ToString () + " time= " + time.ToString () + " planet = " + (tgtPlanet == null ? "None" : tgtPlanet.netId.ToString()) + " done = " + done.ToString();
		}
	}

	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color originalColor;
	[SerializeField] Color enemyColor;
	[SerializeField] Vector3 OFFSCREEN = new Vector3 (-100f, -100f, 0);

	public bool selected { get; private set; }

	Material material;
	List<MovementEvent> movementEvents; // last elements are the most recent

	public void SetTargetPlanet(Planet newPlanet) {
		print ("SetTargetPlanet at " + VTEUtil.GetApparentTime());
		SetSelected (false);
		CmdSetTargetPlanet (newPlanet.netId);
		// todo: feedback that click was registered
	}

	public Vector3 GetActualPosition() {
		return GetPositionAt (VTEUtil.GetApparentTime ()).Value;
	}
		
	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		if (!isLocalPlayer) {
			gameObject.GetComponent<Collider> ().enabled = false;
			originalColor = enemyColor;
			material.color = enemyColor;
		}
		movementEvents = new List<MovementEvent> ();
		AddMovementEvent (transform.position, transform.position);
		UpdateColor ();
	}

	void Update() {
		if (isServer) {
			CheckForArrival ();
		}
		if (isClient) {
			UpdateApparentPosition ();
		}
	}

	void CheckForArrival() {
		Assert.IsTrue (isServer);
		if (IsMoving () && GetActualPosition () == GetTgtPos ()) { 
			RpcEndMovement (GetCurrentMovementEventIdx ());
		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawCube (GetActualPosition(), new Vector3 (0.2f, 0.2f, 0.2f));
	}

	int GetCurrentMovementEventIdx () {
		return movementEvents.Count - 1;
	}

	MovementEvent GetCurrentMovementEvent() {
		return movementEvents [GetCurrentMovementEventIdx()];
	}

	bool IsMoving() {
		return !GetCurrentMovementEvent ().done;
	}
		
	Vector3 GetTgtPos() {
		return GetCurrentMovementEvent().tgtPos;
	}

	void OnMouseUpAsButton() {
		SetSelected (!selected);
	}

	void SetSelected (bool b) {
		selected = b;
		UpdateColor ();
	}

	void UpdateColor() {
		if (!isLocalPlayer) {
			return;
		}
		if (selected) {
			material.color = selectedColor;
		} else if (IsMoving() ) {
			material.color = movingColor;
		} else {
			material.color = originalColor;
		}
	}


	[Command] void CmdSetTargetPlanet(NetworkInstanceId planetId) {
		print ("CmdSetTgtPlanet " + this.netId + " to planet " + planetId);
		RpcStartMovement (planetId, GetActualPosition());
	}

	[ClientRpc] void RpcStartMovement(NetworkInstanceId planetId, Vector3 startPos) { // don't rely on actualPosition being synched at exactly this moment
		print ("RpcStartMovement ");
		Planet tgtPlanet = ClientScene.FindLocalObject (planetId).GetComponent<Planet> ();
		Vector3 tgtPos = tgtPlanet.GetParkingSpace (this.netId);
		AddMovementEvent(startPos, tgtPos, tgtPlanet); 
		UpdateColor ();
	}
		
	[ClientRpc] void RpcEndMovement(int i) {
		MovementEvent movementEvent = movementEvents [i];
		print ("Arrive at " + VTEUtil.GetApparentTime() + " after " + (VTEUtil.GetApparentTime() - movementEvent.time));
		print ("Completing movement " + i.ToString () + " : " + movementEvent.ToString ());
		movementEvent.done = true;
		movementEvent.tgtPlanet.Conquer (this.netId);
		UpdateColor ();
	}
		
	void UpdateApparentPosition() {		
//		print("UpdateApparentPosition");
		float time = VTEUtil.GetApparentTime (VTEUtil.GetDistToLocalPlayer(GetActualPosition())); 
		Vector3? newPosition = GetPositionAt (time);
		transform.position = newPosition.HasValue ? newPosition.Value : OFFSCREEN;
//		print ("Apparent pos " + transform.position.ToString() + " Actual pos " + actualPosition.ToString());
	}

	Vector3? GetPositionAt(float time) {
		MovementEvent lastDeparture = movementEvents.FindLast( movementEvent => movementEvent.time < time );
		if (lastDeparture == null) {
			return null;
		}
		float timeRequired = Vector3.Distance(lastDeparture.startPos, lastDeparture.tgtPos) / unitsPerSec;
		float fractionCompleted = (time - lastDeparture.time) / timeRequired;
//		print (lastDeparture);
//		print("Apparent Time" + time.ToString() + " Actual time " + VTEUtil.GetApparentTime() );
//		print ("Fraction completed = " + fractionCompleted);
		return Vector3.Lerp (lastDeparture.startPos, lastDeparture.tgtPos, fractionCompleted);
	}

	void AddMovementEvent(Vector3 startPosition, Vector3 tgtPosition, Planet tgtPlanet = null) {
		movementEvents.Add (new MovementEvent (startPosition, tgtPosition, tgtPlanet));
		print ("Creating Movement Event for player " + this.netId.Value.ToString () + " : total = " + movementEvents.Count);
		print (movementEvents [movementEvents.Count - 1].ToString ());			
	}
}
