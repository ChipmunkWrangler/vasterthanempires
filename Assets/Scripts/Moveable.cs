using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;


public class Moveable : NetworkBehaviour {
	public bool selected { get; private set; }

	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color originalColor;
	[SerializeField] Color enemyColor;


	Material material;
	List<MovementEvent> movementEvents; // last elements are the most recent
	[SyncVar(hook="OnSyncCommanderId")] public NetworkInstanceId commanderId;

	public void UserSaysSetTargetPlanet(Planet newPlanet) {
		print ("SetTargetPlanet at " + VTEUtil.GetTime());
		SetSelected (false);
		CmdSetTargetPlanet (newPlanet.netId);
		// todo: feedback that click was registered
	}

	public Vector3 GetActualPosition() {
		return GetPositionAt (VTEUtil.GetTime ()).Value;
	}
		
	void OnSyncCommanderId(NetworkInstanceId _value) {
		commanderId = _value;
		UpdateColor ();
	}

	void Start() {
		InitMovementEvents ();
		UpdateColor ();
	}


	void Update() {
		if (isServer) {
			CheckForArrival ();
		}
		if (isClient) {
			transform.position = (isLocalPlayer) ? GetActualPosition() : GetApparentPosition ();
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
		return movementEvents != null && !GetCurrentMovementEvent ().done;
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
		if (!material) {
			material = GetComponent<MeshRenderer> ().material;
		}
			
		if (!IsControlledByLocalPlayer()) {
			material.color = enemyColor;
		} else if (selected) {
			material.color = selectedColor;
		} else if (IsMoving() ) {
			material.color = movingColor;
		} else {
			material.color = originalColor;
		}
	}
		
	Vector3 GetApparentPosition() {		
		//		print("UpdateApparentPosition");
		float time = 0;
		for (int i = GetCurrentMovementEventIdx(); i >= 0; --i) {
			MovementEvent movementEvent = movementEvents [i];
			time = VTEUtil.GetApparentTime (movementEvent.startPos, movementEvent.tgtPos, movementEvent.time, unitsPerSec, VTEUtil.GetLocalPlayerComponent<Moveable> ().GetActualPosition ());
			if (time >= 0) {
				break;
			} else {
				print ("Going back " + (1 + GetCurrentMovementEventIdx () - i).ToString ());
			}
		}
		Vector3? newPosition = GetPositionAt (time);
		return newPosition.HasValue ? newPosition.Value : VTEUtil.OFFSCREEN;
		//		print ("Apparent pos " + transform.position.ToString() + " Actual pos " + actualPosition.ToString());
	}

	Vector3? GetPositionAt(float time) {
		if (time <= 0) {
			return null;
		}
		MovementEvent lastDeparture = movementEvents.FindLast( movementEvent => movementEvent.time <= time );
		if (lastDeparture == null) {
			return null;
		}
		float timeRequired = Vector3.Distance(lastDeparture.startPos, lastDeparture.tgtPos) / unitsPerSec;
		float fractionCompleted = (time - lastDeparture.time) / timeRequired;
		//		print (lastDeparture);
		//		print("Apparent Time" + time.ToString() + " Actual time " + VTEUtil.GetTime() );
		//		print ("Fraction completed = " + fractionCompleted);
		return Vector3.Lerp (lastDeparture.startPos, lastDeparture.tgtPos, fractionCompleted);
	}

	void AddMovementEvent(Vector2 startPosition, Vector2 tgtPosition, Planet tgtPlanet = null) {
		InitMovementEvents ();
		movementEvents.Add (new MovementEvent (startPosition, tgtPosition, tgtPlanet));
		print ("Creating Movement Event for ship " + this.netId.Value.ToString () + " : total = " + movementEvents.Count);
		print (movementEvents [movementEvents.Count - 1].ToString ());			
	}

	void InitMovementEvents() {
		if (movementEvents == null) {
			movementEvents = new List<MovementEvent> ();
			AddMovementEvent (transform.position, transform.position);
		}
	}

	bool IsControlledByLocalPlayer() {
		return VTEUtil.GetLocalPlayerComponent<NetworkBehaviour> ().netId == commanderId;
	}
		
	[Command] void CmdSetTargetPlanet(NetworkInstanceId planetId) {
		print ("CmdSetTgtPlanet: ship " + this.netId + " to planet " + planetId);
		RpcStartMovement (planetId, GetActualPosition());
	}

	[ClientRpc] public void RpcStartMovement(NetworkInstanceId planetId, Vector3 startPos) { // don't rely on actualPosition being synched at exactly this moment
		print ("RpcStartMovement ");
		Planet tgtPlanet = ClientScene.FindLocalObject (planetId).GetComponent<Planet> ();
		Vector3 tgtPos = tgtPlanet.GetParkingSpace (commanderId);
		AddMovementEvent(startPos, tgtPos, tgtPlanet); 
		UpdateColor ();
	}

	[ClientRpc] void RpcEndMovement(int i) {
		MovementEvent movementEvent = movementEvents [i];
		print ("Arrive at " + VTEUtil.GetTime() + " after " + (VTEUtil.GetTime() - movementEvent.time));
		print ("Completing movement " + i.ToString () + " : " + movementEvent.ToString ());
		movementEvent.done = true;
		movementEvent.tgtPlanet.Conquer (commanderId);
		UpdateColor ();
	}


}
