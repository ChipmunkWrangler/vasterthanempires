using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	Vector2 initialTouch;

	void OnMouseDown() {
		initialTouch = Input.mousePosition;
	}

	void OnMouseDrag () {
		transform.position = (Vector3)GetWorldPosOfTouch();
	}

	Vector2 GetWorldPosOfTouch () {
		return Camera.main.ScreenToWorldPoint (Input.mousePosition);
	}
}
