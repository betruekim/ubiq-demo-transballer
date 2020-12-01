using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Ubik.Rooms;
using Ubik.Messaging;

namespace Ubik.WebRtc
{
    /// <summary>
    /// Manages the lifetime of WebRtc Peer Connection objects with respect to changes in the room
    /// </summary>
    [NetworkComponentId(typeof(WebRtcPeerConnectionManager), 3)]
    public class WebRtcPeerConnectionManager : MonoBehaviour, INetworkComponent
    {
        private RoomClient client;
        private Dictionary<string, WebRtcPeerConnection> connections;
        private NetworkContext context;

        private void Awake()
        {
            client = GetComponentInParent<RoomClient>();
            connections = new Dictionary<string, WebRtcPeerConnection>();
        }

        private void Start()
        {
            context = NetworkScene.Register(this);
            client.OnJoinedRoom.AddListener(OnJoinedRoom);
            client.OnPeerRemoved.AddListener(OnPeerRemoved);
        }

        // It is the responsibility of the new peer (the one joining the room) to begin the process of creating a peer connection,
        // and existing peers to accept that connection.
        // This is because we need to know that the remote peer is established, before beginning the exchange of messages.

        public void OnJoinedRoom()
        {
            foreach (var peer in client.Peers)
            {
                if (peer.guid == client.me.guid)
                {
                    continue; // don't connect to ones self!
                }

                var pcid = NetworkScene.GenerateUniqueId();
                var pc = CreatePeerConnection(pcid, peer.guid);
                pc.MakePolite();
                pc.AddLocalAudioSource();
                pc.stats.peer = peer.guid;
                Message m;
                m.type = "RequestPeerConnection";
                m.objectid = pcid; // the shared Id is set by this peer, but it must be chosen so as not to conflict with any other shared id on the network
                m.guid = client.me.guid; // this is so the other end can identify us if we are removed from the room
                Send(peer.networkObject, m);
            }
        }

        public void OnPeerRemoved(PeerArgs peer)
        {
            try
            {
                Destroy(connections[peer.guid].gameObject);
                connections.Remove(peer.guid);
            }
            catch (KeyNotFoundException)
            {
                // never had this peer or already done
            }
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = JsonUtility.FromJson<Message>(message.ToString());
            switch (msg.type)
            {
                case "RequestPeerConnection":
                    var pc = CreatePeerConnection(msg.objectid, msg.guid);
                    pc.stats.peer = msg.guid;
                    pc.AddLocalAudioSource();
                    break;
            }
        }

        [Serializable]
        public struct Message
        {
            public string type;
            public int objectid;
            public string guid;
        }

        private WebRtcPeerConnection CreatePeerConnection(int objectid, string guid)
        {
            var go = new GameObject("Peer Connection");
            go.transform.SetParent(transform);

            var node = go.AddComponent<NetworkGameObject>();
            node.Id.Set(objectid);

            var pc = go.AddComponent<WebRtcPeerConnection>();

            connections.Add(guid, pc);

            return pc;
        }

        public void Send(int sharedId, Message m)
        {
            context.SendJson(sharedId, m);
        }
    }
}