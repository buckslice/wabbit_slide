using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Transform leader;

    // Use this for initialization
    void Start() {
        transform.Rotate(new Vector3(35, 0, 0));
    }

    // Update is called once per frame
    void LateUpdate() {
        transform.position = leader.transform.position;
        transform.Translate(new Vector3(0, 2f, -10f));

    }
}
