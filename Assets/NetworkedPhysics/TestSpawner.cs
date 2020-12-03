using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;
using Ubik.Physics;

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
            GameObject newCube = networkSpawner.SpawnPersistent(spawnPrefab);
            newCube.transform.position = Vector3.up + Random.insideUnitSphere;
            newCube.transform.rotation = Random.rotation;
        }

        // var testMessage = new Messages.GraspMessage();
        // Debug.Log(testMessage.Serialize());
        // Debug.Log(Messages.Deserialize(testMessage.Serialize()).messageType);


    }
}