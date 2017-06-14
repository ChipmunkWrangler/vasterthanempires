using System.Collections;
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
			time = VTEUtil.GetTime();
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
	[SerializeField] SendDronesDecree sendDronesDecreePrefab;
	[SerializeField] DecreeCapsule decreeCapsulePrefab;


	public bool selected { get; private set; }

	Material material;
	List<MovementEvent> movementEvents; // last elements are the most recent

	public void SetTargetPlanet(Planet newPlanet) {
		print ("SetTargetPlanet at " + VTEUtil.GetTime());
		SetSelected (false);
		CmdSetTargetPlanet (newPlanet.netId);
		// todo: feedback that click was registered
	}

	public void SendDrones(Planet origin, Planet destination) {		
		SendDecreeCapsule (origin.transform.position);
		CmdSendDrones (this.netId, origin.netId, destination.netId);
	}

	public Vector3 GetActualPosition() {
		return GetPositionAt (VTEUtil.GetTime ()).Value;
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


	Vector3 GetApparentPosition() {		
		//		print("UpdateApparentPosition");
		float time = 0;
		for (int i = GetCurrentMovementEventIdx(); i >= 0; --i) {
			MovementEvent movementEvent = movementEvents [i];
			time = VTEUtil.GetApparentTime (movementEvent.startPos, movementEvent.tgtPos, movementEvent.time, unitsPerSec, VTEUtil.GetLocalPlayer ().GetActualPosition ());
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
		MovementEvent lastDeparture = movementEvents.FindLast( movementEvent => movementEvent.time < time );
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
		movementEvents.Add (new MovementEvent (startPosition, tgtPosition, tgtPlanet));
		print ("Creating Movement Event for player " + this.netId.Value.ToString () + " : total = " + movementEvents.Count);
		print (movementEvents [movementEvents.Count - 1].ToString ());			
	}

	void SendDecreeCapsule(Vector3 tgtPos) {
		DecreeCapsule decreeCapsule = (DecreeCapsule)GameObject.Instantiate (decreeCapsulePrefab);
		decreeCapsule.GoTo (GetActualPosition(), tgtPos);
	}

	[Command] void CmdSetTargetPlanet(NetworkInstanceId planetId) {
		print ("CmdSetTgtPlanet " + this.netId + " to planet " + planetId);
		RpcStartMovement (planetId, GetActualPosition());
	}
		
	[Command] void CmdSendDrones(NetworkInstanceId commanderId, NetworkInstanceId originPlanetId, NetworkInstanceId targetPlanetId) {
		print ("CmdSendDrones from " + originPlanetId + " to " + targetPlanetId + " / " + GetActualPosition ());
		SendDronesDecree decree = (SendDronesDecree)GameObject.Instantiate(sendDronesDecreePrefab);
		Planet origin = NetworkServer.FindLocalObject (originPlanetId).GetComponent<Planet> ();
		Planet target = NetworkServer.FindLocalObject (targetPlanetId).GetComponent<Planet> ();
		Player commander = NetworkServer.FindLocalObject (commanderId).GetComponent<Player>();
		decree.Send (commander, origin, target);
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
		print ("Arrive at " + VTEUtil.GetTime() + " after " + (VTEUtil.GetTime() - movementEvent.time));
		print ("Completing movement " + i.ToString () + " : " + movementEvent.ToString ());
		movementEvent.done = true;
		movementEvent.tgtPlanet.Conquer (this.netId);
		UpdateColor ();
	}

}
