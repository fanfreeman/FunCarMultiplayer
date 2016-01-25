using UnityEngine;
using System.Collections;

public class Damage : DamageBase
{
	public bool Explosive;
	public float ExplosionRadius = 20;
	public float ExplosionForce = 1000;
	public bool HitedActive = true;
	public float TimeActive = 0;
	private float timetemp = 0;

	private void Start ()
	{
		if (!Owner || !Owner.GetComponent<Collider>())
			return;
		
		
		Physics.IgnoreCollision (GetComponent<Collider>(), Owner.GetComponent<Collider>());
		if(Owner.transform.root){
			foreach(Collider col in Owner.transform.root.GetComponentsInChildren<Collider>()){
				Physics.IgnoreCollision (GetComponent<Collider>(), col);
			}
		}
		
		timetemp = Time.time;
	}

	private void Update ()
	{
		if (!HitedActive || TimeActive > 0) {
			if (Time.time >= (timetemp + TimeActive)) {
				Active ();
			}
		}
	}

	public void Active ()
	{
		if (Effect) {
			GameObject obj = (GameObject)Instantiate (Effect, transform.position, transform.rotation);
			Destroy (obj, 3);
		}

		if (Explosive)
			ExplosionDamage ();


		Destroy (gameObject);
	}

	private void ExplosionDamage ()
	{
		Collider[] hitColliders = Physics.OverlapSphere (transform.position, ExplosionRadius);
		for (int i = 0; i < hitColliders.Length; i++) {
			Collider hit = hitColliders [i];
			if (!hit)
				continue;
			if (DoDamageCheck (hit.gameObject)) {
				hit.gameObject.SendMessage ("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
				if (hit.GetComponent<Rigidbody>())
					hit.GetComponent<Rigidbody>().AddExplosionForce (ExplosionForce, transform.position, ExplosionRadius, 3.0f);
			}
		}

	}

	private void NormalDamage (Collision collision)
	{
		collision.gameObject.SendMessage ("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
	}

	private void OnCollisionEnter (Collision collision)
	{
		if (HitedActive) {
			if (DoDamageCheck (collision.gameObject) && collision.gameObject.tag != this.gameObject.tag) {
				if (!Explosive)
					NormalDamage (collision);
				Active ();
			}
		}
	}
}
