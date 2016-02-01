using System;
using UnityEngine;

/// Synchronizes car position/rotation between peers and performs dead
/// reckoning.
/// 
/// If car is controlled by local player, reads position/rotation/input
/// data from Transform and CarInput and sends to peers.
/// 
/// If it's a remote car, receives data from network to cache correct
/// position/rotation, which are smoothly interpolated, and set input
/// values on CarInput, which performs dead reckoning between synchonization
/// frames.
public class NetworkCar : Photon.MonoBehaviour
{
	// the CarInput to read/write input data from/to
    private CarInput carInput;
	private Rigidbody rb;

	// cached values for correct position/rotation (which are then interpolated)
	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;
	private Vector3 currentVelocity;
	private float updateTime = 0;
    private VehicleController vehicleController;
    // the physics body of the vehicle
    private GameObject physicsBody;
    private void Awake()
    {
        carInput = GetComponent<CarInput>();
    }

    private void Start()
    {
        vehicleController = GetComponentInChildren<VehicleController>();
        physicsBody = GetComponentInChildren<VehicleController>().physicsBody;
		rb = physicsBody.GetComponent<Rigidbody>();
    }

    public void Fire()
    {

        if(photonView.isMine == false) return;
        int iniSpawn = photonView.viewID;
//        Debug.Log("im going to fire:"+nameShooter);
        photonView.RPC (
                "StartFire",
                PhotonTargets.All,
                iniSpawn
        );
    }

    /// <summary>
    ///通过 PhotonView.viewID确定目标玩家
    /// </summary>
    [PunRPC]
    public void StartFire(int viewID)
    {
//        Debug.Log("RPC needs me to fire:"+nameShooter);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag ("Player"))
        {
                if(go.GetComponent<CarRaceControl>().GetViewID.Equals(viewID))
                {
//                    Debug.Log("im fired!:"+nameShooter);
                    go.GetComponentInChildren<WeaponSystem>().BiuBiuBiu();
                }
        }
    }

    public void Boom(int viewID)
    {
        //炸没炸到 主机说的算 其他client只看个热闹
        if(PhotonNetwork.isMasterClient)
        {
//            Debug.Log("im going to boom!!!!!:"+iniSpawn);
            photonView.RPC (
                    "BoomSomeone",
                    PhotonTargets.All,
                    viewID
            );
        }

    }

    [PunRPC]
    public void BoomSomeone(int viewID) {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            if (go.GetComponent<CarRaceControl>().GetViewID.Equals(viewID))
                go.GetComponentInChildren<CarExploder>().trigger.Exploder();
        }
    }

	/// if it is a remote car, interpolates position and rotation
    /// received from network
    public void FixedUpdate()
    {
		if (!photonView.isMine) {
            if (vehicleController.isBoomedOrKilled) return;
			Vector3 projectedPosition = this.correctPlayerPos + currentVelocity * (Time.time - updateTime);
            physicsBody.transform.position = Vector3.Lerp(physicsBody.transform.position, projectedPosition, Time.deltaTime * 4);
            physicsBody.transform.rotation = Quaternion.Lerp(physicsBody.transform.rotation, this.correctPlayerRot, Time.deltaTime * 4);
		}
	}


	/// At each synchronization frame, sends/receives player input, position
	/// and rotation data to/from peers/owner
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) {
			// we own this car: send the others our input and transform data
            stream.SendNext((float)carInput.Steer);
			stream.SendNext((float)carInput.Accell);
			stream.SendNext((float)carInput.Handbrake);
			stream.SendNext(physicsBody.transform.position);
			stream.SendNext(physicsBody.transform.rotation);
			stream.SendNext(rb.velocity);
        }
		else {
            // remote car, receive data
            carInput.Steer = (float)stream.ReceiveNext();
            carInput.Accell = (float)stream.ReceiveNext();
            carInput.Handbrake = (float)stream.ReceiveNext();
			correctPlayerPos = (Vector3)stream.ReceiveNext();
			correctPlayerRot = (Quaternion)stream.ReceiveNext();
			currentVelocity = (Vector3)stream.ReceiveNext();
            updateTime = Time.time;
		} 
	}
}

