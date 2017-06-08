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
	[SerializeField] float maxDist = 8f;

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
		if (VTEUtil.GetLocalPlayer().selected) {
			VTEUtil.GetLocalPlayer().SetTargetPlanet (this);
		}
	}

	IEnumerator UpdateDisplay() {
		while (true) {
			yield return new WaitForSeconds (secsPerDisplayUpdate);
			float distToPlayer = VTEUtil.GetDistToLocalPlayer (transform.position);
			float apparentTime = VTEUtil.GetApparentTime (distToPlayer);
			material.color = originalColor * (1f - distToPlayer / maxDist);
			if (apparentTime >= initialTime && owner == VTEUtil.GetLocalPlayer()) {
				resourceDisplay.text = GetResourcesAtTime (apparentTime).ToString();
			}
		}
	}



	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}
}
