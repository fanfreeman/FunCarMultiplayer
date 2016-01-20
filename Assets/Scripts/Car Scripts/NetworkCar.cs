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

	/// if it is a remote car, interpolates position and rotation
    /// received from network
    public void FixedUpdate()
    {
		if (!photonView.isMine) {
            if (vehicleController.isDestoryed) return;
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
            stream.SendNext((float)carInput.Fire);
            stream.SendNext((bool)carInput.IsMyCarBoomed);
        }
		else {
            // remote car, receive data
            carInput.Steer = (float)stream.ReceiveNext();
            carInput.Accell = (float)stream.ReceiveNext();
            carInput.Handbrake = (float)stream.ReceiveNext();
			correctPlayerPos = (Vector3)stream.ReceiveNext();
			correctPlayerRot = (Quaternion)stream.ReceiveNext();
			currentVelocity = (Vector3)stream.ReceiveNext();
            carInput.Fire = (float)stream.ReceiveNext();
            carInput.IsMyCarBoomed = ((bool)carInput.IsMyCarBoomed);
            updateTime = Time.time;
		} 
	}
}

