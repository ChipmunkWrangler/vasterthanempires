using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Player : NetworkBehaviour {
	class MovementEvent {
		public Vector3 startPos { get; private set; }
		public Vector3 tgtPos { get; private set; }
		public float time { get; private set; }
		public MovementEvent(Vector3 _startPos, Vector3 _tgtPos) { 
			startPos = _startPos;
			tgtPos = _tgtPos;
			time = Time.time;
		}
		override public string ToString() {
			return "startPos = " + startPos.ToString () + " tgtPos " + tgtPos.ToString () + " time= " + time.ToString ();
		}
	}

	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color originalColor;
	[SerializeField] Color enemyColor;
	[SerializeField] Vector3 OFFSCREEN = new Vector3 (-100f, -100f, 0);

	public bool selected { get; private set; }
	public Vector3 actualPosition { get; private set; }

	Material material;
	[SyncVar(hook="TargetPlanetSet")] NetworkInstanceId tgtPlanetId = NetworkInstanceId.Invalid;
	Planet tgtPlanet;
	float CLOSE_ENOUGH = 0.01f;
	bool isColorDirty;
	List<MovementEvent> movementEvents; // last elements are the most recent

	public void SetTargetPlanet(Planet newPlanet) {
		SetSelected (false);
		CmdSetTargetPlanet (newPlanet.netId);
		// todo: feedback that click was registered
	}

	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		if (!isLocalPlayer) {
			gameObject.GetComponent<Collider> ().enabled = false;
			originalColor = enemyColor;
			material.color = enemyColor;
		}
		actualPosition = transform.position;
		isColorDirty = true;
		movementEvents = new List<MovementEvent> ();
		movementEvents.Add (new MovementEvent(actualPosition, actualPosition));
	}

	void Update() {
		if ( isServer && tgtPlanetId != NetworkInstanceId.Invalid && MoveTowards (movementEvents[movementEvents.Count-1].tgtPos)) {
			Arrive ();
		}
		if (isColorDirty) {
			UpdateColor ();
		}
		UpdateApparentPosition ();
	}

	void OnDrawGizmos() {
		Gizmos.DrawCube (actualPosition, new Vector3 (0.2f, 0.2f, 0.2f));
	}
		
	void TargetPlanetSet(NetworkInstanceId newTgtPlanetId) {
		tgtPlanetId = newTgtPlanetId;
		if (newTgtPlanetId != NetworkInstanceId.Invalid) {
			tgtPlanet = ClientScene.FindLocalObject (newTgtPlanetId).GetComponent<Planet> ();
		}
		movementEvents.Add (new MovementEvent(actualPosition, tgtPlanet.GetParkingSpace(this.netId)));
		isColorDirty = true;
	}

	bool MoveTowards(Vector3 tgtPos) {
		actualPosition = Vector3.MoveTowards (actualPosition, tgtPos, unitsPerSec * Time.deltaTime);
		return (Vector3.Distance (actualPosition, tgtPos) <= CLOSE_ENOUGH);			
	}

	void Arrive() {
		Assert.IsTrue (isServer);
		tgtPlanet.Conquer (this.netId);
		tgtPlanetId = NetworkInstanceId.Invalid;
	}

	void OnMouseUpAsButton() {
		SetSelected (!selected);
	}

	void SetSelected (bool b) {
		selected = b;
		isColorDirty = true;
	}

	void UpdateColor() {
		if (!isLocalPlayer) {
			return;
		}
		if (selected) {
			material.color = selectedColor;
		} else if (tgtPlanetId != NetworkInstanceId.Invalid) {
			material.color = movingColor;
		} else {
			material.color = originalColor;
		}
		isColorDirty = false;
	}


	[Command] void CmdSetTargetPlanet(NetworkInstanceId planetId) {
		tgtPlanetId = planetId;
	}

	void UpdateApparentPosition() {
		if (!isClient) {
			return;
		}
		print("UpdateApparentPosition");
		float time = VTEUtil.GetApparentTime (VTEUtil.GetDistToLocalPlayer(actualPosition)); 
		Vector3? newPosition = GetPositionAt (time);
		transform.position = newPosition.HasValue ? newPosition.Value : OFFSCREEN;
		print ("Apparent pos " + transform.position.ToString() + " Actual pos " + actualPosition.ToString());
	}

	Vector3? GetPositionAt(float time) {
		MovementEvent lastDeparture = movementEvents.FindLast( movementEvent => movementEvent.time < time );
		if (lastDeparture == null) {
			return null;
		}
		float timeRequired = Vector3.Distance(lastDeparture.startPos, lastDeparture.tgtPos) / unitsPerSec;
		float fractionCompleted = (time - lastDeparture.time) / timeRequired;
		print (lastDeparture);
		print("Appent Time" + time.ToString() + " Actual time " + Time.time );
		print ("Fraction completed = " + fractionCompleted);
		return Vector3.Lerp (lastDeparture.startPos, lastDeparture.tgtPos, fractionCompleted);
	}
}
