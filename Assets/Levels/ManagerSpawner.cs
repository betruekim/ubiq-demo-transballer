using UnityEngine;
using Ubik.Rooms;
using Ubik.Samples;

public class ManagerSpawner : MonoBehaviour
{
    RoomClient roomClient;

    private void Awake()
    {
        roomClient = GameObject.FindObjectOfType<RoomClient>();

        roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);

    }

    void OnJoinedRoom()
    {
        int peerCount = 0;
        foreach (var peer in roomClient.Peers)
        {
            peerCount++;
        }

        Debug.Log($"ManagerSpawner.cs onjoined {roomClient.roomGuid}");
        if (peerCount == 1)
        {
            NetworkSpawner ns = GameObject.FindObjectOfType<NetworkSpawner>();
            ns.SpawnPersistent(ns.catalogue.prefabs[0]);
        }
    }
}