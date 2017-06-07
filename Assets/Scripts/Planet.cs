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
	[SerializeField] Vector3 labelOffset;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float infoSpeedUnitsPerSec = 0.5f;
	[SerializeField] Transform home;

//	List<ResourceEvent> resourceEvents;
	float initialTime;

	// Use this for initialization
	void Start () {
		initialTime = Time.time;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position) + labelOffset;
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
		float distToHome = Vector3.Distance (transform.position, home.position);
		float transmissionSecsToHome = distToHome / infoSpeedUnitsPerSec;
		return Time.time - transmissionSecsToHome;
	}
	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}
}
