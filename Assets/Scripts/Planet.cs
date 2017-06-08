using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Planet : NetworkBehaviour {
	class ResourceEvent {
		int resourceChange;
		float Time;
	}

	[SerializeField] float secsPerResource = 1f;
	[SerializeField] Text resourceDisplay;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float maxDist = 8f;
	[SerializeField] Color playerColor;
	[SerializeField] Color enemyColor;

//	List<ResourceEvent> resourceEvents;
	float initialTime;
	Material material;
	Color neutralColor;
	[SyncVar] NetworkInstanceId ownerId = NetworkInstanceId.Invalid;

	public void Conquer(NetworkInstanceId conquerorId) {
		Assert.IsTrue (isServer);
		ownerId = conquerorId;
	}

	public Vector3 GetParkingSpace(NetworkInstanceId shipId) {
		return transform.GetChild ((int)shipId.Value % transform.childCount).position;
	}

	void Start () {		
		material = GetComponent<MeshRenderer> ().material;
		initialTime = Time.time;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position);
		resourceDisplay.text = "";
		neutralColor = material.color;
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
			bool ownerIsPlayer = ownerId == VTEUtil.GetLocalPlayer ().netId;
			UpdateColor (ownerIsPlayer, distToPlayer);
			if (apparentTime >= initialTime && ownerIsPlayer) {
				resourceDisplay.text = GetResourcesAtTime (apparentTime).ToString();
			}
		}
	}
		
	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}

	void UpdateColor(bool ownerIsPlayer, float distToPlayer) {
		Color baseColor = neutralColor;
		if (ownerId != NetworkInstanceId.Invalid) {
			baseColor = ownerIsPlayer ? playerColor : enemyColor;
		}
		material.color = baseColor * (1f - distToPlayer / maxDist);
	}
}
