using UnityEngine;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Samples;
using Ubiq.Rooms;

namespace Transballer
{
    public class NetworkManager : MonoBehaviour
    {
        // by setting a fixed networkId we can have objects synced in everyone's scenes
        public NetworkId NetworkId { get; } = new NetworkId(4);
        NetworkContext ctx;
        public static bool roomOwner = true;
        public static bool connected = false;
        public static bool inLevel = false;
        RoomClient roomClient;

        // keep track of when we join a room, if everyone does this we should be able to figure out a room owner
        // it doesn't actually matter who is room owner, so long as all clients agree
        // doesn't keep track of my guid!!
        public static Dictionary<string, long> peers = new Dictionary<string, long>();
        public static long utcTime;

        private void Start()
        {
            ctx = NetworkScene.Register(this);
            roomClient = ctx.Scene.GetComponentInChildren<RoomClient>();
            roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
            roomClient.OnPeerAdded.AddListener(RequestAllTimes);
            roomClient.OnPeerRemoved.AddListener(OnPeerLeft);
        }

        void OnJoinedRoom(IRoom room)
        {
            utcTime = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds();
            connected = true;
            ctx.Send(new JoinOrder(utcTime, roomClient.Me.uuid).Serialize());
            CheckRoomOwner();
        }

        void RequestAllTimes(IPeer args)
        {
            ctx.Send("requestAllTimes");
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string messageType = Messages.GetType(message.ToString());
            switch (messageType)
            {
                case "requestAllTimes":
                    ctx.Send(new JoinOrder(utcTime, roomClient.Me.uuid).Serialize());
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
                if (kvp.Key == roomClient.Me.uuid)
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

        void OnPeerLeft(IPeer args)
        {
            peers.Remove(args.uuid);
            CheckRoomOwner();
        }

        public int GetMyPlayerIndex()
        {
            List<KeyValuePair<string, long>> sortedGuids = new List<KeyValuePair<string, long>>(peers.Count);
            sortedGuids.Add(new KeyValuePair<string, long>(roomClient.Me.uuid, utcTime));
            sortedGuids.Sort((KeyValuePair<string, long> a, KeyValuePair<string, long> b) => { return (int)(a.Value - b.Value); });
            for (int i = 0; i < sortedGuids.Count; i++)
            {
                if (sortedGuids[i].Key == roomClient.Me.uuid)
                {
                    return i;
                }
            }
            throw new System.Exception("GetMyPlayerIndex, couldn't find my guid inside sortedGuids?");
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