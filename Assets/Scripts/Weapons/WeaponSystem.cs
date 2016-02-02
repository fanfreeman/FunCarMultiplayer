using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSystem : WeaponBehaviour {

    [HideInInspector]
    public float inputX;


    //射出去的炮弹
    public GameObject fireMe;
    //根据车的位置生成炮弹
    public GameObject carBody;

    private GameObject physics;
    //车的刚体
    VehicleController vehicleController;
    private Rigidbody carRealRigidbody;
    private const float mineCoolDownCount = 1.5f;
    private float mineCoolDownCounter;
    private PhotonView photonView;
    //
    protected Dictionary<int, WeaponBehaviour> m_Weapons = new Dictionary<int, WeaponBehaviour>();

    private const float timeDeltaOfEveryBiuBiuBiu = 0.13f;
    // Use this for initialization
    void Awake()
    { 
        vehicleController = GetComponentInChildren<VehicleController>();
    }

	public override void Start () {
        photonView = PhotonView.Get(this);
        //获得CD
        mineCoolDownCounter = mineCoolDownCount;
        physics = vehicleController.physicsBody;
        if(physics)
        {
            carRealRigidbody = physics.GetComponent<Rigidbody>();
        }

        base.Start();
    }

    // Update is called once per frame
    public override void Update () {
        if (vehicleController.isBoomedOrKilled)
            return;

        base.Update();

        mineCoolDownCounter -= Time.deltaTime;
        //         //fire
        //         if (Input.GetKeyUp (KeyCode.Space))
        //         {
        //             if(mineCoolDownCounter <=0 )
        //             {
        //                 mineCoolDownCounter = mineCoolDownCount;
        //                 transform.parent.GetComponent<NetworkCar>().Fire();
        //             }
        //         }

        //TODO: lightweight the update
        for (int i = 0; i < weaponLists.Length; i++)
        {
            if (weaponLists[i] != null)
            {
                weaponLists[i].OnActive = false;
            }
        }
        if (currentWeapon < weaponLists.Length && weaponLists[currentWeapon] != null)
        {
            weaponLists[currentWeapon].OnActive = true;
        }
    }


    protected override void SwitchWeapon(int i)
    {
        //do sth u love
    }

    //from origin TankHUD
    void OnGUI()
    {
        if (currentWeapon > WeaponLists.Length)
            return;

        GUI.skin.label.fontSize = 15;
        GUI.Label(new Rect(20, 20, 300, 30), "Weapon Index " + currentWeapon);
        GUI.Label(new Rect(20, Screen.height - 95, 300, 30), "Esc back to mainmenu");
        GUI.Label(new Rect(20, Screen.height - 50, 300, 30), "Scroll Mouse to Change weapons");
        GUI.Label(new Rect(20, Screen.height - 70, 300, 30), "W A S D to Move");
        if (!WeaponLists[currentWeapon].InfinityAmmo)
        {
            GUI.Label(new Rect(20, 70, 300, 50), "Ammo " + WeaponLists[currentWeapon].Ammo + " / " + WeaponLists[currentWeapon].AmmoMax);
        }
        else {
            GUI.Label(new Rect(20, 70, 300, 50), "Infinity ammo");
        }
        GUI.skin.label.fontSize = 25;
        GUI.Label(new Rect(20, 40, 300, 50), "" + WeaponLists[currentWeapon].name);
    }

    public override void InitWeapons()
    {
        weaponLists = gameObject.GetComponentsInChildren<WeaponLauncher>(true);
        foreach (WeaponLauncher w in weaponLists)
        {
            WeaponLauncher weapon = w;  // need to cache shooter or delegate will only be set with values of last weapon
            weapon.isMine = photonView.isMine;
            weapon.GetFireSeed = delegate
            {
                return Shots;
            };
            weapon.m_SendFireEventToNetworkFunc = delegate ()
            {
                    if (!photonView) Debug.LogError("photonView is null");
                    if (photonView.isMine)
                    photonView.RPC("Fire", PhotonTargets.All, CurrentWeapon);
                //vp_MPDebug.Log("sending RPC: " + sho.gameObject.name + ", " + sho.GetFirePosition() + ", " + sho.GetFireRotation());
            };

        }




        //         vp_WeaponShooter[] shooters = gameObject.GetComponentsInChildren<vp_WeaponShooter>(true) as vp_WeaponShooter[];
        //         m_Shooters.Clear();
        //         foreach (vp_WeaponShoot  er f in shooters)
        //         {
        // 
        //             vp_WeaponShooter shooter = f;   // need to cache shooter or delegate will only be set with values of last weapon
        // 
        //             if (m_Shooters.ContainsKey(WeaponHandler.GetWeaponIndex(shooter.Weapon)))
        //                 continue;
        // 
        //             shooter.GetFireSeed = delegate
        //             {
        //                 return Shots;
        //             };
        // 
        //             shooter.m_SendFireEventToNetworkFunc = delegate ()
        //             {
        //                 if (!PhotonNetwork.offlineMode)
        //                     photonView.RPC("FireWeapon", PhotonTargets.All, WeaponHandler.GetWeaponIndex(shooter.Weapon), shooter.GetFirePosition(), shooter.GetFireRotation());
        //                 //vp_MPDebug.Log("sending RPC: " + sho.gameObject.name + ", " + sho.GetFirePosition() + ", " + sho.GetFireRotation());
        //             };
        // 
        //             m_Shooters.Add(WeaponHandler.GetWeaponIndex(shooter.Weapon), shooter);

        //       }

        //Debug.Log("Stored " + m_Shooters.Count + " local weapons.");

    }



    public override WeaponLauncher GetCurrentWeapon()
    {
        if (currentWeapon < weaponLists.Length && weaponLists[currentWeapon] != null)
        {
            return weaponLists[currentWeapon];
        }
        return null;
    }

    public override void LaunchWeapon()
    {
        base.LaunchWeapon();
    }

    [PunRPC]
    public override void Fire(int weaponIndex)
    {
        base.Fire(weaponIndex);
        if (weaponIndex < weaponLists.Length && weaponLists[weaponIndex] != null)
        {
            weaponLists[weaponIndex].Fire();
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
