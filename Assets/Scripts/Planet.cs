using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Planet : NetworkBehaviour {
	class ResourceEvent {
		int resourceChange;
		float Time;
	}

	[SerializeField] float secsPerResource = 1f;
	[SerializeField] Text resourceDisplay;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float infoSpeedUnitsPerSec = 0.5f;
	[SerializeField] float maxDist = 8f;

	Player localPlayer;
//	List<ResourceEvent> resourceEvents;
	float initialTime;
	Material material;
	Color originalColor;
	Player owner;

	public void Conquer(Player conqueror, Color conquerorColor) {
		owner = conqueror;
		originalColor = conquerorColor;
	}

	void Start () {		
		material = GetComponent<MeshRenderer> ().material;
		initialTime = Time.time;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position);
		resourceDisplay.text = "";
		originalColor = material.color;
		StartCoroutine (UpdateDisplay ());
	}

	void OnMouseUpAsButton() {
		if (GetLocalPlayer().selected) {
			GetLocalPlayer().SetTargetPlanet (this);
		}
	}

	IEnumerator UpdateDisplay() {
		while (true) {
			yield return new WaitForSeconds (secsPerDisplayUpdate);
			float distToPlayer = GetDistToPlayer ();
			float apparentTime = GetApparentTime (distToPlayer);
			material.color = originalColor * (1f - distToPlayer / maxDist);
			if (apparentTime >= initialTime && owner == GetLocalPlayer()) {
				resourceDisplay.text = GetResourcesAtTime (apparentTime).ToString();
			}
		}
	}

	float GetDistToPlayer() {
		return Vector2.Distance (transform.position, GetLocalPlayer().transform.position);
	}

	float GetApparentTime(float distToPlayer) {
		float transmissionSecsToPlayer = distToPlayer / infoSpeedUnitsPerSec;
		return Time.time - transmissionSecsToPlayer;
	}

	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}

	Player GetLocalPlayer() {
		if (!localPlayer) {
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject o in players) {
				localPlayer = o.GetComponent<Player> ();
				if (localPlayer.isLocalPlayer) {
					break;
				}
			}
		}
		return localPlayer;
	}
}
