using UnityEngine;
using System.Collections;

public abstract class WeaponBehaviour : MonoBehaviour
{
    //input strings
    public string fire = "Fire1";
    public string changeWeapon = "Mouse ScrollWheel";

    public int Shots = 0;                                   // amount of times the player has spawned projectiles. used for establishing deterministic
                                                            // random seeds that will be the same on all machines _without_ sending data over the network

    protected int currentWeapon = 0;
//     protected WeaponSystem weapon;
    public WeaponLauncher[] weaponLists;
    public virtual WeaponLauncher[] WeaponLists {
        get
        {
            return weaponLists;
        }
    }

    public virtual int CurrentWeapon
    {
        get
        {
            return currentWeapon;
        }
        set
        {
            if (value >= WeaponLists.Length)
            {
                currentWeapon = WeaponLists.Length - 1;
//                 Debug.Log("WeaponLists ptr out of bound:" + WeaponLists.Length + " but " + value);
            }
            else if (value < 0)
            {
                currentWeapon = 0;
//                 Debug.Log("WeaponLists ptr small than 0 fu*k!!");
            }
            else
            {
                currentWeapon = value;
            }
            SwitchWeapon(currentWeapon);
        }
    }
    public virtual void Start()
    {
        InitWeapons();
    }



    //keyboard to weapons
    public virtual void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            //Application.LoadLevel("Menu");
        }
        if (Input.GetAxis(changeWeapon) < 0) // back
        {
            CurrentWeapon -= 1;
        }
        if (Input.GetAxis(changeWeapon) > 0) // forward
        {
            CurrentWeapon += 1;
        }
        if (Input.GetButton("Fire1"))
        {
            if (CurrentWeapon < WeaponLists.Length && WeaponLists[CurrentWeapon] != null)
            {
                WeaponLists[CurrentWeapon].TryFire();
            }
        }
    }

    public abstract void InitWeapons();
    public abstract WeaponLauncher GetCurrentWeapon();

    public virtual void LaunchWeapon()
    {
        if (CurrentWeapon < WeaponLists.Length && WeaponLists[CurrentWeapon] != null)
        {
            WeaponLists[CurrentWeapon].TryFire();
        }
    }

    [PunRPC]
    public virtual void Fire(int weaponIndex)
    {
        Shots++;
    }
    //check bound then do sth
    protected abstract void SwitchWeapon(int i);
}
