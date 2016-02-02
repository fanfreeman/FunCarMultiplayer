using UnityEngine;
using System.Collections;


public delegate int FireSeedFunc();
public delegate void NetworkFunc();
public interface IWeaponBase
{
    FireSeedFunc GetFireSeed { get; set; }
    NetworkFunc m_SendFireEventToNetworkFunc { get; set; } // null in singleplayer & remote players, set if local & multiplayer
    int m_CurrentFireSeed { get; set; }                         // unique number used to generate a random spread for every projectile

    GameObject Owner { get; set; }
    GameObject Target { get; set; }

    string[] TargetTag { get; set; }
    string[] IgnoreTag { get; set; }
    bool RigidbodyProjectile { get; set; }
    Vector3 TorqueSpeedAxis { get; set; }
    GameObject TorqueObject { get; set; }

    bool DoDamageCheck (GameObject gob);
    void TryNetworkFire ();
}


// public abstract class WeaponBase : MonoBehaviour {
//     public delegate int FireSeedFunc();
//     public FireSeedFunc GetFireSeed = null;
// 
//     public delegate void NetworkFunc();
//     public NetworkFunc m_SendFireEventToNetworkFunc = null; // null in singleplayer & remote players, set if local & multiplayer
//     protected int m_CurrentFireSeed;									// unique number used to generate a random spread for every projectile
// 
//     [HideInInspector]
//     public GameObject Owner;
// 	[HideInInspector]
// 	public GameObject Target;
// 	
//     public string[] TargetTag = new string[1]{"Enemy"};
// 	public string[] IgnoreTag;
// 	public bool RigidbodyProjectile;
// 	public Vector3 TorqueSpeedAxis;
// 	public GameObject TorqueObject;
// 	
// 	public bool DoDamageCheck(GameObject gob){
// 		for(int i=0;i<IgnoreTag.Length;i++){
// 			if(IgnoreTag[i] == gob.tag)
// 				return false;
// 		}
// 		return true;
// 	}
// 
//     protected virtual void TryNetworkFire()
//     {
//         if (m_SendFireEventToNetworkFunc != null)
//             m_SendFireEventToNetworkFunc.Invoke();
//     }
// }

