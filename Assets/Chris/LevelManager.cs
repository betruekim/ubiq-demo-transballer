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

        public GameObject levelSelect;
        public GameObject ui;

        void Awake()
        {
            ctx = NetworkScene.Register(this);
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