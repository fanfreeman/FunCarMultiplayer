using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class houseLights : MonoBehaviour {

	public Transform targetObj;
	public List<Renderer> lights;
	public float onDistLessThan = 150f;
	public float offDistGreaterThan = 200f;
	public Material houseLightON;
	public Material houseLightOFF;
	public bool randomLeftOn = true;

	private bool lightsON = true;


	void Start()
	{
		for(int n = lights.Count-1; n >= 0; n--)
		{
			lights[n].sharedMaterial = houseLightON;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if(lights.Count > 0)
		{
			if(targetObj)
			{

				float dist = Vector3.Distance(transform.position,targetObj.position);
			
				if(dist < onDistLessThan)
				{
					if(!lightsON)
					{
						for(int n = lights.Count-1; n >= 0; n--)
						{
							lights[n].sharedMaterial = houseLightON;
						}
						lightsON = true;
					}
				}
				else if(dist > offDistGreaterThan)
				{
					if(lightsON)
					{

						for(int n = lights.Count-1; n >= 0; n--)
						{
							lights[n].sharedMaterial = houseLightOFF;
						}

						if(randomLeftOn)
						{
							if(lights.Count > 1)
							{
								int r = Random.Range(0,lights.Count);
								lights[r].sharedMaterial = houseLightON;
							}
						}

						lightsON = false;

					}
				}
			}
		}
	}
}



