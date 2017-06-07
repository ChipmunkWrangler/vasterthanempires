using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] Planet planet;
	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color originalColor;

	public bool selected { get; private set; }

	Material material;
	Vector3 offset;
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
		offset = transform.position - planet.transform.position;
		SetTargetPlanet (planet);
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
		planet.Conquer (originalColor);
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
