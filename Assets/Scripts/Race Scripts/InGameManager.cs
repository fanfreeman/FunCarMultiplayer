using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon;

/// Manages the race gameplay, instantiating player car and sending/receiving
/// RPCs for race start/finish, timer, etc.
public class InGameManager: PunBehaviour {
    
	public Text lapGUI;
	public Text positionGUI;
	public Text speedGUI;
	public Text messagesGUI;
	public Sprite noCar;
    public Text[] playerNames;
    public Text[] playerDeaths;
	// reference to local player car
	private GameObject car;

	// list of all player´s cars (for position calculation)
	[HideInInspector]
    public List<CarRaceControl> carControllers = new List<CarRaceControl> ();
    public Dictionary<int, int> deathCounter;

    public float raceTime = 0;
	public double startTimestamp = 0;

	private RaceState state = RaceState.PRE_START;

	// all spawn points for track (uses one of them for player car)
	public Transform[] spawnPoints;
	
	void Start () {
		Debug.Log ("Created car");
        deathCounter = new Dictionary<int, int>();
		CreateCar();
	}

	void Update () {
		raceTime += Time.deltaTime;
		switch (state) {
		case RaceState.PRE_START:
			if (PhotonNetwork.isMasterClient && raceTime >= 1) {
				photonView.RPC ("StartCountdown", PhotonTargets.All, PhotonNetwork.time + 4);
			}
			break;
		case RaceState.COUNTDOWN:
			messagesGUI.text = "" + (1 + (int) (startTimestamp - PhotonNetwork.time));
			if (PhotonNetwork.time >= startTimestamp) {
				StartRace();
			}
			break;
		case RaceState.RACING:
			if (raceTime > 3) {
				messagesGUI.text = "";
			} else {
				messagesGUI.text = "GO!";
			}
			break;
		}
		// compute positions locally...
		bool swap = Sort(ref carControllers);
		int position = 1;
		foreach(CarRaceControl c in carControllers) {
			c.currentPosition = position;

			// for updating the player order GUI
			playerNames[position - 1].text = c.photonView.owner.name;
            playerDeaths[position - 1].text = (deathCounter[c.GetPlayerID]).ToString();
			position++;
		}
	}

    //reset carControllers every reborn
    public void ResetCarControllers()
    {
        carControllers.Clear();
        GameObject[] cars = GameObject.FindGameObjectsWithTag ("Player");
        foreach (GameObject go in cars)
        {
            if(!go.GetComponentInChildren<VehicleController>().isBoomedOrKilled)
            {
                CarRaceControl carRaceControl = go.GetComponent<CarRaceControl>();
                go.GetComponent<CarInput>().enabled = true;
                carControllers.Add(carRaceControl);
                if (!deathCounter.ContainsKey(carRaceControl.GetPlayerID))
                {
                    deathCounter.Add(carRaceControl.GetPlayerID, 0);
                }
            }

        }
        carControllers.Sort();
    }

    public void ResetCarControllers(CarRaceControl carRaceControl)
    {
       // carControllers.Remove(carRaceControl);
    }

	// Instantiates player car on all peers, using the appropriate spawn point (based
	// on join order), and sets local camera target.
	private void CreateCar() {
		// gets spawn Transform based on player join order (spawn property)
		int pos = (int) PhotonNetwork.player.customProperties["spawn"];
//		int carNumber = (int) PhotonNetwork.player.customProperties["car"];
        Transform spawn = spawnPoints[pos];

		// instantiate car at Spawn Transform
		// car prefabs are numbered in the same order as the car sprites that the player chose from
        car = PhotonNetwork.Instantiate("Car", spawn.position, spawn.rotation, 0);
        //car = ((Transform)GameObject.Instantiate(carPrefab, spawn.position, spawn.rotation)).gameObject;
        // car starting race position (for GUI) is same as spawn position + 1 (grid position)
        car.GetComponent<CarRaceControl> ().currentPosition = pos + 1;

        // local car has simulated physics (remotes are kinematic by default - controlled by network)
        //car.GetComponentInChildren<VehicleController>().physicsBody.GetComponent<Rigidbody>().isKinematic = false;
        car.GetComponentInChildren<VehicleController>().cameraObject.SetActive(true);

        // enable GUI for local car
        car.GetComponent<CarGUI> ().enabled = true;
		car.GetComponent<CarGUI> ().lapGUI = lapGUI;
		car.GetComponent<CarGUI> ().positionGUI = positionGUI;
		car.GetComponent<CarGUI> ().speedGUI = speedGUI;
		car.GetComponent<CarGUI> ().messagesGUI = messagesGUI;
	}



    //死亡计分
    public void IwasBoomedBySomeOne(GameObject oldCar)
    {
        CarRaceControl _carRaceControl = oldCar.GetComponent<CarRaceControl>();
        deathCounter[_carRaceControl.GetPlayerID]++;
    }

	/// <summary>
	///	被炸重生
	/// </summary>
	/// <param name="oldCar">需要销毁的被炸了的车</param>
    public void ReBornPlayer(GameObject oldCar)
    {

        CarRaceControl _carRaceControl = oldCar.GetComponent<CarRaceControl>();
        if(_carRaceControl.photonView.isMine)
        {
            PhotonNetwork.Destroy(oldCar);

            car = PhotonNetwork.Instantiate("Car", spawnPoints[0].transform.position , spawnPoints[0].transform.rotation, 0);
            car.GetComponentInChildren<VehicleController>().cameraObject.SetActive(true);
            // enable GUI for local car
            car.GetComponent<CarGUI> ().enabled = true;
            car.GetComponent<CarGUI> ().lapGUI = lapGUI;
            car.GetComponent<CarGUI> ().positionGUI = positionGUI;
            car.GetComponent<CarGUI> ().speedGUI = speedGUI;
            car.GetComponent<CarGUI> ().messagesGUI = messagesGUI;
            car.GetComponent<CarInput>().controlable = true;
            car.GetComponent<CarInput>().enabled = true;
        }
    }
	
	public void StartRace() {
		Debug.Log ("Start");
		state = RaceState.RACING;
		GameObject[] cars = GameObject.FindGameObjectsWithTag ("Player");
        carControllers.Clear();
        deathCounter.Clear();
		foreach (GameObject go in cars) {
            CarRaceControl carRaceControl = go.GetComponent<CarRaceControl>();
			carControllers.Add(carRaceControl);
			go.GetComponent<CarInput>().enabled = true;
			go.GetComponent<CarRaceControl>().currentWaypoint = GameObject.Find("Checkpoint1").GetComponent<Checkpoint>();
            deathCounter.Add(carRaceControl.GetPlayerID, 0);
        }
		car.GetComponent<CarInput>().controlable = true;
		raceTime = 0;
	}

    //large to small n^2complex
    private static bool Sort(ref List<CarRaceControl> unsorted)
    {
        bool swp = false;
        for (int i = 0; i < unsorted.Count; i++)
        {
            for (int j = i; j < unsorted.Count; j++)
            {
                if (unsorted[i].CompareTo(unsorted[j]) > 0 )
                {
                    CarRaceControl temp = unsorted[i];
                    unsorted[i] = unsorted[j];
                    unsorted[j] = temp;
                    swp = true;
                }
            }
        }
        return swp;
    }

	[PunRPC]
	public void StartCountdown(double startTimestamp) {
		Debug.Log ("Countdown");
		state = RaceState.COUNTDOWN;
		// sets 
		this.startTimestamp = startTimestamp;
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnetedPlayer) {
		Debug.Log (disconnetedPlayer.name + " disconnected...");
		CarRaceControl toRemove = null;
		foreach (CarRaceControl rc in carControllers) {
			//Debug.Log (rc.photonView.owner);
			if (rc.photonView.owner == null) {
				toRemove = rc;
			}
		}
		// remove car controller of disconnected player from the list
		carControllers.Remove (toRemove);

		// reset names, so next frame can include only remaining players
		foreach (Text name in playerNames) {
			name.text = "";
		}
	}
}
