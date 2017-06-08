using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class VTEUtil {
	static float infoSpeedUnitsPerSec = 1f;
	static Player localPlayer;


	static public float GetDistToLocalPlayer(Vector3 pos) {
		return Vector2.Distance (pos, GetLocalPlayer().actualPosition);
	}
		
	static public float GetApparentTime(float distToLocalPlayer) {
		float transmissionSecsToPlayer = distToLocalPlayer / infoSpeedUnitsPerSec;
		return Time.time - transmissionSecsToPlayer;
	}

	static public Player GetLocalPlayer() {
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
