using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;

public class TestSpawner : MonoBehaviour
{
    // TODO replace this with a findOfType call
    public NetworkSpawner networkSpawner;
    public GameObject spawnPrefab;

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