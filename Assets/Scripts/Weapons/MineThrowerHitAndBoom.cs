using UnityEngine;
using System.Collections;

public class MineThrowerHitAndBoom : MonoBehaviour {
    [Tooltip("attach your GameObject that has a Particle System, it will be placed for the explosion")]
    public GameObject particlesEmitter;
    private GameObject particlesEmitterObj;
    private float explodRadius = 3f;

    void OnCollisionEnter(Collision collision) {
        Vector3 contactPoint = collision.contacts[0].point;

        Collider[] colliders = Physics.OverlapSphere(contactPoint, explodRadius);
//        Debug.Log("colliders:"+colliders.Length);
        foreach (Collider hit in colliders) {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
//            Debug.Log("AddExplosionForce:"+hit.name);

            if(hit.transform.parent)
            {
                CarExploderRigidbodyTrigger cert;
                if(cert = hit.transform.parent.GetComponent<CarExploderRigidbodyTrigger>())
                    cert.SetImBoomed();
            }
            else
            if (rb != null)
            {
                rb.AddExplosionForce(100f, contactPoint, explodRadius, 3.0F);
            }
        }
        Boom();
    }

    public void Boom()
    {
        if(particlesEmitter)
        {
            particlesEmitterObj = Instantiate(particlesEmitter,transform.position,transform.rotation) as GameObject;
            particlesEmitterObj.AddComponent<ParticlesDestroySelf>();
        }
        Destroy(transform.parent.gameObject);
    }
}
