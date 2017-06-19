using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameOverController : NetworkBehaviour {
	[SerializeField] Text gameOverText;

	void Start() {
		Planet.OnPlanetConquered += CheckGameOver;
	}

	void CheckGameOver() {
		var planets = GameObject.FindGameObjectsWithTag ("Planet");
		NetworkInstanceId firstOwnerId = GetOwnerId (planets.First ());
		if (firstOwnerId == NetworkInstanceId.Invalid) {
			return;
		}
		int numOwnedByFirstOwner = planets.Count (oPlanet => GetOwnerId (oPlanet) == firstOwnerId);
		if (numOwnedByFirstOwner == planets.Count ()) {
			GameOver (firstOwnerId);
		}
	}

	NetworkInstanceId GetOwnerId(GameObject oPlanet) {
		return oPlanet.GetComponent<Planet> ().GetOwnerIdAt (VTEUtil.GetTime ());
	}

	void GameOver(NetworkInstanceId winnerId) {
		UnityEngine.Assertions.Assert.IsTrue (isClient);
		bool youWin = winnerId == VTEUtil.GetLocalPlayerComponent<NetworkBehaviour> ().netId;
		gameOverText.text = youWin ? "You win!" : "You lose!";
		gameOverText.color = youWin ? Color.green : Color.red;
	}
}
