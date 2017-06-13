using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decree {
	public static IEnumerator Send(string decreeType, Vector3 startPos, Planet originPlanet, Planet targetPlanet) {
		Debug.Log ("Send decree " + decreeType + " from " + originPlanet.netId + " to " + targetPlanet.netId + " at " + VTEUtil.GetTime());

		float travelTime = Vector2.Distance(startPos, originPlanet.transform.position) / DecreeCapsule.unitsPerSec;
		yield return new WaitForSeconds (travelTime);
		Execute (decreeType, originPlanet, targetPlanet);
	}

	static void Execute(string decreeType, Planet origin, Planet target) {
		Debug.Log ("Executing " + decreeType + " from " + origin.netId + " to " + target.netId + " at " + VTEUtil.GetTime());
	}
}
