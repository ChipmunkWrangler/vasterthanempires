using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] Transform planet;

	public bool selected { get; private set; }

	Material material;
	Color originalColor;
	Vector3 offset;

	public void MoveTo(Transform planet) {
		transform.position = planet.position + offset;
		SetSelected (false);
	}

	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		originalColor = material.color;
		offset = transform.position - planet.position;
	}

	void OnMouseUpAsButton() {
		SetSelected (!selected);
	}

	void SetSelected (bool b) {
		selected = b;
		material.color = selected ? Color.yellow : originalColor;
	}

}
