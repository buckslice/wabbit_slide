using UnityEngine;
using System.Collections;

public class DeliciousMilk : MonoBehaviour {

    private float life = 30f;

    // Use this for initialization
    void Start() {
        RaycastHit hit;
        Physics.Raycast(new Ray(transform.position, Vector3.down), out hit);
        transform.position = hit.point;
        transform.up = transform.TransformDirection(hit.normal);
        transform.position = transform.position + transform.up;
    }

    void Update() {
        life -= Time.deltaTime;
        if (life < 0f) {
            Destroy(gameObject);
        }

        transform.Rotate(0, 90 * Time.deltaTime, 0);
        float s = Mathf.Sin(Time.time * 8f) * Time.deltaTime * 5f;
        transform.position = transform.position + transform.up * s;
    }

}
