using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// Controls how far the car has traveled into the race 
/// (by counting laps/checkpoints and distance to next checkpoint).
/// Uses this information in CompareTo, so cars can be ordered by this distance,
/// effectively computing their relative positions on the track (by PUNRaceManager)
public class CarRaceControl : Photon.MonoBehaviour, IComparable<CarRaceControl> {
	
	public Checkpoint currentWaypoint;

	public int lapsCompleted = 0;
	public int totalLaps = 2;
	public float distanceTraveled = 0;
	public int currentPosition = 5;
	public int waypointsPassed = 0;
    public TextMesh playerNameMesh;
    public string GetPlayerName{
        get { return photonView.owner.name; }
    }
	/* Compare the desired direction (towards next checkpoint) with current 
	 * car heading (using dot product).
	 * If dot is less than 0, car is poiting wrong way, even slightly.
	 * Same principle is used for upside-down computation.
	 */
	public bool WrongWay {
		get {
			Vector3 correctDirection = currentWaypoint.transform.position - transform.position;
			if (Vector3.Dot(transform.forward, correctDirection) < 0) {
				return true;
			}
			return false;
		}
	}

	// register car with manager
	// get and manage waypoints
	void Start () {
		currentWaypoint = GameObject.Find ("Checkpoint1").GetComponent<Checkpoint>();
        //显示敌对玩家的名字！！！！！
        if(!photonView.isMine)
        	playerNameMesh.text = photonView.owner.name;
	}

	void Update () {
		UpdateDistanceTraveled ();
	}

	// called when passing a race checkpoint
	// counts laps and disables car controls at the end of the race
	void OnTriggerEnter(Collider other) {
		Checkpoint waypoint = other.GetComponent<Checkpoint>();
		if (waypoint != currentWaypoint) {
			// not always wrong (3 colliders, the first replaces the current checkpoint, the other 2 think its wrong
			//Debug.Log ("Wrong checkpoint - " + other.gameObject.name);
		} else {
			waypointsPassed++;
			if (waypoint.isStartFinish)
				lapsCompleted++;
			
			if (lapsCompleted == totalLaps) {
				CarInput input = GetComponent<CarInput> ();
				input.controlable = false;
				input.Steer = 0;
				input.Handbrake = 1;
				input.Accell = 0;
			}
			Debug.Log ("Checkpoint - " + other.gameObject.name);
			currentWaypoint = waypoint.next;
		}

	}
    
	// Uses waypoints passed + distance to next to compute how far the car has traveled into the race
	void UpdateDistanceTraveled() {
		Vector3 distance = transform.position - currentWaypoint.transform.position;
		distanceTraveled = 1000 * waypointsPassed + (1000 - distance.magnitude);
	}



	/// <summary>
	/// Compares to other CarRaceControl, by using distance traveled, so cars can be ordered (position).
	/// </summary>
	/// <returns>The to.</returns>
	/// <param name="other">Other.</param>
	public int CompareTo(CarRaceControl other)
	{
		if (distanceTraveled > other.distanceTraveled)
			return -1;
		else if (distanceTraveled < other.distanceTraveled)
			return 1;
		else{
            if (other == null || this == null) return -1;
            //如果相等就用名字来排序吧
            return other.photonView.owner.name.CompareTo(
                    photonView.owner.name
            );
        }

	}

	void OnDestroy()
    {
        GameObject.Find("PUNManager").GetComponent<InGameManager>().ResetCarControllers(this);
    }
	
}