using UnityEngine;
using System.Collections;

public class CarExploder : MonoBehaviour {

    private GameObject physics;
    [HideInInspector]
    public CarExploderRigidbodyTrigger trigger;
    private GameObject wheels;
    public GameObject vehicleBody;
    public GameObject car;
    private GameObject suspensionBody;
    //wheels to boom
    private GameObject wheel_mid_extra_leftback;
    private GameObject wheel_mid_extra_leftfront;
    private GameObject wheel_mid_extra_rightback;
    private GameObject wheel_mid_extra_rightfront;
    private InGameManager inGameManager;

    public PhysicMaterial boomPhysics;

    void Awake()
    {
        inGameManager =
            GameObject.Find("PUNManager").GetComponent<InGameManager>();

    }
	// Use this for initialization
	void Start ()
    {
        PhotonView owner = transform.parent.GetComponent<NetworkCar>().photonView;
        VehicleController _vehicleController = GetComponent<VehicleController>();
        wheel_mid_extra_leftback = _vehicleController.wheelLeftBack;
        wheel_mid_extra_leftfront = _vehicleController.wheelLeftFront;
        wheel_mid_extra_rightback = _vehicleController.wheelRightBack;
        wheel_mid_extra_rightfront = _vehicleController.wheelRightFront;

        //加一个脚本让车爆炸
        physics = _vehicleController.physicsBody;
        wheels = _vehicleController.wheels;
        suspensionBody = _vehicleController.suspensionBody;
        if(physics)
        {
            trigger = physics.AddComponent<CarExploderRigidbodyTrigger>();
        }
        //设置需要炸飞的部分
        if(trigger &&   wheels)
            trigger.Setup(
                    transform,
                    vehicleBody,
                    GetComponent<VehicleController>(),
                    wheel_mid_extra_leftback,
                    wheel_mid_extra_leftfront,
                    wheel_mid_extra_rightback,
                    wheel_mid_extra_rightfront,
                    boomPhysics,
                    suspensionBody,
                    owner
            );
	}

    //毁掉的车销毁掉
    public void DestoryCar(float time)
    {
        inGameManager.IwasBoomedBySomeOne(car);
        StartCoroutine(WaitSomeSecondsAndDestoryCar(time));
    }

    IEnumerator WaitSomeSecondsAndDestoryCar(float time)
    {

        yield return new WaitForSeconds(time) ;
        inGameManager.ReBornPlayer(
                car
        );
    }
}
