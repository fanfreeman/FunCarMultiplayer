using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class zombie : MonoBehaviour {
	
	public Transform targetBrains;
	public Vector3 spawnerPos;
	public float spawnAreaSize;
	public float smellDistance = 50f;
	public float exciteDistance = 10f;
	public GameObject exploderObj;
	public GameObject particlesEmitter;
	public GameObject puddleObj;

	private Rigidbody rbody;
	private Animator anim;
	private string behaviour = "hidden";
	private float n;
	private float animSpeedGoal = 1f;
	private float energy;
	private float waitF = 0f;
	private Quaternion startRot;
	private GameObject animObj;


	// Use this for initialization
	void Start () 
	{
		rbody = GetComponent<Rigidbody>();
		rbody.isKinematic = true;
		GetComponent<SphereCollider>().enabled = false;
		anim = GetComponentInChildren<Animator>();
		startRot = transform.rotation;
		animObj = anim.gameObject;
		animObj.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		//get distance from target//
		n = Vector3.Distance(transform.position,targetBrains.position);

		//hidden behaviour//
		if(behaviour == "hidden")
		{
			//rise from the ground//
			if(n <= smellDistance)
			{
				anim.gameObject.SetActive(true);
				anim.SetBool("alive",true);
				behaviour = "climbOut";
				waitF = 1.5f;
				rbody.isKinematic = false;
				GetComponent<SphereCollider>().enabled = true;
			}
		}
		//climbOut behaviour//
		else if(behaviour == "climbOut")
		{
			if(waitF > 0f)
			{
				waitF -= Time.deltaTime;
			}
			else
			{
				behaviour = "idle";
			}
		}
		//idle behaviour//
		else if(behaviour == "idle")
		{
			//smell brains//
			if(n<=smellDistance)
			{
				behaviour = "stalking";
				anim.SetBool("walk",true);
				animSpeedGoal = Random.Range(1f,1.5f);
			}
		}
		//stalking behaviour//
		else if(behaviour == "stalking")
		{
			//lose scent//
			if(n > smellDistance)
			{
				behaviour = "idle";
				anim.SetBool("walk",false);
				animSpeedGoal = Random.Range(0.1f,1f);
			}
			//become excited//
			else if(n <= exciteDistance)
			{
				//recover from previous charge//
				if(energy < 0f)
				{
					energy += Time.deltaTime;
				}
				//charge//
				else
				{
					behaviour = "charge";
					animSpeedGoal = Random.Range(2f,5f);
					energy = Random.Range(1f,5f);
				}
			}
			//walk towards target//
			else
			{
				walkTowardsTarget();
			}
		}
		//charge behaviour//
		else if(behaviour == "charge")
		{
			//lose excitement//
			if(n > exciteDistance)
			{
				behaviour = "stalking";
				animSpeedGoal = 1f;
			}
			//run out of energy//
			else if(energy <= 0f)
			{
				behaviour = "stalking";
				animSpeedGoal = 1f;
				energy = Random.Range(-5f,0f);
			}
			//walk towards target//
			else
			{
				walkTowardsTarget();
				energy -= Time.deltaTime;
			}
		}
	
		//adjust animation speed//
		anim.speed = Mathf.Lerp(anim.speed,animSpeedGoal,0.5f*Time.deltaTime);
	}

	void walkTowardsTarget()
	{
		//rotate towards brains//
		Quaternion initRot = transform.rotation;
		transform.LookAt(targetBrains.position);
		transform.rotation = Quaternion.Lerp(initRot,transform.rotation,5f*Time.deltaTime);
		transform.rotation = Quaternion.Euler(0f,transform.eulerAngles.y,0f);
		
		//move forwards//
		Vector3 moveVel = transform.forward*(60f*anim.speed)*Time.deltaTime;
		rbody.velocity = new Vector3(moveVel.x,rbody.velocity.y,moveVel.z);
	}


	void OnTriggerEnter(Collider col)
	{
		if(behaviour != "hidden")
		{
			//collide with car body//
			if(col.name == "colSuspension" || col.name == "colBody")
			{

				Vector3 _colPos = (col.transform.position + transform.position)/2;
				explode(_colPos);
			}
			//collide with car tire//
			if(col.name == "colLB" || col.name == "colLF" || col.name == "colRB" || col.name == "colRF")
			{
				Vector3 _colPos = (col.transform.position + transform.position)/2;
				explode(_colPos);
			}
		}
	}

	void explode(Vector3 colPos)
	{
		gameObject.SetActive(false);

		GameObject puddle = Instantiate(puddleObj,transform.position + new Vector3(0f,0.1f,0f),transform.rotation) as GameObject;
		//find a normal of ground for splat rotation//
		Vector3 rayPos = transform.position + new Vector3(0f,0.1f,0f);
		RaycastHit hit;
		if (Physics.Raycast(rayPos, Vector3.down, out hit))
		{
			puddle.transform.rotation = Quaternion.FromToRotation(Vector3.up,hit.normal);
		}

		GameObject exploder = Instantiate(exploderObj,transform.position,transform.rotation) as GameObject;
		exploder.GetComponent<exploder>().particlesEmitter = particlesEmitter;
		exploder.GetComponent<exploder>().explode(colPos);

		Invoke("reset",5f);

	}

	void reset()
	{
		//find a random position on the ground to spawn from//
		Vector3 rayPos = spawnerPos + new Vector3(Random.Range(spawnAreaSize*-0.5f,spawnAreaSize*0.5f),5f,Random.Range(spawnAreaSize*-0.5f,spawnAreaSize*0.5f));
		RaycastHit hit;
		if (Physics.Raycast(rayPos, Vector3.down, out hit))
		{
			rayPos = hit.point;

			gameObject.SetActive(true);
			animObj.SetActive(true);

			rbody.velocity = new Vector3(0f,0f,0f);
			rbody.angularVelocity = new Vector3(0f,0f,0f);
			transform.position = rayPos;
			transform.rotation = startRot;
			behaviour = "hidden";
			anim.Play("hidden");
			anim.SetBool("alive",false);
			anim.SetBool("walk",false);
			animSpeedGoal = 1f;
			rbody.isKinematic = true;
			GetComponent<SphereCollider>().enabled = false;
			animObj.SetActive(false);
		}
		else
		{
			Invoke("reset",5f);
		}
	}

}










