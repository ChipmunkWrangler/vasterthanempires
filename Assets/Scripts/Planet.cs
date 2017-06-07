using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour {
	class ResourceEvent {
		int resourceChange;
		float Time;
	}

	[SerializeField] float secsPerResource = 1f;
	[SerializeField] Text resourceDisplay;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float infoSpeedUnitsPerSec = 0.5f;
	[SerializeField] float maxDist = 8f;

//	List<ResourceEvent> resourceEvents;
	float initialTime;
	float transmissionSecsToHome;


	// Use this for initialization
	void Start () {
		float distToHome = transform.position.magnitude; // for now, the home planet is at 0,0,0 and doesn't move
		transmissionSecsToHome = distToHome / infoSpeedUnitsPerSec;
		Material material = GetComponent<MeshRenderer> ().material;
		material.color = material.color * (1f - distToHome / maxDist);
		initialTime = Time.time;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position);
		resourceDisplay.text = "";
		StartCoroutine (UpdateDisplay ());
	}

	IEnumerator UpdateDisplay() {
		while (true) {
			yield return new WaitForSeconds (secsPerDisplayUpdate);
			float apparentTime = GetApparentTime ();
			if (apparentTime >= initialTime) {
				resourceDisplay.text = GetResourcesAtTime (apparentTime).ToString();
			}
		}
	}

	float GetApparentTime() {
		return Time.time - transmissionSecsToHome;
	}
	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}
}
