using System;
using System.Collections.Generic;
using Ubik.Networking;
using Ubik.Messaging;
using UnityEngine;
using UnityEngine.Events;
using Ubik.Dictionaries;
using UnityEngine.Profiling;

namespace Ubik.Rooms
{
    /// <summary>
    /// Maintains a representation of a shared room. This component exchanges messages with other room managers
    /// and the matchmaking service in order to keep all peers in sync.
    /// The room manager is responsible for forwarding messages to other peers, until p2p connections can be e
    /// </summary>
    [RequireComponent(typeof(NetworkScene))] //not strictly true, but anyone advanced enough to use multiple clients in one scene can remove this...
    [NetworkComponentId(typeof(RoomClient), 1)]
    public class RoomClient : MonoBehaviour, INetworkComponent
    {
        private NetworkContext context;
        private Dictionary<string, PeerArgs> peers;

        public string roomGuid;

        private const int RoomServerObjectId = 1;
        private const int RoomServerComponentId = 1;

        private const string roomClientVersion = "0.0.3";

        /// <summary>
        /// A list of default servers to connect to on start-up
        /// </summary>
        public ConnectionDefinition[] servers;
        public PeerArgs me;

        private Dictionary<string, Action<string>> blobCallbacks;

        public class Room
        {
            public string guid { get; private set; }
            public string name { get; private set; }

            public bool needsUpdate;

            private SerializableDictionary properties;

            public Room()
            {
                properties = new SerializableDictionary();
            }

            public void Merge(RoomArgs args)
            {
                this.guid = args.guid;
                this.name = args.name;
                this.properties = args.properties;
            }

            public void SetName(string name)
            {
                this.name = name;
                needsUpdate = true;
            }

            public RoomArgs GetArgs()
            {
                return new RoomArgs()
                {
                    guid = guid,
                    name = name,
                    properties = properties
                };
            }

            public string this[string key]
            {
                get => properties[key];
                set
                {
                    var existing = properties[key];
                    if(existing != value)
                    {
                        needsUpdate = true;
                    }
                    properties[key] = value;
                }
            }

            public IEnumerable<KeyValuePair<string,string>> Properties
            {
                get
                {
                    return properties.Enumerator;
                }
            }
        }

        public Room room;


        public class PeerEvent : UnityEvent<PeerArgs> { };
        public class RoomsAvailableEvent : UnityEvent<List<RoomArgs>> { };

        /// <summary>
        /// Emitted when a peer has joined or updated its properties
        /// </summary>
        public PeerEvent OnPeer;

        /// <summary>
        /// Emitted when a peer has left the room
        /// </summary>
        public PeerEvent OnPeerRemoved;

        /// <summary>
        /// Emitted when this peer has joined a room
        /// </summary>
        public UnityEvent OnJoinedRoom;

        /// <summary>
        /// Emitted when the room this peer is a member of has updated its properties
        /// </summary>
        public UnityEvent OnRoom;

        /// <summary>
        /// Contains the latest list of rooms currently available on the server. Usually emitted in response to a discovery request.
        /// </summary>
        public RoomsAvailableEvent OnRoomsAvailable;

        public IEnumerable<PeerArgs> Peers
        {
            get
            {
                return peers.Values;
            }
        }

        private void Reset()
        {
            servers = new ConnectionDefinition[]
                {
                    new ConnectionDefinition() // default is a local rendezvous server
                    {
                        send_to_ip = "nexus.cs.ucl.ac.uk",
                        send_to_port = "8001",
                        type = ConnectionType.tcp_client
                    }
                };
        }

        public bool joinedRoom
        {
            get
            {
                return room != null && room.guid != null && room.guid != "";
            }
        }

        private void Awake()
        {
            if (OnJoinedRoom == null)
            {
                OnJoinedRoom = new UnityEvent();
            }
            if(OnPeer == null)
            {
                OnPeer = new PeerEvent();
            }
            if(OnPeerRemoved == null)
            {
                OnPeerRemoved = new PeerEvent();
            }
            if(OnRoomsAvailable == null)
            {
                OnRoomsAvailable = new RoomsAvailableEvent();
            }

            blobCallbacks = new Dictionary<string, Action<string>>();

            room = new Room();
            peers = new Dictionary<string, PeerArgs>();

            OnJoinedRoom.AddListener(() => Debug.Log("Joined Room " + room.name));

            me = new PeerArgs();
            me.guid = Guid.NewGuid().ToString();
        }

        private void Start()
        {
            context = NetworkScene.Register(this);
            context.networkObject.Id.Set(NetworkScene.GenerateUniqueId());
            me.networkObject = context.networkObject.Id;
            me.component = context.componentId;

            foreach (var item in servers)
            {
                Connect(item);
            }
        }

        // The room joining process occurs in two steps: id aquisition and then room join. This is to avoid a race condition whereby
        // another peer may be informed of this peer before this peer has updated its id.

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var container = JsonUtility.FromJson<Message>(message.ToString());
            switch (container.type)
            {
                case "Accepted":
                    {
                        var args = JsonUtility.FromJson<AcceptedArgs>(container.args);
                        room.Merge(args.room);
                        roomGuid = room.guid;
                        peers.Clear();
                        foreach (var item in args.peers)
                        {
                            peers[item.guid] = item;
                        }
                        OnJoinedRoom.Invoke();
                        OnRoom.Invoke();
                    }
                    break;
                case "UpdateRoom":
                    {
                        var args = JsonUtility.FromJson<RoomArgs>(container.args);
                        room.Merge(args);
                        OnRoom.Invoke();
                    }
                    break;
                case "UpdatePeer":
                    {
                        var peer = JsonUtility.FromJson<PeerArgs>(container.args);
                        peers[peer.guid] = peer;
                        OnPeer.Invoke(peer);
                    }
                    break;
                case "RemovedPeer":
                    {
                        var peer = JsonUtility.FromJson<PeerArgs>(container.args);
                        peers.Remove(peer.guid);
                        OnPeerRemoved.Invoke(peer);
                    }
                    break;
                case "Rooms":
                    {
                        var available = JsonUtility.FromJson<RoomsResponseArgs>(container.args);
                        if (roomClientVersion != available.version)
                        {
                            Debug.LogError($"Your version {roomClientVersion} of Ubik doesn't match the server version {available.version}. Please update Ubik in your project.");
                        }
                        OnRoomsAvailable.Invoke(available.rooms);
                    }
                    break;
                case "Blob":
                    {
                        var blob = JsonUtility.FromJson<Blob>(container.args);
                        var key = blob.GetKey();
                        if(blobCallbacks.ContainsKey(key))
                        {
                            blobCallbacks[key](blob.blob);
                            blobCallbacks.Remove(key);
                        }
                    }
                    break;
            }
        }

        private void SendToServer(string type, object argument)
        {
            context.Send(RoomServerObjectId, RoomServerComponentId, JsonUtility.ToJson(new Message(type, argument)));
        }

        /// <summary>
        /// Joins the room with the GUID specified by the RoomClient::roomGuid member.
        /// </summary>
        /// <remarks>
        /// Intended for use by the Inspector only.
        /// </remarks>
        public void Join()
        {
            Join(roomGuid);
        }

        /// <summary>
        /// Creates a new room with the current settings on the server, and joins it.
        /// </summary>
        public void JoinNew()
        {
            Join(""); // an empty guid means request a new room
        }

        public void Join(string guid)
        {
            SendToServer("Join", new JoinArgs()
            {
                guid = guid,
                peer = me
            });
            me.needsUpdate = false;
        }

        public void Connect(ConnectionDefinition connection)
        {
            context.scene.AddConnection(Connections.Resolve(connection));
        }

        private void Update()
        {
            if(me.needsUpdate)
            {
                SendToServer("UpdatePeer", me);
                me.needsUpdate = false;
            }
            if(room.needsUpdate)
            {
                SendToServer("UpdateRoom", room.GetArgs());
                room.needsUpdate = false;
            }
        }

        public void DiscoverRooms()
        {
            SendToServer("RequestRooms", new RoomsRequestArgs());
        }

        /// <summary>
        /// Retrieves the value of a blob from a room. When the server responds a call will be made to callback.
        /// Only one callback is made per server response, regardless of how many times GetBlob was called between.
        /// If a blob does not exist, callback will be called with an empty string. (In this case, callback may issue
        /// another GetBlob call, to poll the server, if it is known that a value will eventually be set.)
        /// Blobs are by convention immutable, so it is safe to cache the result, once a valid result is returned.
        /// </summary>
        public void GetBlob(string room, string guid, Action<string> callback)
        {
            var blob = new Blob()
            {
                room = room,
                guid = guid
            };
            var key = blob.GetKey();
            if(!blobCallbacks.ContainsKey(key))
            {
                blobCallbacks.Add(key, callback);
            }
            SendToServer("GetBlob", blob);
        }

        private void SetBlob(string room, string guid, string blob) // private because this could encourage re-using guids, which is not allowed because blobs are meant to be immutable
        {
            if (blob.Length > 0)
            {
                SendToServer("SetBlob", new Blob()
                {
                    room = room,
                    guid = guid,
                    blob = blob
                });
            }
        }

        /// <summary>
        /// Sets a persistent variable that exists for as long as the room does. This variable is not sent with 
        /// Room Updates, but rather only when requested, making this method suitable for larger data.
        /// If the room does not exist, the data is discarded. 
        /// </summary>
        public string SetBlob(string room, string blob)
        {
            var guid = Guid.NewGuid().ToString();
            SetBlob(room, guid, blob);
            return guid;
        }
    }
}