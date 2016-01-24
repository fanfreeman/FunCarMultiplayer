using UnityEngine;
using System.Collections;

public class CarWeaponSystem : MonoBehaviour {

    [HideInInspector]
    public float inputX;


    //射出去的炮弹
    public GameObject fireMe;
    //根据车的位置生成炮弹
    public GameObject carBody;

    private GameObject physics;
    //车的刚体
    private Rigidbody carRealRigidbody;
    private const float mineCoolDownCount = 1.5f;
    private float mineCoolDownCounter;

    private const float timeDeltaOfEveryBiuBiuBiu = 0.13f;
    // Use this for initialization
	void Start () {
        //获得刚体
        mineCoolDownCounter = mineCoolDownCount;
        physics = gameObject.GetComponent<VehicleController>().physicsBody;
        if(physics)
        {
            carRealRigidbody = physics.GetComponent<Rigidbody>();
        }
	}
	
	// Update is called once per frame
	void Update () {

        if (GetComponent<VehicleController>().isBoomedOrKilled)
            return;
        mineCoolDownCounter -= Time.deltaTime;
        //fire
        if (Input.GetKeyUp (KeyCode.Space))
        {
            if(mineCoolDownCounter <=0 )
            {
                mineCoolDownCounter = mineCoolDownCount;
                transform.parent.GetComponent<NetworkCar>().Fire();
            }
        }
	}

    //射击
    public void BiuBiuBiu()
    {
        StartCoroutine(FireMine());
    }

    public IEnumerator FireMine()
    {
        int time = firePosRotationOffset.Length;
        while (time > 0)
        {
            time--;
            ShootMineOut(time);
            yield return new WaitForSeconds(0.13f);
        }
    }

    /// <summary>
    /// 一次射击多发的位置偏移
    /// </summary>
    private float[] firePosRotationOffset = {-25f,0f,25f};
    private void ShootMineOut(int i)
    {
        GameObject biubiu = Instantiate(
                fireMe,
                carBody.transform.position + new Vector3(0,2f,0),
                carBody.transform.rotation
        ) as GameObject;

        //方向随机性
        biubiu.transform.Rotate(Vector3.up*firePosRotationOffset[i]);
        //calRotation = Quaternion.Euler(0f,randomY,0f);
        MineThrower canBeShootOut = biubiu.GetComponent<MineThrower>();
        canBeShootOut.Fire(carRealRigidbody.velocity.magnitude);
    }
}
