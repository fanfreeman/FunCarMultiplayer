using System.Collections;
using UnityEngine;

//挂在在刚体上 方便实现各种物理效果
public class CarExploderRigidbodyTrigger : MonoBehaviour {

    private CharacterJoint suspension;
    private GameObject carBody;
    private PhysicMaterial boomPhysics;
    private GameObject[] wheels;
    private VehicleController _vehicleScript;
    private GameObject suspensionBody;
    private CarExploder exploder;
    private Transform exploderTransform;
    //撞击同时带有trigger和collider的物体可能会多次触发,防止多次触发
    private bool isBoomed = false;
    // Use this for initialization
    public void Setup (
            Transform _exploderTransform,
            GameObject body,
            VehicleController vehicleScript,
            GameObject wheel1,
            GameObject wheel2,
            GameObject wheel3,
            GameObject wheel4,
            PhysicMaterial physicMaterial,
            GameObject _suspensionBody
    ) {
        exploderTransform = _exploderTransform;
        exploder = _exploderTransform.GetComponent<CarExploder>();
        carBody = body;

        wheels = new GameObject[4];
        wheels[0] = wheel1;
        wheels[1] = wheel2;
        wheels[2] = wheel3;
        wheels[3] = wheel4;
        _vehicleScript = vehicleScript;
        boomPhysics = physicMaterial;
        //为了让车轮胎也被炸飞 必须先取消CharacterController
        suspension = _suspensionBody.GetComponent<CharacterJoint>();

    }
//    void OnDrawGizmos() {
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawSphere(transform.position,3f);
//    }
    public void SetImBoomed (){
        //本地炸了 另一边必须也炸 不管另一端打没打到
        exploderTransform.parent.GetComponent<CarInput>().IsMyCarBoomed = true;
    }

    //引爆
    public void Exploder(float explosionForce = 123f)
    {
        if (isBoomed) return;
        isBoomed = true;
        _vehicleScript.SetToDestory();

        //爆炸前的speed
        Vector3 speed;
        Vector3 explosionPos = carBody.transform.position;
        const float explosionRadius = 6f;

        Destroy(suspension);
        //移除车先前的collider
        Rigidbody selfRig = gameObject.GetComponent<Rigidbody>();
        speed = selfRig.velocity;
        Destroy(selfRig);

        //给轮子添加刚体和collider
        foreach (GameObject wheel in wheels) {
            Collider collider =
            wheel.AddComponent<SphereCollider>();
            collider.material = boomPhysics;

            Rigidbody rigidbody = wheel.AddComponent<Rigidbody>();
            rigidbody.mass = 0.5f;
            rigidbody.velocity = speed;

            if (explosionForce > 0)
            {
                //炸飞一切的神秘力量
                rigidbody.AddExplosionForce(explosionForce, explosionPos, explosionRadius, 1.0F);
            }
        }
        //给车添加刚体模拟炸飞
        Rigidbody rigidbodyCar = carBody.transform.parent.gameObject.AddComponent<Rigidbody>();
        rigidbodyCar.mass = GetComponent<Rigidbody>().mass;
        rigidbodyCar.velocity = speed;
        //炸飞车身
        if(explosionForce > 0)
            rigidbodyCar.AddExplosionForce(explosionForce, explosionPos, explosionRadius, 3.0F);

        BoxCollider carBodyCollider = carBody.AddComponent<BoxCollider>();
        carBodyCollider.size = carBody.transform.GetChild(0).GetComponent<MeshRenderer>().bounds.size;

        exploder.DestoryCar(4f);
        Debug.Log("爆炸");
        Destroy(gameObject);
    }
}
