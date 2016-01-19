using UnityEngine;
using System.Collections;




public class mergeMeshes : MonoBehaviour 
{
	[TextArea(3,10)]
	public string Note = "Read/Write Enabled must be set to true on the import settings of each mesh";



	void Start() {
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		int i = 0;
		while (i < meshFilters.Length) {
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			i++;
		}
		MeshFilter myMeshFilter = gameObject.AddComponent<MeshFilter>();
		myMeshFilter.mesh = new Mesh();
		myMeshFilter.mesh.CombineMeshes(combine);

		//round vertices to snap together seams//
		Mesh mesh = myMeshFilter.mesh;
		Vector3[] vertices = mesh.vertices;
		int k = 0;
		while (k < vertices.Length) 
		{
			float x = vertices[k].x;
			x*=10f;
			x = Mathf.Round(x);
			x*=0.1f;

			float y = vertices[k].y;
			y*=10f;
			y = Mathf.Round(y);
			y*=0.1f;

			float z = vertices[k].z;
			z*=10f;
			z = Mathf.Round(z);
			z*=0.1f;


			vertices[k] = new Vector3(x,y,z);
			k++;
		}
		mesh.vertices = vertices;

		myMeshFilter.mesh.RecalculateNormals();

		MeshCollider myCollider = gameObject.AddComponent<MeshCollider>();
		myCollider.sharedMesh = null;
		myCollider.sharedMesh = transform.GetComponent<MeshFilter>().mesh; 



	}

}
