using UnityEngine;
using System.Collections.Generic;
using Ubik.Messaging;
using Ubik.Samples;
using Ubik.Rooms;

namespace Transballer
{
    public class NetworkManager : MonoBehaviour, INetworkObject, INetworkComponent
    {
        // by setting a fixed networkId we can have objects synced in everyone's scenes
        NetworkId INetworkObject.Id { get; } = new NetworkId(4);
        NetworkContext ctx;
        public static bool roomOwner = true;
        public static bool connected = false;
        public static bool inLevel = false;
        RoomClient roomClient;

        public GameObject[] levels;

        // keep track of when we join a room, if everyone does this we should be able to figure out a room owner
        // it doesn't actually matter who is room owner, so long as all clients agree
        // doesn't keep track of my guid!!
        public static Dictionary<string, long> peers = new Dictionary<string, long>();
        public static long utcTime;

        private void Awake()
        {
            ctx = NetworkScene.Register(this);
            roomClient = GameObject.FindObjectOfType<RoomClient>();
            roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
        }

        private void Start()
        {
            roomClient.OnPeer.AddListener(RequestAllTimes);
            roomClient.OnPeerRemoved.AddListener(OnPeerLeft);
        }

        void OnJoinedRoom()
        {
            utcTime = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds();
            connected = true;
            ctx.Send(new JoinOrder(utcTime, roomClient.me.guid).Serialize());
            CheckRoomOwner();
        }

        void RequestAllTimes(PeerArgs args)
        {
            ctx.Send("requestAllTimes");
        }

        void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string messageType = Messages.GetType(message.ToString());
            switch (messageType)
            {
                case "requestAllTimes":
                    ctx.Send(new JoinOrder(utcTime, roomClient.me.guid).Serialize());
                    break;
                case "joinOrder":
                    JoinOrder info = JoinOrder.Deserialize(message.ToString());
                    peers[info.guid] = info.joinTime;
                    CheckRoomOwner();
                    Debug.Log($"roomOwner {roomOwner}");
                    break;
                default:
                    throw new System.Exception($"unknown message type {messageType}");
            }
        }

        void CheckRoomOwner()
        {
            Debug.Log($"checking room owner with {peers.Count} peers");
            foreach (var kvp in peers)
            {
                if (kvp.Key == roomClient.me.guid)
                {
                    continue;
                }
                if (kvp.Value < utcTime)
                {
                    // this peer joined before us
                    roomOwner = false;
                    return;
                }
            }
            roomOwner = true;
        }

        void OnPeerLeft(PeerArgs args)
        {
            peers.Remove(args.guid);
            CheckRoomOwner();
        }

        [System.Serializable]
        public class JoinOrder : Messages.Message
        {
            public override string messageType => "joinOrder";
            public long joinTime;
            public string guid;

            public override string Serialize()
            {
                return $"joinOrder${joinTime}${guid}";
            }

            public JoinOrder(long joinTime, string guid)
            {
                this.joinTime = joinTime;
                this.guid = guid;
            }

            public static JoinOrder Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new JoinOrder(long.Parse(components[1]), components[2]);
            }
        }
    }
}