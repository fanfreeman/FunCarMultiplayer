using UnityEngine;
using System.Collections;

public abstract class WeaponFireBase : MonoBehaviour, IWeaponBase
{

    private FireSeedFunc _getFireSeed = null;
    public FireSeedFunc GetFireSeed
    {
        get { return _getFireSeed; }
        set { _getFireSeed = value; }
    }

    public NetworkFunc m_SendFireEventToNetworkFunc { get; set; } // null in singleplayer & remote players, set if local & multiplayer
    public int m_CurrentFireSeed { get; set; }								// unique number used to generate a random spread for every projectile

    [HideInInspector]
    public GameObject Owner { get; set; }
    [HideInInspector]
    public GameObject Target { get; set; }

    public string[] targetTag = new string[1] { "Enemy" };
    public string[] TargetTag
    {
        get { return targetTag; }
        set { targetTag = value; }
    }

    public string[] ignoreTag;
    public string[] IgnoreTag
    {
        get { return ignoreTag; }
        set { ignoreTag = value; }
    }

    public bool rigidbodyProjectile;
    public bool RigidbodyProjectile
    {
        get { return rigidbodyProjectile; }
        set { rigidbodyProjectile = value; }
    }

    public Vector3 torqueSpeedAxis;
    public Vector3 TorqueSpeedAxis
    {
        get { return torqueSpeedAxis; }
        set { torqueSpeedAxis = value; }
    }

    public GameObject torqueObject;
    public GameObject TorqueObject
    {
        get { return torqueObject; }
        set { torqueObject = value; }
    }

    public bool DoDamageCheck(GameObject gob)
    {
        for (int i = 0; i < ignoreTag.Length; i++)
        {
            if (ignoreTag[i] == gob.tag)
                return false;
        }
        return true;
    }

    public virtual void TryNetworkFire()
    {
        if (m_SendFireEventToNetworkFunc != null)
            m_SendFireEventToNetworkFunc.Invoke();
    }
}
