﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSelector : MonoBehaviour {
	GameObject selected;

	public void Select (GameObject o) {
		if (selected == o) {
			gameObject.SetActive (false);
			selected = null;
		} else {
			gameObject.SetActive (true);
			transform.position = o.transform.position;
			selected = o;
		}
	}
}
