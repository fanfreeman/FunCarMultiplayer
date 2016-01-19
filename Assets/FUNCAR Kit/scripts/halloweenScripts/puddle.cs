using UnityEngine;
using System.Collections;

public class puddle : MonoBehaviour 
{
	public Material tireTrailMat;

	private bool cleanup = false;

	void Start()
	{
		Invoke("startCleanup",5f);
		transform.localScale = new Vector3(0.2f,0.2f,0.2f);
	}

	void Update()
	{
		if(cleanup)
		{
			//out of view//
			if(!GetComponent<Renderer>().isVisible)
			{
				Destroy(gameObject);
			}
		}
		else
		{
			transform.localScale = Vector3.Lerp(transform.localScale,new Vector3(1f,1f,1f),3f*Time.deltaTime);
		}
	}

	void OnTriggerEnter(Collider col)
	{
		//collide with car tire//
		if(col.name == "colLB" || col.name == "colLF" || col.name == "colRB" || col.name == "colRF")
		{
			//tell the wheel it hit a puddle//
			col.GetComponent<wheel>().hitPuddle(tireTrailMat);
		}
	}
	void startCleanup()
	{
		cleanup = true;
	}


}










