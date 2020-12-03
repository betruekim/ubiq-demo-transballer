using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;

public class TestSpawner : MonoBehaviour
{

    public NetworkSpawner networkSpawner;
    public GameObject spawnPrefab;

    private void Awake()
    {
        networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject newCube = networkSpawner.Spawn(spawnPrefab);
            newCube.transform.position = Vector3.up + Random.insideUnitSphere;
            newCube.transform.rotation = Random.rotation;
        }
    }
}