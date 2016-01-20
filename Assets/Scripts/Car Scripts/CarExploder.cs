using UnityEngine;
using System.Collections;

public class CarExploder : MonoBehaviour {

    private GameObject physics;
    private CarExploderRigidbodyTrigger trigger;
    private GameObject wheels;
    public GameObject vehicleBody;
    //wheels to boom
    private GameObject wheel_mid_extra_leftback;
    private GameObject wheel_mid_extra_leftfront;
    private GameObject wheel_mid_extra_rightback;
    private GameObject wheel_mid_extra_rightfront;

    public PhysicMaterial boomPhysics;

	// Use this for initialization
	void Start ()
    {
        VehicleController _vehicleController = GetComponent<VehicleController>();
        wheel_mid_extra_leftback = _vehicleController.wheelLeftBack;
        wheel_mid_extra_leftfront = _vehicleController.wheelLeftFront;
        wheel_mid_extra_rightback = _vehicleController.wheelRightBack;
        wheel_mid_extra_rightfront = _vehicleController.wheelRightFront;

        //加一个脚本让车爆炸
        physics = GameObject.Find(gameObject.name+" physics");
        wheels = GameObject.Find(gameObject.name+" wheels");
        if(physics)
        {
            trigger = physics.AddComponent<CarExploderRigidbodyTrigger>();
        }
        //设置需要炸飞的部分
        if(trigger &&   wheels)
            trigger.Setup(
                    gameObject.name,
                    vehicleBody,
                    GetComponent<VehicleController>(),
                    wheel_mid_extra_leftback,
                    wheel_mid_extra_leftfront,
                    wheel_mid_extra_rightback,
                    wheel_mid_extra_rightfront,
                    boomPhysics
            );
	}
}
