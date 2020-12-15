using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Transballer.NetworkedPhysics;
using Ubik.Samples;
using Ubik.Messaging;

namespace Transballer.Levels
{
    public class LevelManager : MonoBehaviour, INetworkObject, ISpawnable
    {
        NetworkId INetworkObject.Id { get; } = new NetworkId();

        public List<Ball> ballList;
        BallSpawner spawner;
        List<Hoop> hoops;

        public int allowedMaterial = 0;
        [System.Serializable]
        public struct EmissionBurst
        {
            public int count;
            public float duration;
        }
        [SerializeField]
        public List<EmissionBurst> emission = new List<EmissionBurst>();

        // you can set either of these, leave both empty to allow all objects
        public List<string> allowedPlaceables;
        public List<string> disallowedPlaceables;
        public int nextLevelIndex = -1;

        void Awake()
        {
            spawner = GetComponentInChildren<BallSpawner>();
            spawner.SetEmissions(emission);

            hoops = new List<Hoop>();
            hoops.AddRange(GetComponentsInChildren<Hoop>());
            for (int i = 0; i < hoops.Count; i++)
            {
                hoops[i].SetIndex(i);
            }
        }

        bool started = false;
        float startTime = 0;

        public void StartLevel()
        {
            if (NetworkManager.roomOwner)
            {
                if (!started)
                {
                    Debug.Log("levelStart");
                    started = true;
                    spawner.SpawnBalls();
                    startTime = Time.time;
                    HintManager.SetComplete(HintManager.spawners, true);
                    HintManager.SetComplete(HintManager.levelButton, true);
                }
                else
                {
                    spawner.StopSpawning();
                    started = false;
                    // reset level, clear up all balls
                    foreach (var ball in ballList)
                    {
                        ball.Remove();
                    }
                    ballList.Clear();
                }
            }
        }

        public void HoopComplete(int index)
        {
            for (int i = 0; i < hoops.Count; i++)
            {
                if (hoops[i].index == index)
                {
                    hoops.RemoveAt(i);
                    break;
                }
            }
            if (hoops.Count == 0)
            {
                // level complete
                Debug.Log("LEVEL COMLETE!");
                throw new System.Exception("SPAWN NEXT LEVEL DOOR HERE! USE LEVELLOADER");
            }
        }

        void ISpawnable.OnSpawned(bool local)
        {
            GameObject.FindObjectOfType<LevelLoader>().LevelSpawned(this);
        }
    }

}