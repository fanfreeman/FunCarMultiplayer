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
    private CarInput m_CarInput;
	private Rigidbody rb;

	// cached values for correct position/rotation (which are then interpolated)
	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;
	private Vector3 currentVelocity;
	private float updateTime = 0;

    // the physics body of the vehicle
    private GameObject physicsBody;

    private void Start()
    {
		m_CarInput = GetComponent<CarInput>();
        physicsBody = GetComponentInChildren<VehicleController>().physicsBody;
		rb = physicsBody.GetComponent<Rigidbody>();
    }

	/// if it is a remote car, interpolates position and rotation
    /// received from network
    public void FixedUpdate()
    {
		if (!photonView.isMine) {
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
			stream.SendNext((float)m_CarInput.Steer);
			stream.SendNext((float)m_CarInput.Accell);
			stream.SendNext((float)m_CarInput.Handbrake);
            stream.SendNext(physicsBody.transform.position);
			stream.SendNext(physicsBody.transform.rotation);
			stream.SendNext(rb.velocity);
            stream.SendNext((float)m_CarInput.Fire);
        }
		else {
			// remote car, receive data
			m_CarInput.Steer = (float)stream.ReceiveNext();
			m_CarInput.Accell = (float)stream.ReceiveNext();
			m_CarInput.Handbrake = (float)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
			correctPlayerRot = (Quaternion)stream.ReceiveNext();
			currentVelocity = (Vector3)stream.ReceiveNext();
            m_CarInput.Fire = (float)stream.ReceiveNext();
            updateTime = Time.time;
		} 
	}
}

