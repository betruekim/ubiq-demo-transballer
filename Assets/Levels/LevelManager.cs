using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Transballer.NetworkedPhysics;
using Ubiq.Spawning;
using Ubiq.Messaging;

namespace Transballer.Levels
{
    public class LevelManager : MonoBehaviour, INetworkSpawnable
    {
        public NetworkId NetworkId { get; set; }

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

        private int activeHoops;

        void Awake()
        {
            spawner = GetComponentInChildren<BallSpawner>();
            if (spawner)
            {
                spawner.SetEmissions(emission);
            }

            hoops = new List<Hoop>();
            hoops.AddRange(GetComponentsInChildren<Hoop>());
            activeHoops = 0;
            for (int i = 0; i < hoops.Count; i++)
            {
                hoops[i].SetIndex(i);
                activeHoops++;
            }
        }

        bool started = false;

        public void StartLevel()
        {
            if (NetworkManager.roomOwner)
            {
                if (!started)
                {
                    Debug.Log("levelStart");
                    started = true;
                    spawner.SpawnBalls();
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

                    // Reset the hoops
                    activeHoops = 0;
                    for (int i = 0; i < hoops.Count; i++)
                    {
                        hoops[i].ballsToGo = hoops[i].initialBalls;
                        hoops[i].gameObject.transform.Find("BallDisplay").GetComponent<TextMesh>().text = string.Format("{0}", hoops[i].ballsToGo);
                        activeHoops++;
                    }

                }
            }
        }

        public void HoopComplete(int index)
        {
            for (int i = 0; i < hoops.Count; i++)
            {
                if (hoops[i].index == index)
                {
                    // hoops.RemoveAt(i);
                    activeHoops--;
                    break;
                }
            }
            if (activeHoops == 0)
            {
                // level complete
                Debug.Log("LEVEL COMLETE!");
                GameObject.FindObjectOfType<LevelLoader>().SpawnNextLevelDoor(nextLevelIndex, Vector3.zero, Quaternion.identity);
            }
        }
    }

}