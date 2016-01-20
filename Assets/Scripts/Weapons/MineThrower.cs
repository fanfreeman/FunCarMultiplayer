using UnityEngine;
using System.Collections;

public class MineThrower : CanBeShootOut {
    private Vector3 shootDirection;
    private Vector3 selfRotation;
    private Transform body;
    private bool fired = false;
    private float initSpeed = 50f;

    public GameObject rotator;
    public Transform rotatorMeshObj;

    // Use this for initialization
    void Awake()
    {
        //phybody
        body = transform.GetChild(0);
        rotatorMeshObj = rotator.transform.GetChild(0);
    }

	// Update is called once per frame
	public override void Fire (float speed)
    {
        fired = true;
        //发射方向
        shootDirection = body.transform.TransformDirection(Vector3.up);
        //发射后旋转模拟抛物线
        selfRotation = body.transform.TransformDirection(Vector3.back);
        body.GetComponent<Rigidbody>().velocity =
        shootDirection.normalized * (speed + initSpeed);

        body.GetComponent<Rigidbody>().angularVelocity =
        selfRotation * 1.8f;
    }


    private float rotatorMeshObjZ = 1.25f;
    private float rotatorMeshObjZSpeed = 24f;
    private int factor = -1;
    void Update()
    {
  //     MakeBombMoveCrazy();
    }

    private void MakeBombMoveCrazy()
    {
        rotator.transform.Rotate(Vector3.up *Time.deltaTime * 2000f);
        if (rotatorMeshObjZ > 1.25f)
        {
            factor = -1;
        }
        if (rotatorMeshObjZ < -1.25f)
        {
            factor = 1;
        }
        rotatorMeshObjZ += rotatorMeshObjZSpeed*Time.deltaTime * factor;
        rotatorMeshObj.transform.localPosition = new Vector3(0,0,rotatorMeshObjZ);
    }

    void FixedUpdate ()
    {
        if(!fired) return;
            body.GetComponent<Rigidbody>().AddForce(400f*500f*Vector3.forward*Time.deltaTime);
            body.GetComponent<Rigidbody>().AddForce(1200f*500f*Vector3.down*Time.deltaTime);
    }
}
