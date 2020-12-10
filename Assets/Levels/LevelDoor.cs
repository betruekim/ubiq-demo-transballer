using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;


namespace Transballer.Levels
{
    public class LevelDoor : MonoBehaviour, INetworkObject, INetworkComponent
    {
        public NetworkId Id { get; } = new NetworkId(10);
        NetworkContext ctx;

        private LevelLoader loaderScript;
        public int levelIndex = 0;

        void Awake()
        {
            ctx = NetworkScene.Register(this);
            loaderScript = GameObject.FindObjectOfType<LevelLoader>();
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log("Door collided with " + other.gameObject.name);
            Debug.Log(NetworkManager.roomOwner);
            if (other.gameObject.name == "Right Hand" && NetworkManager.roomOwner)
            {
                // Load in the level
                loaderScript.loadLevelOwner(levelIndex);

                // Send out message to peers
                ctx.Send(new OnLoad().Serialize());
            }
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgString = message.ToString();
            string messageType = Transballer.Messages.GetType(msgString);

            if (messageType == "levelLoad")
            {
                // If your message had variables, you would deserialise it here
                // To access the message object
                loaderScript.loadLevel();
            }
        }
    }



    [System.Serializable]
    public class OnLoad : Transballer.Messages.Message
    {
        public override string messageType => "levelLoad";

        public override string Serialize()
        {
            return "levelLoad$";
        }


    }
}