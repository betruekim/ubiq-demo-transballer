using UnityEngine;
using Ubik.Samples;

public class TestSpawner : MonoBehaviour
{

    public NetworkSpawner networkSpawner;
    public GameObject spawnPrefab;
    public PlacableObjects.PlacementManager placementManager;

    private void Awake()
    {
        networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
        placementManager = GameObject.FindObjectOfType<PlacableObjects.PlacementManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject newBall = networkSpawner.SpawnPersistent(spawnPrefab);

            newBall.transform.position = placementManager.rightHand.transform.position + placementManager.rightHand.transform.forward;
            newBall.transform.rotation = Quaternion.Euler(0, placementManager.rightHand.transform.rotation.eulerAngles.y, 0);
        }
    }
}