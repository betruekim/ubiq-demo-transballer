using UnityEngine;
using System.Collections.Generic;
using Ubik.Messaging;
using Ubik.Samples;

public class NetworkManager : MonoBehaviour, INetworkObject, INetworkComponent, ISpawnable
{
    NetworkId INetworkObject.Id => new NetworkId();

    public static bool roomOwner = false;

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
}