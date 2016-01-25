using UnityEngine;
using System.Collections;

public class RayShoot : DamageBase
{

	public int Range = 10000;
	public Vector3 AimPoint;
	public GameObject Explosion;
	public float LifeTime = 1;
	public LineRenderer Trail;
	
	void Start ()
	{
		RaycastHit hit;
		GameObject explosion = null;
		if (Physics.Raycast (this.transform.position, this.transform.forward, out hit, Range)) {
			AimPoint = hit.point;
			if (Explosion != null) {
				explosion = (GameObject)GameObject.Instantiate (Explosion, AimPoint, this.transform.rotation);
			}
			hit.collider.gameObject.SendMessage ("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
		} else {
			AimPoint = this.transform.forward * Range;
			explosion = (GameObject)GameObject.Instantiate (Explosion, AimPoint, this.transform.rotation);
			
		}
		if (explosion) {
			DamageBase dmg = explosion.GetComponent<DamageBase> ();
			if (dmg) {
				dmg.TargetTag = TargetTag;	
			}
		}
		if (Trail) {
			Trail.SetPosition (0, this.transform.position);
			Trail.SetPosition (1, AimPoint);
		}
		Destroy (this.gameObject, LifeTime);
	}

	void Update ()
	{
		
	}
}
