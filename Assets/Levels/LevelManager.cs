using UnityEngine;
using System.Collections.Generic;
using Ubik.Messaging;
using Ubik.Samples;

public class LevelManager : MonoBehaviour, INetworkObject, INetworkComponent, ISpawnable
{
    NetworkId INetworkObject.Id => new NetworkId();

    public static bool roomOwner = false;
    public Dictionary<string, Level> levels;

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
        levels = new Dictionary<string, Level>();
        foreach (var level in Resources.LoadAll<Level>("Levels"))
        {
            Debug.Log($"Registering level {level.name}");
            levels[level.name] = level;
        }
    }

    public void SelectLevel()
    {
        // should send a message detailing which level was selected
    }
}