using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color originalColor;
	[SerializeField] Color enemyColor;
	[SerializeField] Vector3 offset;

	public bool selected { get; private set; }

	Material material;
	[SyncVar(hook="TargetPlanetSet")] NetworkInstanceId tgtPlanetId = NetworkInstanceId.Invalid;
	Planet tgtPlanet;
	float CLOSE_ENOUGH = 0.01f;
	bool isColorDirty;

	public void SetTargetPlanet(Planet newPlanet) {
		SetSelected (false);
		CmdSetTargetPlanet (newPlanet.netId);
		// todo: feedback that click was registered
	}

	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		transform.position = transform.position + offset; 
		if (!isLocalPlayer) {
			originalColor = enemyColor;
		}
		isColorDirty = true;
	}

	void Update() {
		if ( tgtPlanetId != NetworkInstanceId.Invalid && MoveTowards (tgtPlanet.transform.position + offset)) {
			Arrive ();
		}
		if (isColorDirty) {
			UpdateColor ();
		}
	}
		
	void TargetPlanetSet(NetworkInstanceId newTgtPlanetId) {
		tgtPlanetId = newTgtPlanetId;
		if (newTgtPlanetId != NetworkInstanceId.Invalid) {
			tgtPlanet = ClientScene.FindLocalObject (newTgtPlanetId).GetComponent<Planet> ();
		}
		isColorDirty = true;
	}

	bool MoveTowards(Vector3 tgtPos) {
		transform.position = Vector3.MoveTowards (transform.position, tgtPos, unitsPerSec * Time.deltaTime);
		return (Vector3.Distance (transform.position, tgtPos) <= CLOSE_ENOUGH);			
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
}
