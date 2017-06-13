using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

public class Player : NetworkBehaviour {
	class MovementEvent {
		public bool done;
		public Vector3 startPos { get; private set; }
		public Vector3 tgtPos { get; private set; }
		public float time { get; private set; }
		public Planet tgtPlanet { get; private set; }
		public MovementEvent(Vector3 _startPos, Vector3 _tgtPos, Planet _tgtPlanet) { 
			startPos = _startPos;
			tgtPos = _tgtPos;
			time = VTEUtil.GetApparentTime();
			tgtPlanet = _tgtPlanet;
			done = _startPos == _tgtPos;
		}
		override public string ToString() {
			return "startPos = " + startPos.ToString () + " tgtPos " + tgtPos.ToString () + " time= " + time.ToString () + " planet = " + (tgtPlanet == null ? "None" : tgtPlanet.netId.ToString()) + " done = " + done.ToString();
		}
	}

	[SerializeField] float unitsPerSec = 0.25f;
	[SerializeField] Color movingColor = Color.red;
	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color originalColor;
	[SerializeField] Color enemyColor;
	[SerializeField] Vector3 OFFSCREEN = new Vector3 (-100f, -100f, 0);
	[SerializeField] DecreeCapsule decreePrefab;

	public bool selected { get; private set; }

	Material material;
	List<MovementEvent> movementEvents; // last elements are the most recent
	const float SMALL = 0.0001f;

	public void SetTargetPlanet(Planet newPlanet) {
		print ("SetTargetPlanet at " + VTEUtil.GetApparentTime());
		SetSelected (false);
		CmdSetTargetPlanet (newPlanet.netId);
		// todo: feedback that click was registered
	}

	public void SendDrones(Planet origin, Planet destination) {
		// todo: feedback that click was registered
		CmdSendDrones (origin.netId, destination.netId);
	}

	public Vector3 GetActualPosition() {
		return GetPositionAt (VTEUtil.GetApparentTime ()).Value;
	}
		
	void Start() {
		material = GetComponent<MeshRenderer> ().material;
		if (!isLocalPlayer) {
			gameObject.GetComponent<Collider> ().enabled = false;
			originalColor = enemyColor;
			material.color = enemyColor;
		}
		movementEvents = new List<MovementEvent> ();
		AddMovementEvent (transform.position, transform.position);
		UpdateColor ();
	}

	void Update() {
		if (isServer) {
			CheckForArrival ();
		}
		if (isClient) {
			UpdateApparentPosition ();
		}
	}

	void CheckForArrival() {
		Assert.IsTrue (isServer);
		if (IsMoving () && GetActualPosition () == GetTgtPos ()) { 
			RpcEndMovement (GetCurrentMovementEventIdx ());
		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawCube (GetActualPosition(), new Vector3 (0.2f, 0.2f, 0.2f));
	}

	int GetCurrentMovementEventIdx () {
		return movementEvents.Count - 1;
	}

	MovementEvent GetCurrentMovementEvent() {
		return movementEvents [GetCurrentMovementEventIdx()];
	}

	bool IsMoving() {
		return !GetCurrentMovementEvent ().done;
	}
		
	Vector3 GetTgtPos() {
		return GetCurrentMovementEvent().tgtPos;
	}

	void OnMouseUpAsButton() {
		SetSelected (!selected);
	}

	void SetSelected (bool b) {
		selected = b;
		UpdateColor ();
	}

	void UpdateColor() {
		if (!isLocalPlayer) {
			return;
		}
		if (selected) {
			material.color = selectedColor;
		} else if (IsMoving() ) {
			material.color = movingColor;
		} else {
			material.color = originalColor;
		}
	}


	[Command] void CmdSetTargetPlanet(NetworkInstanceId planetId) {
		print ("CmdSetTgtPlanet " + this.netId + " to planet " + planetId);
		RpcStartMovement (planetId, GetActualPosition());
	}

	[Command] void CmdSendDrones(NetworkInstanceId originPlanetId, NetworkInstanceId targetPlanetId) {
		print ("CmdSendDrones from " + originPlanetId + " to " + targetPlanetId + " / " + GetActualPosition());
		DecreeCapsule decreeCapsule = (DecreeCapsule)GameObject.Instantiate (decreePrefab);
//		Planet originPlanet = GetPlanetFromId (originPlanetId);
//		Planet tgtPlanet = GetPlanetFromId (targetPlanetId);
//		decreeCapsule.Init(new SendDronesDecree(originPlanet, tgtPlanet), originPlanet.transform.position);
		NetworkServer.Spawn (decreeCapsule.gameObject);
		RpcSendDrones (originPlanetId, targetPlanetId, GetActualPosition(), decreeCapsule.netId);
	}

	[ClientRpc] void RpcStartMovement(NetworkInstanceId planetId, Vector3 startPos) { // don't rely on actualPosition being synched at exactly this moment
		print ("RpcStartMovement ");
		Planet tgtPlanet = GetPlanetFromId (planetId);
		Vector3 tgtPos = tgtPlanet.GetParkingSpace (this.netId);
		AddMovementEvent(startPos, tgtPos, tgtPlanet); 
		UpdateColor ();
	}

	[ClientRpc] void RpcEndMovement(int i) {
		MovementEvent movementEvent = movementEvents [i];
		print ("Arrive at " + VTEUtil.GetApparentTime() + " after " + (VTEUtil.GetApparentTime() - movementEvent.time));
		print ("Completing movement " + i.ToString () + " : " + movementEvent.ToString ());
		movementEvent.done = true;
		movementEvent.tgtPlanet.Conquer (this.netId);
		UpdateColor ();
	}
		
	[ClientRpc] void RpcSendDrones(NetworkInstanceId originPlanetId, NetworkInstanceId targetPlanetId, Vector3 startPos, NetworkInstanceId decreeId) {
		Planet originPlanet = GetPlanetFromId (originPlanetId);
		Planet tgtPlanet = GetPlanetFromId (targetPlanetId);
		print ("RpcSendDrones from " + originPlanetId + " to " + targetPlanetId + " / " + startPos + " to " +  originPlanet.transform.position);
		DecreeCapsule decreeCapsule = ClientScene.FindLocalObject (decreeId).GetComponent<DecreeCapsule> ();
		decreeCapsule.transform.position = startPos;
		decreeCapsule.Init(new SendDronesDecree(originPlanet, tgtPlanet), originPlanet.transform.position);
	}

	Planet GetPlanetFromId(NetworkInstanceId planetId) {
		Assert.IsTrue (isClient);
		return ClientScene.FindLocalObject (planetId).GetComponent<Planet> ();
	}

	void UpdateApparentPosition() {		
//		print("UpdateApparentPosition");
		float time = GetApparentTime (VTEUtil.GetLocalPlayer());
		Vector3? newPosition = GetPositionAt (time);
		transform.position = newPosition.HasValue ? newPosition.Value : OFFSCREEN;
//		print ("Apparent pos " + transform.position.ToString() + " Actual pos " + actualPosition.ToString());
	}

	Vector3? GetPositionAt(float time) {
		MovementEvent lastDeparture = movementEvents.FindLast( movementEvent => movementEvent.time < time );
		if (lastDeparture == null) {
			return null;
		}
		float timeRequired = Vector3.Distance(lastDeparture.startPos, lastDeparture.tgtPos) / unitsPerSec;
		float fractionCompleted = (time - lastDeparture.time) / timeRequired;
//		print (lastDeparture);
//		print("Apparent Time" + time.ToString() + " Actual time " + VTEUtil.GetApparentTime() );
//		print ("Fraction completed = " + fractionCompleted);
		return Vector3.Lerp (lastDeparture.startPos, lastDeparture.tgtPos, fractionCompleted);
	}

	void AddMovementEvent(Vector3 startPosition, Vector3 tgtPosition, Planet tgtPlanet = null) {
		movementEvents.Add (new MovementEvent (startPosition, tgtPosition, tgtPlanet));
		print ("Creating Movement Event for player " + this.netId.Value.ToString () + " : total = " + movementEvents.Count);
		print (movementEvents [movementEvents.Count - 1].ToString ());			
	}
		
	float GetApparentTime(Player otherPlayer) { 
		// for two moving players, their positions given by f(t) = other player's position at time t, and g(t) = my position at time t, 
		// we want to find the latest t such that the other player can see me now.
		//                                     => information that leaves g(t) at time t reaches f(NOW) at time NOW
		//                                     => t + h(t) = NOW, where we define h(t) as the time it takes for information to travel between g(t) and f(NOW).
		// h(t) = Distance(f(NOW), g(t)) / C, where C is the constant speed of information.
		// Let (X, Y) := f(NOW) and (x,y) := g(t)
		// So t + h(t)                                                     = NOW 
		//        h(t)                                                     = NOW - t
		// =>     sqrt( (X - x)^2 + (Y - y)^2) / C = NOW - t
		// =>           (X - x)^2 + (Y - y)^2      = C^2 * (NOW - t)^2                                    (call it Equation 2)
		// For a given movement command, movement is in a straight line at a constant speed.
		// Thus, g(t) = g(T) + V * (t - T), where T = the time of the command and V is the player's movement velocity.
		// Let (A,B) := g(T) and (S,W) := V
		// Then (x,y) = (A,B) + (S,W) * (t - T)
		// =>           (X - (A + St - ST))^2 + (Y - (B + Wt - WT))^2 = C^2 * (NOW^2 - 2t * NOW + t^2)    (from Equation 2)
		// =>           (X - A + ST - St)^2 + (Y - B + WT - Wt)^2 = C^2 * (NOW^2 - 2t * NOW + t^2)
		// Let K := X - A + ST and L := Y - B + WT. Then
		// =>           (K - St)^2 + (L - Wt)^2 = C^2 * (NOW^2 - 2t * NOW + t^2)
		// =>           K^2 - 2KSt + S^2*t^2 + L^2 - 2LWt + W^2*t^2 = C^2*NOW^2 - 2*C^2*NOW*t + C^2*t^2
		// =>           (K^2 + L^2 - C^2*NOW^2) + (2*C^2*NOW - 2KS - 2LW)t + (S^2 + W^2 - C^2)t^2 = 0     (Call this Equation 1)
		// Let a := (S^2 + W^2 - C^2), b := (2*C^2*NOW - 2KS - 2LW), c := (K^2 + L^2 - C^2*NOW^2)
		// => t = (-b +/- sqrt( b^2 - 4ac)) / 2a  (Quadratic formula)

		// If S & W are zero, we get the special case used in VEUtil from Equation 1:
		// =>           (K^2 + L^2 - C^2*NOW^2) + (2*C^2*NOW)t - C^2*t^2 = 0
		// =>           ((X - A)^2 + (Y - B)^2 - C^2*NOW^2) + (2*C^2*NOW)t - C^2*t^2 = 0
		// =>           Dist(g(NOW), f(NOW))^2 = C^2*NOW^2 - (2*C^2*NOW)t + C^2*t^2
		// =>           Dist(g(NOW), f(NOW))^2 / C^2 = NOW^2 - (2*NOW)t + t^2
		// =>           Dist(g(NOW), f(NOW))^2 / C^2 = (NOW - t)^2
		// =>           Dist(g(NOW), f(NOW) / C = NOW - t
		// =>  t =  NOW - Dist(g(NOW), f(NOW) / C
		// or, using the quadratic formula:
		// => a = K^2 + L^2 - C^2*NOW^2, b = 2*C^2*NOW, c := -C^2
		// => a = -C^2, b = 2*C^2*NOW, c := Dist(g(NOW), f(NOW))^2 - C^2*NOW^2
		// => t = (-2*C^2*NOW +/- sqrt( (2*C^2*NOW)^2 + 4C^2(Dist(g(NOW), f(NOW))^2 - C^2*NOW^2))) / -2C^2  (Quadratic formula)
		// => t = (-2*C^2*NOW +/- 2C * sqrt( C^2*NOW^2 + Dist(g(NOW), f(NOW))^2 - C^2*NOW^2)) / -2C^2
		// => t = (-2*C^2*NOW +/- 2C * sqrt( Dist(g(NOW), f(NOW))^2 )) / -2C^2
		// => t = (-2*C^2*NOW +/- 2C * Dist(g(NOW), f(NOW)) ) / -2C^2
		// => t = NOW +/- Dist(g(NOW), f(NOW)) / C

		// If g = f (that is, if we are determine the apparent time wrt to ourselves), then
		// consider the two-dimensional version:
		// g(t) = g(T) + S * (t - T)
		// => f(t) = g(T) + S * (t - T)  		(since f = g)
		// => f(NOW) = g(T) + S * (NOW - T)
		// => X = A + S * (NOW - T)
		// => X - A + ST = SNOW		 			(call this Equation 3)
		// and
		// h(t) = NOW - t
		// Distance(f(NOW), g(t)) / C = NOW - t
		// t = NOW - |X - x| / C
		// t = NOW - |X - (A + S * (t - T))| / C
		// t = NOW - |X - A + ST - St| / C
		// t = NOW - |SNOW - St| / C			(Equation 3)
		// t = NOW - |S(NOW - t)| / C  			
		// Ct = CNOW - SNOW + St				(since S >= 0 and NOW >= t)
		// t = NOW * (C - S) / (C - S)
		// t = NOW as expected (unless C = S).

		// now try it in three dimensions:
		// g(t) = g(T) + V * (t - T)
		// => f(t) = g(T) + V * (t - T)
		// => f(NOW) = g(T) + V * (NOW - T)
		// => (X,Y) = (A,B) + (S,W) * (NOW - T)
		// => (X - A, Y - B) = (NOW * S - ST, NOW * W - WT)
		// => (X - A + ST, Y - B + WT) = (NOW * S, NOW * W)
		// => (K, L) = (NOW * S, NOW * W)		(call this Equation 4)
		// and
		// h(t) = NOW - t
		// Distance(f(NOW), g(t)) / C = NOW - t
		// t = NOW - Sqrt((X - x)^2 + (Y - y)^2) / C
		// t = NOW - Sqrt((X - (A + S * (t - T)))^2 + (Y - (B + W * (t - T)))^2) / C
		// t = NOW - Sqrt((X - A - St + ST)^2 + (Y - B - Wt + WT)^2) / C
		// t = NOW - Sqrt((SNOW - St)^2 + (WNOW - Wt)^2) / C								(Equation 4)
		// C(NOW - t) = Sqrt((SNOW - St)^2 + (WNOW - Wt)^2)
		// C^2(NOW - t)^2 = (SNOW - St)^2 + (WNOW - Wt)^2
		// C^2NOW^2 - 2C^2NOWt + C^2t^2 = S^2NOW^2 - 2S^2tNOW + S^2t^2 + W^2NOW^2 - 2W^2tNOW + W^2t^2
		// NOW^2(C^2 - S^2 - W^2) - 2NOW(C^2 - W^2 - S^2)t + (C^2 - S^2 - W^2)t^2 = 0
		// if the ship is not travelling at lightspeed, then 0 != (C^2 - S^2 - W^2), so
		// NOW^2 - 2NOW * t + t^2 = 0		
		// (t - NOW)^2 = 0
		// t = NOW as expected.
		// Now with the quadratic formula:
		// a := (S^2 + W^2 - C^2), b := (2*C^2*NOW - 2KS - 2LW), c := (K^2 + L^2 - C^2*NOW^2)
		// b := -2NOW(S^2 + W^2 - C^2), c := NOW^2(S^2 + W^2 - C^2)
		// b := -2NOWa, c := NOW^2a, c = NOW^2a
		// t = (-b +/- sqrt(b^2 - 4ac))/2a
		// t = (2NOWa +/- sqrt((2NOWa)^2 - 4aNOW^2a))/2a
		// t = (2NOWa +/- sqrt(4NOW^2a^2 - 4NOW^2a^2))/2a
		// t = 2NOWa/2a
		// t = NOW

		MovementEvent movementEvent = GetCurrentMovementEvent();

		Vector2 movementDir = ((Vector2)(movementEvent.tgtPos - movementEvent.startPos)).normalized;
		float S = unitsPerSec * movementDir.x;
		float W = unitsPerSec * movementDir.y;
		Vector3 otherPos = otherPlayer.GetActualPosition ();
		float K = otherPos.x - movementEvent.startPos.x + S * movementEvent.time;
		float L = otherPos.y - movementEvent.startPos.y + W * movementEvent.time;
		float C = VTEUtil.infoSpeedUnitsPerSec;
		float NOW = VTEUtil.GetApparentTime ();
		float a = S * S + W * W - C * C;
		float b = 2 * C * C * NOW - 2 * K * S - 2 * L * W;
		float c = K * K + L * L - C * C * NOW * NOW;
		float square = b * b - 4 * a * c;
		if (square < 0 && square > -SMALL) {
			square = 0;
		}
		// TODO Special case for unitsPerSec = C 
		// => a = 0 
		// =>  t = -(K^2 + L^2 - C^2*NOW^2) / (2*C^2*NOW - 2KS - 2LW) for (C^2*NOW != KS + LW), in which case TODO

		float t = (-b - Mathf.Sqrt (square)) / (2 * a);
		return t;
	}
}
