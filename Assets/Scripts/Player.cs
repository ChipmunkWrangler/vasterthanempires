using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
	[SerializeField] Vector3 offset;
	[SerializeField] Vector3 OFFSCREEN = new Vector3 (-100f, -100f, 0);
	[SerializeField] float LOCAL_Z_OFFSET = -1f;

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
		if (isLocalPlayer) {
			offset.z += LOCAL_Z_OFFSET;
			transform.position = transform.position + offset;
		} else {
			transform.position = transform.position - new Vector3 (0, 0, LOCAL_Z_OFFSET);
			gameObject.GetComponent<Collider> ().enabled = false;
		}
		transform.rotation = Random.rotationUniform;
		actualPosition = transform.position;
		if (!isLocalPlayer) {
			originalColor = enemyColor;
			material.color = enemyColor;
		}
		isColorDirty = true;
		movementEvents = new List<MovementEvent> ();
		movementEvents.Add (new MovementEvent(actualPosition, actualPosition));
	}

	void Update() {
		if ( tgtPlanetId != NetworkInstanceId.Invalid && MoveTowards (movementEvents[movementEvents.Count-1].tgtPos)) {
			Arrive ();
		}
		if (isColorDirty) {
			UpdateColor ();
		}
		UpdateApparentPosition ();
	}
		
	void TargetPlanetSet(NetworkInstanceId newTgtPlanetId) {
		tgtPlanetId = newTgtPlanetId;
		if (newTgtPlanetId != NetworkInstanceId.Invalid) {
			tgtPlanet = ClientScene.FindLocalObject (newTgtPlanetId).GetComponent<Planet> ();
		}
		movementEvents.Add (new MovementEvent(actualPosition, tgtPlanet.transform.position + offset));
		isColorDirty = true;
	}

	bool MoveTowards(Vector3 tgtPos) {
		actualPosition = Vector3.MoveTowards (actualPosition, tgtPos, unitsPerSec * Time.deltaTime);
		return (Vector3.Distance (actualPosition, tgtPos) <= CLOSE_ENOUGH);			
	}

	void Arrive() {
		tgtPlanet.Conquer (this, originalColor);
		if (isServer) {
			tgtPlanetId = NetworkInstanceId.Invalid;
		}
	}

	void OnMouseUpAsButton() {
		if (isLocalPlayer) {
			SetSelected (!selected);
		}
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
