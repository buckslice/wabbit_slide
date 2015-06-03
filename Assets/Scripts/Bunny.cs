using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Bunny : MonoBehaviour {

    private float speedLimit = 20f;
    private float maxSpeed = 0f;
    private Rigidbody rb;
    private Transform model;
    private int milkCount;
    public Text topLeft;
    public Text speedometer;
    private bool raceMode = false;
    private bool gameOver = false;
    private int sliceBegin = 0;
    private float timeLeft = 60f;
    public Gradient speedGrad;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        model = transform.Find("Model");
    }

    // Update is called once per frame
    void Update() {
        if (!gameOver) {
            model.forward = Vector3.Slerp(model.forward, rb.velocity, 4f * Time.deltaTime);
            if (rb.velocity.sqrMagnitude > speedLimit * speedLimit) {
                rb.velocity = rb.velocity.normalized * speedLimit;
            }

            float currentSpeed = rb.velocity.magnitude;
            maxSpeed = Mathf.Max(maxSpeed, currentSpeed);
            speedometer.text = currentSpeed.ToString("F2");
            speedometer.color = speedGrad.Evaluate(currentSpeed / 70f);
            if (currentSpeed > 20f) {
                Vector3 offsetMin = speedometer.rectTransform.offsetMin;
                Vector3 offsetMax = speedometer.rectTransform.offsetMax;
                float growth = Mathf.Pow(1.1f, (currentSpeed - 20f));
                offsetMin.x = (Mathf.PerlinNoise(Time.time * 20f, 0f) - .5f) * growth;
                float ynoise = (Mathf.PerlinNoise(Time.time * 13f, 0f) - .5f) * growth;
                offsetMin.y = ynoise;
                offsetMax.y = ynoise;
                speedometer.rectTransform.offsetMin = offsetMin;
                speedometer.rectTransform.offsetMax = offsetMax;
            } else {
                speedometer.rectTransform.offsetMin = Vector2.zero;
                speedometer.rectTransform.offsetMax = Vector2.zero;
            }

            if (raceMode) {
                topLeft.color = Color.red;
                timeLeft -= Time.deltaTime;
                int slicesGenerated = LevelGenerator.sliceNumber - sliceBegin;
                if (slicesGenerated >= 100 || timeLeft < 0) {
                    gameOver = true;
                    speedometer.rectTransform.offsetMin = Vector2.zero;
                    speedometer.rectTransform.offsetMax = Vector2.zero;
                    topLeft.alignment = TextAnchor.MiddleCenter;
                    topLeft.rectTransform.anchorMin = new Vector3(0, .75f);
                    speedometer.alignment = TextAnchor.MiddleCenter;
                    speedometer.rectTransform.anchorMin = new Vector3(0, .07f);
                    if (timeLeft > 0) {
                        GetComponent<AudioSource>().Play();
                        topLeft.text = "YOU WIN!\nPress 'R' to play again.";
                        speedometer.text = "time:" + timeLeft.ToString("F2") + "     " + speedometer.text + "     max:" + maxSpeed.ToString("F2");
                    } else {
                        topLeft.text = "TOO SLOW!\nPress 'R' to try again.";
                    }
                    Time.timeScale = 0f;
                    model.forward = Vector3.forward;
                    transform.Rotate(-25f, 0, 0, Space.Self);
                } else {
                    topLeft.text = slicesGenerated + "/100  " + timeLeft.ToString("F2");
                }

            } else {
                topLeft.text = "Milks: " + milkCount;
            }
        } else {
            float r = Mathf.Sin(Time.realtimeSinceStartup * 4f * Mathf.PI / 3.031f) - .5f;
            transform.Rotate(0, r * 2f, 0, Space.World);

            if (Input.GetKeyDown(KeyCode.R)) {
                Time.timeScale = 1f;
                Application.LoadLevel(0);
            }
        }
    }

    void FixedUpdate() {
        float x = Input.GetAxis("Horizontal") * 15f;
        rb.AddForce(transform.right * x);
    }

    void OnTriggerEnter(Collider col) {
        if (col.tag == "Milk") {
            milkCount++;
            if (milkCount >= 10 && !raceMode) {
                raceMode = true;
                sliceBegin = LevelGenerator.sliceNumber;
            }
            speedLimit += 5f;
            Destroy(col.gameObject);
            rb.AddForce(transform.forward * 5f, ForceMode.Impulse);
        }
    }
}
