using UnityEngine;
using System.Collections.Generic;
using Ubik.Messaging;
using Ubik.Samples;
using Ubik.Rooms;

public class NetworkManager : MonoBehaviour, INetworkObject, INetworkComponent, ISpawnable
{
    // by setting a fixed networkId we can have objects synced in everyone's scenes
    NetworkId INetworkObject.Id { get; } = new NetworkId(4);
    NetworkContext ctx;
    public static bool roomOwner = true;
    public static bool connected = false;
    public static bool inLevel = false;
    RoomClient roomClient;

    public GameObject[] levels;

    private void Awake()
    {
        ctx = NetworkScene.Register(this);
        roomClient = GameObject.FindObjectOfType<RoomClient>();
        roomClient.OnRoom.AddListener(OnRoom);
    }

    private void Start()
    {
        roomClient.OnPeer?.AddListener(CheckRoomOwner);
        roomClient.OnPeerRemoved?.AddListener(CheckRoomOwner);
    }

    void OnRoom()
    {
        Debug.Log("OnRoom");
        connected = true;
        CheckRoomOwner(null);
    }

    void CheckRoomOwner(PeerArgs peerArgs)
    {
        int peerCount = 0;
        foreach (var peer in roomClient.Peers)
        {
            peerCount++;
        }
        roomOwner = peerCount == 1;
        Debug.Log($"roomOwner: {roomOwner}");
    }

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log("levelmanager message");
        Debug.Log(message);
    }

    void ISpawnable.OnSpawned(bool local)
    {
        DontDestroyOnLoad(this);
        Debug.Log("Levelmanager spawned");
        if (local)
        {
            roomOwner = true;
            Debug.Log("!!I am the room owner!!");
        }
    }

    public void LoadLevel(int index)
    {

    }
}