﻿using UnityEngine;
using System.Collections;

public class ChainDamage : DamageBase
{
	public GameObject ChainObject;
	public int NumberChain = 5;
	public int Distance = 100;
	public float Direction = 0.5f;
	
	void Start ()
	{
		int count = 0;
		for (int t=0; t<TargetTag.Length; t++) {
			if (GameObject.FindGameObjectsWithTag (TargetTag [t]).Length > 0) {
				GameObject[] objs = GameObject.FindGameObjectsWithTag (TargetTag [t]);
				float distance = Distance;
				for (int i = 0; i < objs.Length; i++) {
					if (objs [i] != null) {
						Vector3 dir = (objs [i].transform.position - this.transform.position).normalized;
						float direction = Vector3.Dot (dir, this.transform.forward);
						float dis = Vector3.Distance (objs [i].transform.position, this.transform.position);
						if (dis < distance) {
							if (direction >= Direction) {
								if (ChainObject) {
									if (count <= NumberChain) {
										GameObject chain = (GameObject)GameObject.Instantiate (ChainObject, this.transform.position, this.transform.rotation);
										Quaternion targetlook = Quaternion.LookRotation (objs [i].transform.position - chain.transform.position);
										chain.transform.rotation = targetlook;
										count += 1;
									}
								}
							}
							distance = dis;
						}
					}			
				}
			}		
		}
	}

}
