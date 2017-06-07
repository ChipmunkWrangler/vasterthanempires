using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] Transform planet;
	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;

	public bool selected { get; private set; }

	Material material;
	Color originalColor;
	Vector3 offset;
	Transform tgtPlanet;
	float CLOSE_ENOUGH = 0.01f;

	public void SetTargetPlanet(Transform newPlanet) {
		tgtPlanet = newPlanet;
		planet = null;
		SetSelected (false);
		UpdateColor ();
	}

	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		originalColor = material.color;
		offset = transform.position - planet.position;
	}

	void Update() {
		if ( tgtPlanet && MoveTowards (tgtPlanet.position + offset)) {
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
		UpdateColor ();
	}

	void OnMouseUpAsButton() {
		SetSelected (!selected);
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
