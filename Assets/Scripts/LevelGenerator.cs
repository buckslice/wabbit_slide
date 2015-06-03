using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

    public Material mat;
    public Material fade;
    public int width;
    public int length;
    public float magnitude;
    public float frequency;
    public float exp;
    public float displace;
    public float wallHeight;
    public float xDisplace;
    public float xFreq;
    public int sliceLength = 10;
    public int maxSlices = 15;

    private bool setSpawn = false;
    public static int sliceNumber;
    private int[] triArray;
    private float seed;
    private List<GameObject> slices;
    private MaterialPropertyBlock mpb;
    private Transform bunny;
    private MeshRenderer newest;
    private Color newestC;
    private float fadingIn;

    public GameObject barrel;
    public GameObject milk;

    private BoxCollider back;
    private BoxCollider roof;

    // Use this for initialization
    void Start() {
        sliceNumber = 0;
        slices = new List<GameObject>();
        mpb = new MaterialPropertyBlock();
        seed = Random.value;

        back = gameObject.AddComponent<BoxCollider>();
        back.size = new Vector3(width, wallHeight, 0);
        roof = gameObject.AddComponent<BoxCollider>();
        roof.size = new Vector3(xDisplace, 0, maxSlices * sliceLength);


        // set triangle list for slices (verts are shared for smooth lighting)
        List<int> tris = new List<int>();
        int w = width + 3; // 2 extra wall verts
        for (int z = 0; z < sliceLength; z++) {
            for (int x = 0; x < w - 1; x++) {
                tris.Add(z * w + x);
                tris.Add((z + 1) * w + x);
                tris.Add((z + 1) * w + x + 1);
                tris.Add((z + 1) * w + x + 1);
                tris.Add(z * w + x + 1);
                tris.Add(z * w + x);
            }
        }
        triArray = tris.ToArray();

    }

    private GameObject GenerateSlice() {
        GameObject slice = new GameObject("Slice " + sliceNumber);
        slice.transform.parent = transform;
        slice.transform.localRotation = Quaternion.identity;

        MeshFilter mf = slice.AddComponent<MeshFilter>();
        MeshRenderer mr = slice.AddComponent<MeshRenderer>();
        MeshCollider mc = slice.AddComponent<MeshCollider>();
        mr.material = fade;
        Color c = new Color(Random.value, Random.value, Random.value, 0f);
        mpb.SetColor("_Color", c);
        mr.SetPropertyBlock(mpb);

        if (newest != null) {
            newest.material = mat;
            newestC.a = 1f;
            mpb.SetColor("_Color", newestC);
            newest.SetPropertyBlock(mpb);
        }

        newest = mr;
        newestC = c;

        int w = width + 1;
        int lStart = sliceNumber * sliceLength;
        int l = (sliceNumber + 1) * sliceLength + 1;

        float tOff = Mathf.PerlinNoise(seed, (float)lStart / length * xFreq) * xDisplace + w / 2;
        slice.transform.Translate(new Vector3(tOff, 0, sliceNumber * sliceLength), Space.Self);

        Vector3 spawn = slice.transform.position;
        spawn.y += 30f;

        if (Random.value < .2f) {
            spawn.x += (Random.value * 2f - 1f) * ((width / 2f) - displace);
            Instantiate(milk, spawn, Quaternion.identity);
        } else {
            spawn.x += (Random.value * 2f - 1f) * ((width / 2f) - displace);
            Instantiate(barrel, spawn, Quaternion.Euler(0, 0, 90));
        }

        List<Vector3> verts = new List<Vector3>();
        for (int z = lStart; z < l; z++) {
            // offset whole z slice with a seperate noise function
            float xOff = Mathf.PerlinNoise(seed, (float)z / length * xFreq) * xDisplace;
            xOff -= tOff;
            verts.Add(new Vector3(0 + xOff, wallHeight, z - lStart));
            for (int x = 0; x < w; x++) {
                // make ground bumpy
                float xCoord = (float)x / width;
                float zCoord = (float)z / width;
                float y = Mathf.PerlinNoise(xCoord * frequency + seed, zCoord * frequency + seed);

                // add bend around z direction
                float halfW = width / 2f;
                float bend = Mathf.Abs(x - halfW);
                bend = Mathf.Pow(exp, bend - (halfW - displace));

                verts.Add(new Vector3(x + xOff, y * magnitude + bend, z - lStart));
            }
            verts.Add(new Vector3(w - 1 + xOff, wallHeight, z - lStart));
        }

        // build mesh and set collider
        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.triangles = triArray;
        m.Optimize();
        m.RecalculateNormals();
        m.RecalculateBounds();

        mf.mesh = m;
        mc.sharedMesh = m;

        sliceNumber++;

        // find starting point for bunny
        if (!setSpawn) {
            Vector3 v = verts[w / 2 - 1];
            bunny = GameObject.Find("Bunny").transform;
            Vector3 p = slice.transform.position;
            bunny.position = new Vector3(v.x + p.x, 15f + p.y, 15f + p.z);
            setSpawn = true;
        }

        return slice;
    }

    // Update is called once per frame
    void Update() {
        if (slices.Count < maxSlices) {
            slices.Add(GenerateSlice());
        }

        newestC.a += Time.deltaTime * 2f;
        mpb.SetColor("_Color", newestC);
        newest.SetPropertyBlock(mpb);

        GameObject slice = slices[0];
        if (slice.transform.position.z < bunny.position.z - sliceLength * 3) {
            slices.Remove(slice);
            Destroy(slice.GetComponent<MeshCollider>().sharedMesh);
            Destroy(slice.GetComponent<MeshFilter>().mesh);
            Destroy(slice);
        }

        // set back and roof collider positions
        Vector3 backPos = slices[0].transform.position;
        backPos.y = back.size.y / 2f;
        backPos.z = (sliceNumber - maxSlices) * sliceLength;
        back.center = backPos;
        roof.center = backPos + new Vector3(0, back.size.y / 2f, roof.size.z / 2f);
    }
}
