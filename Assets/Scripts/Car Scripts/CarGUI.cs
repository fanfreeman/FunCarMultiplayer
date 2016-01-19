using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// For the local car only. Updates GUI information based on computed position and lap
/// (from CarCareControl), speed (from rigidbody), etc
public class CarGUI : Photon.MonoBehaviour {
	
	// GUI references to update on race
	public Text speedGUI;
	public Text positionGUI;
	public Text lapGUI;
	public Text messagesGUI;
	
	//private Rigidbody m_rigidbody;
	private CarRaceControl carRaceControl;

    private Rigidbody carRigidBody;
	
	// register car with manager
	// get and manage waypoints
	void Start () {
		carRaceControl = GetComponent<CarRaceControl>();
        carRigidBody = GetComponentInChildren<VehicleController>().physicsBody.GetComponent<Rigidbody>();
	}
	
	void Update () {
        speedGUI.text = ((int)(carRigidBody.velocity.magnitude * 2.23693629f * 1.6f)) + "";
        lapGUI.text = (carRaceControl.lapsCompleted + 1) + "/" + carRaceControl.totalLaps;
		positionGUI.text = "" + carRaceControl.currentPosition;
		// wait for first checkpoint, so the GO! message isn't hidden by this
		if (carRaceControl.waypointsPassed > 0) {
			if (carRaceControl.WrongWay) {
				messagesGUI.text = "Wrong Way!";
			} else {
				messagesGUI.text = "";
			}
		}
	}
}
