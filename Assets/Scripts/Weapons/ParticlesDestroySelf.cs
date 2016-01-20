using UnityEngine;
using System.Collections;

public class ParticlesDestroySelf : MonoBehaviour {
    private ParticleSystem particles;

    void Start()
    {
        particles = GetComponent<ParticleSystem>();
    }
	// Update is called once per frame
	void Update () {
        if(!particles.IsAlive())
        {
            Destroy(gameObject);
        }
	}
}
