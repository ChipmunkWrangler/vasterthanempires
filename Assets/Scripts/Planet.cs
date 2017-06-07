using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour {
	[SerializeField] float secsPerResource = 1;
	[SerializeField] Text resourceDisplay;
	[SerializeField] Vector3 labelOffset;

	int resources;

	// Use this for initialization
	void Start () {
		StartCoroutine (IncreaseResources ());
	}

	IEnumerator IncreaseResources() {
		while (true) {
			yield return new WaitForSeconds (secsPerResource);
			++resources;
			resourceDisplay.text = resources.ToString();
			resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position) + labelOffset;
		}
	}
}
