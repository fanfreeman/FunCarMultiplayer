using UnityEngine;
using System.Collections;

public class DamageBase : MonoBehaviour
{

    public GameObject Effect;
    [HideInInspector]
    public GameObject Owner;
    public int Damage = 20;

    public string[] TargetTag = new string[1] { "Enemy" };
    public string[] IgnoreTag;

    public bool DoDamageCheck(GameObject gob)
    {
        for (int i = 0; i < IgnoreTag.Length; i++)
        {
            if (IgnoreTag[i] == gob.tag)
                return false;
        }
        return true;
    }
}

