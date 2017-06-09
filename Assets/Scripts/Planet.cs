using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Planet : NetworkBehaviour {
	class ConquestEvent {
		public NetworkInstanceId ownerId { get; private set; }
		public float time { get; private set; }
		public ConquestEvent(NetworkInstanceId _ownerId) { 
			ownerId = _ownerId;
			time = VTEUtil.GetApparentTime();
		}
	}

	[SerializeField] float secsPerResource = 1f;
	[SerializeField] Text resourceDisplay;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float maxDist = 8f;
	[SerializeField] Color playerColor;
	[SerializeField] Color enemyColor;

	List<ConquestEvent> conquestEvents;
	float initialTime;
	Material material;
	Color neutralColor;

	public void Conquer(NetworkInstanceId conquerorId) {
		conquestEvents.Add(new ConquestEvent(conquerorId));
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
		conquestEvents = new List<ConquestEvent> ();
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
			NetworkInstanceId apparentOwnerId = GetOwnerAt (apparentTime);
			UpdateColor (apparentOwnerId, distToPlayer);
			bool apparentOwnerIsPlayer = apparentOwnerId == VTEUtil.GetLocalPlayer ().netId;
			if (apparentTime >= initialTime && apparentOwnerIsPlayer) {
				resourceDisplay.text = GetResourcesAtTime (apparentTime).ToString();
			}
		}
	}
		
	int GetResourcesAtTime(float time) {
		int resourcesWithoutEvents = Mathf.FloorToInt(time / secsPerResource);
		return resourcesWithoutEvents;
	}

	NetworkInstanceId GetOwnerAt(float time) {
		ConquestEvent lastConquest = conquestEvents.FindLast( conquestEvent => conquestEvent.time < time );
		return lastConquest != null ? lastConquest.ownerId : NetworkInstanceId.Invalid;
	}

	void UpdateColor(NetworkInstanceId apparentOwnerId, float distToPlayer) {
		Color baseColor = neutralColor;
		if (apparentOwnerId != NetworkInstanceId.Invalid) {
			bool apparentOwnerIsPlayer = apparentOwnerId == VTEUtil.GetLocalPlayer ().netId;
			baseColor = apparentOwnerIsPlayer ? playerColor : enemyColor;
		}
		material.color = baseColor * (1f - distToPlayer / maxDist);
	}
}
