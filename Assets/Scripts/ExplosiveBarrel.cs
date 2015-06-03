using UnityEngine;
using System.Collections;

public class ExplosiveBarrel : MonoBehaviour {

    private float timeSinceCollisionEvent = 0f;

    // Update is called once per frame
    void Update() {
        timeSinceCollisionEvent += Time.deltaTime;
        if (timeSinceCollisionEvent > 3f) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision col) {
        timeSinceCollisionEvent = 0f;
        if (col.gameObject.tag == "Bunny") {
            Rigidbody mybody = GetComponent<Rigidbody>();
            mybody.freezeRotation = true;
            Collider[] explodees = Physics.OverlapSphere(transform.position, 20f);
            foreach (Collider c in explodees) {
                Rigidbody rb = c.GetComponent<Rigidbody>();
                if (rb && mybody != rb) {
                    rb.AddExplosionForce(30f, transform.position, 40f, 3f, ForceMode.Impulse);
                }
            }

            GetComponent<ParticleSystem>().Play();
            Destroy(gameObject, 1.5f);
        }
    }

    void OnCollisionStay() {
        timeSinceCollisionEvent = 0f;
    }
}
