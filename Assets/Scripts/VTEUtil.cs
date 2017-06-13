using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class VTEUtil {
	public const float infoSpeedUnitsPerSec = 1f;
	const float SMALL = 0.0001f;
	static Player localPlayer;

	static public float GetDistToLocalPlayer(Vector3 pos) {
		return Vector2.Distance (pos, GetLocalPlayer().GetActualPosition());
	}
		
	static public float GetTime() {
		return Time.time;
	}

	static public float GetApparentTime(float distToLocalPlayer) {
		float transmissionSecsToPlayer = distToLocalPlayer / infoSpeedUnitsPerSec;
		return GetTime() - transmissionSecsToPlayer;
	}

	static public float GetApparentTime(Vector2 myStartPos, Vector2 myEndPos, float myStartTime, float mySpeed, Vector2 otherPos) { 
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
		// Thus, g(t) = g(T) + V * (t - T), where T = the time of the command and V is the player's movement velocity, V < C
		// Let (A,B) := g(T) and (S,W) := V
		// Then (x,y) = (A,B) + (S,W) * (t - T)
		// =>           (X - (A + St - ST))^2 + (Y - (B + Wt - WT))^2 = C^2 * (NOW^2 - 2t * NOW + t^2)    (from Equation 2)
		// =>           (X - A + ST - St)^2 + (Y - B + WT - Wt)^2 = C^2 * (NOW^2 - 2t * NOW + t^2)
		// Let K := X - A + ST and L := Y - B + WT. Then
		// =>           (K - St)^2 + (L - Wt)^2 = C^2 * (NOW^2 - 2t * NOW + t^2)
		// =>           K^2 - 2KSt + S^2*t^2 + L^2 - 2LWt + W^2*t^2 = C^2*NOW^2 - 2*C^2*NOW*t + C^2*t^2
		// =>           (K^2 + L^2 - C^2*NOW^2) + (2*C^2*NOW - 2KS - 2LW)t + (S^2 + W^2 - C^2)t^2 = 0     (Call this Equation 1)
		// Let a := (S^2 + W^2 - C^2), b := (2*C^2*NOW - 2KS - 2LW), c := (K^2 + L^2 - C^2*NOW^2)
		// => t = (-b +/- sqrt( b^2 - 4ac)) / 2a 														  (Quadratic formula -- a != 0 because V < C)

		// If S & W are zero, we get the special case used in VEUtil from Equation 1:
		// => a = K^2 + L^2 - C^2*NOW^2, b = 2*C^2*NOW, c := -C^2
		// => a = -C^2, b = 2*C^2*NOW, c := Dist(g(NOW), f(NOW))^2 - C^2*NOW^2
		// => t = (-2*C^2*NOW +/- sqrt( (2*C^2*NOW)^2 + 4C^2(Dist(g(NOW), f(NOW))^2 - C^2*NOW^2))) / -2C^2  (Quadratic formula)
		// => t = (-2*C^2*NOW +/- 2C * sqrt( C^2*NOW^2 + Dist(g(NOW), f(NOW))^2 - C^2*NOW^2)) / -2C^2
		// => t = (-2*C^2*NOW +/- 2C * sqrt( Dist(g(NOW), f(NOW))^2 )) / -2C^2
		// => t = (-2*C^2*NOW +/- 2C * Dist(g(NOW), f(NOW)) ) / -2C^2
		// => t = NOW +/- Dist(g(NOW), f(NOW)) / C

		// If g = f (that is, if we are determine the apparent time wrt to ourselves), then
		// g(t) = g(T) + V * (t - T)
		// => f(t) = g(T) + V * (t - T)
		// => f(NOW) = g(T) + V * (NOW - T)
		// => (X,Y) = (A,B) + (S,W) * (NOW - T)
		// => (X - A, Y - B) = (NOW * S - ST, NOW * W - WT)
		// => (X - A + ST, Y - B + WT) = (NOW * S, NOW * W)
		// => (K, L) = (NOW * S, NOW * W)		(call this Equation 4)
		// For the quadratic formula, we need
		// a := (S^2 + W^2 - C^2), b := (2*C^2*NOW - 2KS - 2LW), c := (K^2 + L^2 - C^2*NOW^2)
		//                         b := -2NOW(S^2 + W^2 - C^2),  c := NOW^2(S^2 + W^2 - C^2)							(by Equation 4)
		//                         b := -2NOWa, c := NOW^2a,     c = NOW^2a
		// t = (-b +/- sqrt(b^2 - 4ac))/2a
		// t = (2NOWa +/- sqrt((2NOWa)^2 - 4aNOW^2a))/2a
		// t = (2NOWa +/- sqrt(4NOW^2a^2 - 4NOW^2a^2))/2a
		// t = 2NOWa/2a
		// t = NOW

		// What about V == C?
		// c + bt + at^2 = 0     							
		// c + bt = 0 											    													(V == C => a = 0)
		// t = -c/b																										(unless b == 0)
		// What if a = b = 0?
		// => c = 0	, which is not obviously a contradiction but makes t impossible to solve for. I guess we give up at that point.				

		Vector2 movementDir = (myEndPos - myStartPos).normalized;
		float S = mySpeed * movementDir.x;
		float W = mySpeed * movementDir.y;
		float K = otherPos.x - myStartPos.x + S * myStartTime;
		float L = otherPos.y - myStartPos.y + W * myStartTime;
		float C = infoSpeedUnitsPerSec;
		float NOW = GetTime ();
		float a = S * S + W * W - C * C;
		float b = 2f * C * C * NOW - 2f * K * S - 2f * L * W;
		float c = K * K + L * L - C * C * NOW * NOW;
		float square = b * b - 4f * a * c;
		if (square < 0 && square > -SMALL) {
			square = 0;
		}
		float t;
		if (Mathf.Abs(a) == 0) {
			UnityEngine.Assertions.Assert.IsFalse (b == 0);
			t = -c / b;	
		}			
		t = (-b + Mathf.Sqrt (square)) / (2 * a); // question: Do we ever need -b - ..., or just -b + ...? What is the -b root, physically?
		return t;
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
