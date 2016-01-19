using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class baddieSpawner : MonoBehaviour {

	public Transform baddieTarget;
	public int goalPopulation = 5;
	public float spawnAreaSize = 20f;
	public List<GameObject> baddieList;
	public List<GameObject> baddieParticleList;
	public List<GameObject> baddiePuddleList;
	private int currentPopulation;
	
	// Update is called once per frame
	void Update () 
	{
		if(currentPopulation < goalPopulation)
		{
			placeBaddie();
		}
	}

	void placeBaddie()
	{
		if(baddieList.Count > 0)
		{
			int n = Random.Range(0,baddieList.Count);
			
			//find a random position on the ground for the baddie to spawn from//
			Vector3 rayPos = transform.position + new Vector3(Random.Range(spawnAreaSize*-0.5f,spawnAreaSize*0.5f),5f,Random.Range(spawnAreaSize*-0.5f,spawnAreaSize*0.5f));
			RaycastHit hit;
			if (Physics.Raycast(rayPos, Vector3.down, out hit))
			{
				rayPos = hit.point;

				GameObject baddie;
				baddie = Instantiate(baddieList[n],rayPos,transform.rotation) as GameObject;
				if(baddieList[n].name == "char_zombie" || baddieList[n].name == "char_skeleton")
				{
					baddie.GetComponent<zombie>().targetBrains = baddieTarget;
					baddie.GetComponent<zombie>().puddleObj = baddiePuddleList[n]; 
					baddie.GetComponent<zombie>().particlesEmitter = baddieParticleList[n];
					baddie.GetComponent<zombie>().spawnerPos = transform.position;
					baddie.GetComponent<zombie>().spawnAreaSize = spawnAreaSize;
				}
				
				currentPopulation++;

			}


		}
	}




}






