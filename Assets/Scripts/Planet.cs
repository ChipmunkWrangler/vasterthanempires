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

	[SerializeField] float secsPerDrone = 1f;
	[SerializeField] Text resourceDisplay;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float maxDist = 8f;
	[SerializeField] Color playerColor;
	[SerializeField] Color enemyColor;
	[SerializeField] PlanetSelector selection;

	List<ConquestEvent> conquestEvents;
	Material material;
	Color neutralColor;
	ConquestEvent initialEvent;

	public void Conquer(NetworkInstanceId conquerorId) {
		conquestEvents.Add(new ConquestEvent(conquerorId));
	}

	public Vector3 GetParkingSpace(NetworkInstanceId shipId) {
		return transform.GetChild ((int)shipId.Value % transform.childCount).position;
	}

	void Start () {		
		material = GetComponent<MeshRenderer> ().material;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position);
		resourceDisplay.text = "";
		neutralColor = material.color;
		conquestEvents = new List<ConquestEvent> ();
		initialEvent = new ConquestEvent (NetworkInstanceId.Invalid);
		StartCoroutine (UpdateDisplay ());
	}

	void OnMouseUpAsButton() {
		if (VTEUtil.GetLocalPlayer ().selected) {
			VTEUtil.GetLocalPlayer ().SetTargetPlanet (this);
		} else {
			GameObject origin = selection.GetSelected ();
			if (origin == null) {
				SelectOrigin ();
			} else if (origin == gameObject) {
				Deselect ();
			} else {
				SelectTarget (origin);
			}
		}
	}

	void SelectOrigin() {
		selection.Select (gameObject);
	}

	void Deselect() {
		selection.Select(null);
	}

	void SelectTarget(GameObject origin) {
		VTEUtil.GetLocalPlayer ().SendDrones (origin.GetComponent<Planet> (), this);
		Deselect ();
	}

	IEnumerator UpdateDisplay() {
		while (true) {
			yield return new WaitForSeconds (secsPerDisplayUpdate);
			float distToPlayer = VTEUtil.GetDistToLocalPlayer (transform.position);
			float apparentTime = VTEUtil.GetApparentTime (distToPlayer);
			UpdateColor (apparentTime, distToPlayer);
			UpdateDroneDisplay (apparentTime);
		}
	}
		
	void UpdateDroneDisplay(float time) {
		int numDrones = GetDronesAt (time);
		resourceDisplay.text = numDrones > 0 ? numDrones.ToString() : "";
	}
		
	int GetDronesAt(float time) {
		int numDrones = 0;
		ConquestEvent lastConquest = GetLastConquestEventBefore (time);
		if (lastConquest.ownerId != NetworkInstanceId.Invalid) {
			float timeSinceLastConquest = time - lastConquest.time;
			numDrones = Mathf.FloorToInt (timeSinceLastConquest / secsPerDrone);
		}
		return numDrones;
	}

	ConquestEvent GetLastConquestEventBefore(float time) {
		ConquestEvent ce = conquestEvents.FindLast( conquestEvent => conquestEvent.time < time);
		if (ce == null) {
			ce = initialEvent;
		}
		return ce;
	}

	bool IsFriendly(float time) {
		NetworkInstanceId ownerId = GetLastConquestEventBefore (time).ownerId;
		return ownerId == VTEUtil.GetLocalPlayer ().netId;
	}
					
	void UpdateColor(float time, float distToPlayer) {
		Color baseColor = enemyColor;
		NetworkInstanceId ownerId = GetLastConquestEventBefore (time).ownerId;
		if (ownerId == NetworkInstanceId.Invalid) {
			baseColor = neutralColor;
		} else if (ownerId == VTEUtil.GetLocalPlayer ().netId) {
			baseColor = playerColor;
		}
		material.color = baseColor * (1f - distToPlayer / maxDist);
	}
}
