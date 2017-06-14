using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Planet : NetworkBehaviour {
	class DroneEvent {
		public NetworkInstanceId ownerId { get; private set; }
		public float time { get; private set; }
		public int numDrones { get; private set; }
		public DroneEvent(NetworkInstanceId _ownerId, int newNumDrones) { 
			ownerId = _ownerId;
			numDrones = newNumDrones;
			time = VTEUtil.GetTime();
		}
	}

	[SerializeField] float secsPerDrone = 1f;
	[SerializeField] Text resourceDisplay;
	[SerializeField] float secsPerDisplayUpdate = 1f;
	[SerializeField] float maxDist = 8f;
	[SerializeField] Color playerColor;
	[SerializeField] Color enemyColor;
	[SerializeField] PlanetSelector selection;

	List<DroneEvent> droneEvents;
	Material material;
	Color neutralColor;
	DroneEvent initialEvent;

	public void Conquer(NetworkInstanceId conquerorId) {
		droneEvents.Add(new DroneEvent(conquerorId, 0));
	}

	public Vector3 GetParkingSpace(NetworkInstanceId shipId) {
		return transform.GetChild ((int)shipId.Value % transform.childCount).position;
	}

	public NetworkInstanceId GetOwnerIdAt(float time) {
		return GetLastConquestEventBefore (time).ownerId;
	}

	public int GetNumDrones() {
		return GetDronesAt (VTEUtil.GetTime ());
	}

	[ClientRpc] public void RpcAddDrones(int numDronestoAdd) {
		droneEvents.Add(new DroneEvent(GetOwnerIdAt(VTEUtil.GetTime()), GetNumDrones() + numDronestoAdd));
	}

	void Start () {		
		material = GetComponent<MeshRenderer> ().material;
		resourceDisplay.transform.position = Camera.main.WorldToScreenPoint (transform.position);
		resourceDisplay.text = "";
		neutralColor = material.color;
		droneEvents = new List<DroneEvent> ();
		initialEvent = new DroneEvent(NetworkInstanceId.Invalid, 0);
		StartCoroutine (UpdateDisplay ());
	}

	void OnMouseUpAsButton() {
		Moveable player = VTEUtil.GetLocalPlayerComponent<Moveable> ();
		if (player.selected) {
			player.UserSaysSetTargetPlanet (this);
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
		VTEUtil.GetLocalPlayerComponent<Player> ().SendDrones (origin.GetComponent<Planet> (), this);
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
		DroneEvent lastConquest = GetLastConquestEventBefore (time);
		if (lastConquest.ownerId != NetworkInstanceId.Invalid) {
			float timeSinceLastConquest = time - lastConquest.time;
			numDrones = Mathf.FloorToInt (timeSinceLastConquest / secsPerDrone);
		}
		return numDrones;
	}

	DroneEvent GetLastConquestEventBefore(float time) {
		DroneEvent ce = droneEvents.FindLast( conquestEvent => conquestEvent.time < time);
		if (ce == null) {
			ce = initialEvent;
		}
		return ce;
	}
							
	void UpdateColor(float time, float distToPlayer) {
		Color baseColor = enemyColor;
		NetworkInstanceId ownerId = GetOwnerIdAt(time);
		if (ownerId == NetworkInstanceId.Invalid) {
			baseColor = neutralColor;
		} else if (ownerId == VTEUtil.GetLocalPlayerComponent<NetworkBehaviour> ().netId) {
			baseColor = playerColor;
		}
		material.color = baseColor * (1f - distToPlayer / maxDist);
	}
}
