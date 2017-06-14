using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking
;
public class StartButton : NetworkBehaviour {

	// Use this for initialization
	public void StartGame () {
		foreach (GameObject player in GameObject.FindGameObjectsWithTag ("Player")) {
			player.GetComponent<Player>().enabled = true;
		}
		gameObject.SetActive (false);
	}
}
