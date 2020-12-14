using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Transballer.NetworkedPhysics;
using Ubik.Messaging;

namespace Transballer.Levels
{
    public class LevelManager : MonoBehaviour, INetworkObject, INetworkComponent
    {
        public NetworkId Id { get; } = new NetworkId(15);
        NetworkContext ctx;
        public List<Ball> ballList;
        BallSpawner spawner;
        List<Hoop> hoops;

        public GameObject levelSelect;
        public GameObject ui;

        public Level level;

        void Awake()
        {
            ctx = NetworkScene.Register(this);
            spawner = GetComponentInChildren<BallSpawner>();
            spawner.SetEmissions(level.emission);

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
            Debug.Log("levelStart");
            if (!started && NetworkManager.roomOwner)
            {
                started = true;
                spawner.SpawnBalls();
                startTime = Time.time;
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
                levelComplete();
            }
        }

        public void levelComplete()
        {
            if (NetworkManager.roomOwner)
            {
                levelCompleteOwner();
            }
        }

        public void levelCompleteOwner()
        {
            destroyBallsOwner();

            // send signal
            ctx.Send(new ManagerMessage().Serialize());

            // Reenable the level select and ui
            levelSelect.SetActive(true);
            ui.SetActive(true);

            movePlayer();

            GameObject.Destroy(transform.gameObject);
        }

        public void levelCompletePeer()
        {
            // destroyBallsPeer();

            // Reenable the level select and ui
            levelSelect.SetActive(true);
            ui.SetActive(true);

            movePlayer();

            GameObject.Destroy(transform.gameObject);
        }

        public void destroyBallsOwner()
        {
            foreach (var ball in ballList)
            {
                // remove will remove balls from other people's scenes as well, so we probably don't need destroyBallsPeer
                ball.Remove();
            }
        }

        public void destroyBallsPeer()
        {
            GameObject sceneManager = GameObject.Find("Scene Manager");
            int childs = sceneManager.transform.childCount;
            for (int i = childs - 1; i > 0; i--)
            {
                GameObject.Destroy(sceneManager.transform.GetChild(i).gameObject);
            }
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgString = message.ToString();
            string messageType = Transballer.Messages.GetType(msgString);

            if (messageType == "managerMessage")
            {
                Debug.Log("Message received from manager");
                levelCompletePeer();
            }
        }

        private void movePlayer()
        {
            GameObject playerPosition = GameObject.Find("PlayerPosition");
            GameObject player = GameObject.Find("Player");
            player.transform.position = playerPosition.transform.position;
            player.transform.rotation = playerPosition.transform.rotation;
        }
    }

    [System.Serializable]
    public class ManagerMessage : Transballer.Messages.Message
    {
        public override string messageType => "managerMessage";

        public override string Serialize()
        {
            return "managerMessage$";
        }

        public static ManagerMessage Deserialize(string message)
        {
            // string[] components = message.Split('$');

            return new ManagerMessage();
        }

    }
}