using UnityEngine;
using System.Collections;

public class swapToExploder : MonoBehaviour {

	public GameObject exploderObj;

	//detect collision with trigger//
	void OnTriggerEnter(Collider col)
	{
		Vector3 _colPos = (col.transform.position + transform.position)/2;
		explode(_colPos);
	}

	void explode(Vector3 colPos)
	{
		
		GameObject exploder = Instantiate(exploderObj,transform.position,transform.rotation) as GameObject;
		exploder.GetComponent<exploder>().explode(colPos);
		Destroy(gameObject);
	}
}
