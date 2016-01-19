using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class exploder : MonoBehaviour {

	[TextArea(3,10)]
	public string Note = "Put Script on GameObject, give GameObject a Trigger(Collider with 'is Trigger' set to true), put Meshes inside GameObject as Children, they will be the debris";

	[Header("Debris")]
	[Tooltip("Physics Material to place on the debris (children GameObjects with Meshes)")]
	public PhysicMaterial physicsMat;
	[Tooltip("strength of the force that is applied to the debris")]
	public float debrisForce = 1f;
	[Tooltip("radius of the force that is applied to the debris")]
	public float debrisForceRadius = 3f;


	[Header("Particles")]

	[Tooltip("attach your GameObject that has a Particle System, it will be placed for the explosion")]
	public GameObject particlesEmitter;
	[Tooltip("offset the position that the Particles Emitter is placed")]
	public Vector3 particleEmitterOffset = new Vector3(0f,1f,0f);

	[Header("Debris Removal")]

	[Tooltip("time to wait before starting to shrink/remove debris")]
	public float waitBeforeCleanup = 5f;
	[Tooltip("remove debris while not visible by any camera(including scene view)")]
	public bool deleteOutOfViewDebris = true;
	[Tooltip("shrink visible debris and then remove them")]
	public bool shrinkDebris = true;
	[Tooltip("shrink debris even if they are still moving")]
	public bool shrinkWhileMoving = false;


	[HideInInspector]
	public List<GameObject> debrisList;
	private bool hit = false;
	private bool cleanup = false;
	private GameObject particlesEmitterObj;
	private ParticleSystem particles;

	void Awake()
	{	
		//add a collider if none exists//
		if(!GetComponent<Collider>())
		{
			gameObject.AddComponent<SphereCollider>();
			gameObject.GetComponent<SphereCollider>().isTrigger = true;
		}

		foreach(Transform child in transform)
		{
			debrisList.Add(child.gameObject);
			
			child.gameObject.AddComponent<Rigidbody>();
			child.gameObject.GetComponent<Rigidbody>().mass = 0.00001f;
			child.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			
			if(!child.gameObject.GetComponent<Collider>())
			{
				child.gameObject.AddComponent<MeshCollider>();
				child.gameObject.GetComponent<MeshCollider>().convex = true;
				child.gameObject.GetComponent<MeshCollider>().material = physicsMat;
				child.gameObject.GetComponent<MeshCollider>().enabled = false;
			}
		}
	}

	//detect collision with collider//
	void OnCollisionEnter(Collision col)
	{
		Vector3 _colPos = (col.transform.position + transform.position)/2;
		explode(_colPos);
	}

	//detect collision with trigger//
	void OnTriggerEnter(Collider col)
	{
		Vector3 _colPos = (col.transform.position + transform.position)/2;
		explode(_colPos);
	}

	//explode//
	public void explode(Vector3 colPos)
	{


		if(!hit)
		{
			hit = true;
			for(int n = debrisList.Count-1; n >= 0; n--)
			{
				debrisList[n].gameObject.GetComponent<Rigidbody>().isKinematic = false;
				debrisList[n].gameObject.GetComponent<Rigidbody>().AddExplosionForce(debrisForce/50f,colPos,debrisForceRadius);
			
				if(debrisList[n].gameObject.GetComponent<MeshCollider>())
				{
					debrisList[n].gameObject.GetComponent<MeshCollider>().enabled = true;
				}
				debrisList[n].transform.SetParent(null);
			}
		
			if(particlesEmitter)
			{
				particlesEmitterObj = Instantiate(particlesEmitter,transform.position + particleEmitterOffset,transform.rotation) as GameObject;
				particles = particlesEmitterObj.GetComponent<ParticleSystem>();
			}
			Invoke("startCleanup",waitBeforeCleanup);
		}

	}

	void startCleanup()
	{
		cleanup = true;
	}

	void Update()
	{
		if(cleanup)
		{
			for(int n = debrisList.Count-1; n >= 0; n--)
			{
				//by default do not shrink or destroy//
				bool destroyNow = false;
				bool shrinkNow = false;

				//shrink even if still moving//
				if(shrinkWhileMoving)
				{
					shrinkNow = true;
				}

				//still moving//
				if(debrisList[n].GetComponent<Rigidbody>().isKinematic == false)
				{
					//hardly moving at all//
					if(debrisList[n].GetComponent<Rigidbody>().velocity.magnitude < 0.01f)
					{
						//stop moving//
						debrisList[n].GetComponent<Rigidbody>().isKinematic = true;
						if(debrisList[n].gameObject.GetComponent<MeshCollider>())
						{
							debrisList[n].gameObject.GetComponent<MeshCollider>().enabled = false;
						}
					}
				}
				//stopped moving//
				else
				{
					if(shrinkDebris)
					{
						shrinkNow = true;
					}
				}

				//out of view//
				if(!debrisList[n].GetComponent<Renderer>().isVisible)
				{
					if(deleteOutOfViewDebris)
					{
						destroyNow = true;
					}
				}

				//shrink//
				if(shrinkNow)
				{
					//still has room to shrink//
					if(debrisList[n].transform.localScale.x > 0.01f)
					{
						float s = Mathf.Lerp (debrisList[n].transform.localScale.x,0f,0.8f*Time.deltaTime);
						debrisList[n].transform.localScale = new Vector3(s,s,s);
					}
					//small enough to destroy//
					else
					{
						destroyNow = true;
					}
				}

				//destroy//
				if(destroyNow)
				{
					GameObject debrisObj = debrisList[n];
					debrisList.Remove(debrisList[n]);
					Destroy(debrisObj);
				}
			}

			//delete self once debris are gone//
			if(debrisList.Count <= 0)
			{
				if(particlesEmitter)
				{
					if(!particles.IsAlive())
					{
						Destroy(particlesEmitterObj);
					}
				}
				Destroy(gameObject);
			}
		}
	}
	//Exploder Script - Version 1.0 - Aaron Hibberd - 9.30.2015 - www.hibbygames.com//
}



