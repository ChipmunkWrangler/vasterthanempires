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
	Planet planet;
	Planet tgtPlanet;
	float CLOSE_ENOUGH = 0.01f;

	public void SetTargetPlanet(Planet newPlanet) {
		tgtPlanet = newPlanet;
		planet = null;
		SetSelected (false);
		UpdateColor ();
	}

	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		transform.position = transform.position + offset; 
		if (!isLocalPlayer) {
			originalColor = enemyColor;
		}
		UpdateColor ();
	}

	void Update() {
		if ( tgtPlanet && MoveTowards (tgtPlanet.transform.position + offset)) {
			Arrive ();
		}
	}

	bool MoveTowards(Vector3 tgtPos) {
		transform.position = Vector3.MoveTowards (transform.position, tgtPos, unitsPerSec * Time.deltaTime);
		return (Vector3.Distance (transform.position, tgtPos) <= CLOSE_ENOUGH);			
	}

	void Arrive() {
		planet = tgtPlanet;
		tgtPlanet = null;
		planet.Conquer (this, originalColor);
		UpdateColor ();
	}

	void OnMouseUpAsButton() {
		if (isLocalPlayer) {
			SetSelected (!selected);
		}
	}

	void SetSelected (bool b) {
		selected = b;
		UpdateColor ();
	}

	void UpdateColor() {
		if (selected) {
			material.color = selectedColor;
		} else if (tgtPlanet) {
			material.color = movingColor;
		} else {
			material.color = originalColor;
		}
	}
}
