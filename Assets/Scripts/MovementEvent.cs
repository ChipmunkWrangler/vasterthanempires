using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementEvent {
	public bool done;
	public Vector3 startPos { get; private set; }
	public Vector3 tgtPos { get; private set; }
	public float time { get; private set; }
	public Planet tgtPlanet { get; private set; }
	public MovementEvent(Vector3 _startPos, Vector3 _tgtPos, Planet _tgtPlanet) { 
		startPos = _startPos;
		tgtPos = _tgtPos;
		time = VTEUtil.GetTime();
		tgtPlanet = _tgtPlanet;
		done = _startPos == _tgtPos;
	}
	override public string ToString() {
		return "startPos = " + startPos.ToString () + " tgtPos " + tgtPos.ToString () + " time= " + time.ToString () + " planet = " + (tgtPlanet == null ? "None" : tgtPlanet.netId.ToString()) + " done = " + done.ToString();
	}
}
