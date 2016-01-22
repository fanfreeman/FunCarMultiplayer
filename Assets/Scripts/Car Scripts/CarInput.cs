using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

/// Replaces standard Unity's CarInput. Caches player input values,
/// so they can be synchronized over the network for dead reckoning 
/// physics of remote cars.
/// 
/// Relies on Unity's standard CarController for basic car physics.
/// 
/// Used by NetworkClass when sending/receiving input to/from remote cars.
public class CarInput : MonoBehaviour
{
	// our car controller
	private VehicleController car;

	// player input properties
	public float Steer { get; set; }
    public float Accell { get; set; }
	public float Handbrake { get; set; }

    // when true, car will be controlled locally (remotely otherwise)
	public bool controlable = false;

	void Start()
	{
		car = GetComponentInChildren<VehicleController>();
	}
	
	void FixedUpdate()
	{
		// if car is locally controllable, read and cache player input
		if (controlable) {
			GetInput();
		}
		// allways move the car, independently of where the input values
		// came from (local player or remote)
		ApplyInput ();
	}

	// get input
	protected virtual void GetInput() {
		Steer = CrossPlatformInputManager.GetAxis("Horizontal");
		Accell = CrossPlatformInputManager.GetAxis("Vertical");

        if (Flipped () || CrossPlatformInputManager.GetButton("Fire3")) {
			Unflip();
		}
		#if !MOBILE_INPUT
		Handbrake = CrossPlatformInputManager.GetAxis("Jump");
		#endif
	}

	// apply universal input to move the car
	protected virtual void ApplyInput() {
        car.Move (Steer, Accell, Accell, Handbrake);
	}

	private bool Flipped() {
		// Car is upside-down if transform.up vector is poiting downward (even slightly).
		// If standing, and upside-down, then it is flipped and should be unflipped
		if (car.physicsBody.GetComponent<Rigidbody>().velocity.sqrMagnitude < 0.01 && Vector3.Dot(transform.up, Vector3.down) > 0) {
			return true;
		}
		return false;
	}

	private void Unflip() {
		Vector3 angles = car.physicsBody.transform.eulerAngles;
		angles.z = 0;
        car.physicsBody.transform.eulerAngles = angles;
	}
	
}

