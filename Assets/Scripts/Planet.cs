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
	[SerializeField] Player player;

//	List<ResourceEvent> resourceEvents;
	float initialTime;
	Material material;
	Color originalColor;

	// Use this for initialization
	void Start () {
		material = GetComponent<MeshRenderer> ().material;
		initialTime = Time.time;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position);
		resourceDisplay.text = "";
		originalColor = material.color;
		StartCoroutine (UpdateDisplay ());
	}

	void OnMouseUpAsButton() {
		if (player.selected) {
			player.SetTargetPlanet (transform);
		}
	}

	IEnumerator UpdateDisplay() {
		while (true) {
			yield return new WaitForSeconds (secsPerDisplayUpdate);
			float distToPlayer = GetDistToPlayer ();
			float apparentTime = GetApparentTime (distToPlayer);
			material.color = originalColor * (1f - distToPlayer / maxDist);
			if (apparentTime >= initialTime) {
				resourceDisplay.text = GetResourcesAtTime (apparentTime).ToString();
			}
		}
	}

	float GetDistToPlayer() {
		return Vector2.Distance (transform.position, player.transform.position);
	}

	float GetApparentTime(float distToPlayer) {
		float transmissionSecsToPlayer = distToPlayer / infoSpeedUnitsPerSec;
		return Time.time - transmissionSecsToPlayer;
	}

	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}
}
