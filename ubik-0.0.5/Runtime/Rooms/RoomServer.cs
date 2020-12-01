using System;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Networking;
using Ubik.Messaging;
using Ubik.Dictionaries;

namespace Ubik.Rooms
{
    /// <summary>
    /// Implements a Room Server. Intended to be used for debugging and development.
    /// The server only supports one room which all clients connect to regardless of their join arguments.
    /// </summary>
    public class RoomServer : MonoBehaviour
    {
        private int objectId = 1;

        private Room room;
        public ConnectionDefinition connection;

        private INetworkConnectionServer server;
        private List<Client> clients;

        private List<Action> actions;

        public RoomsResponseArgs AvailableRooms
        {
            get
            {
                RoomsResponseArgs args = new RoomsResponseArgs();
                args.rooms = new List<RoomArgs>();
                args.rooms.Add(room.GetRoomArgs());
                args.version = "0.0.3";
                return args;
            }
        }

        private class Room
        {
            public string guid;
            public string name;
            public List<PeerArgs> peers = new List<PeerArgs>();
            public SerializableDictionary properties = new SerializableDictionary();
            public List<Client> clients = new List<Client>();

            public RoomArgs GetRoomArgs()
            {
                return new RoomArgs()
                {
                    guid = guid,
                    name = name,
                    properties = properties
                };
            }

            public void Join(Client client)
            {
                // join the room - for now this just means adding to the locked list
                peers.Add(client.peer);

                clients.Add(client);
                client.room = this;

                // send confirmation to the client
                client.SendAccepted(new AcceptedArgs()
                {
                    room = GetRoomArgs(),
                    peers = peers,
                });

                SendPeerUpdate(client.peer);
            }

            public void SendPeerUpdate(PeerArgs args)
            {
                foreach (var item in clients)
                {
                    item.SendPeer(args);
                }
            }

            public void SetRoomArgs(RoomArgs args)
            {
                this.guid = args.guid;
                this.name = args.name;
                this.properties = args.properties;
                SendRoomUpdate();
            }

            public void SendRoomUpdate()
            {
                foreach (var item in clients)
                {
                    item.SendRoom(GetRoomArgs());
                }
            }
        }

        private class Client
        {
            public INetworkConnection connection;
            public PeerArgs peer;
            public Room room;

            public Client(INetworkConnection connection)
            {
                this.connection = connection;
            }

            public void Send(string type, object argument)
            {
                var msg = ReferenceCountedSceneGraphMessage.Rent(JsonUtility.ToJson(new Message(type, argument)));
                msg.objectid = peer.networkObject;
                msg.componentid = peer.component;
                Send(msg);
            }

            public void Send(ReferenceCountedSceneGraphMessage m)
            {
                connection.Send(m.buffer);
            }

            public void SendAccepted(AcceptedArgs args)
            {
                Send("Accepted", args);
            }

            public void SendRoom(RoomArgs args)
            {
                Send("UpdateRoom", args);
            }

            public void SendPeer(PeerArgs args)
            {
                Send("UpdatePeer", args);
            }

            public void SendRooms(RoomsResponseArgs args)
            {
                Send("Rooms", args);
            }
        }

        private void Awake()
        {
            room = new Room();
            room.guid = Guid.NewGuid().ToString();
            room.name = "Sample Room";

            clients = new List<Client>();
            actions = new List<Action>();

            server = new TCPServer(connection.listen_on_ip, connection.listen_on_port);
            server.OnConnection = OnConnection;
        }

        private void OnDestroy()
        {
            server.Dispose();
        }

        private void OnConnection(INetworkConnection connection)
        {
            clients.Add(new Client(connection)); // add new clients to the outstanding clients list. once negotiated, they will be moved to a room.
        }

        private void Update()
        {
            foreach (var client in clients)
            {
                while (true)
                {
                    var buffer = client.connection.Receive();

                    if(buffer == null)
                    {
                        break;
                    }

                    try
                    {
                        var message = new ReferenceCountedSceneGraphMessage(buffer);

                        // check if this message is meant for us, or if we are to forward it

                        if (message.objectid == this.objectId)
                        {
                            var container = JsonUtility.FromJson<Message>(message.ToString());
                            switch (container.type)
                            {
                                case "Join":
                                    var joinArgs = JsonUtility.FromJson<JoinArgs>(container.args);
                                    client.peer = joinArgs.peer;
                                    room.Join(client);
                                    break;
                                case "UpdatePeer":
                                    client.peer = JsonUtility.FromJson<PeerArgs>(container.args);
                                    if (client.room != null)
                                    {
                                        client.room.SendPeerUpdate(client.peer);
                                    }
                                    break;
                                case "UpdateRoom":
                                    room.SetRoomArgs(JsonUtility.FromJson<RoomArgs>(container.args));
                                    break;
                                case "RequestRooms":
                                    client.SendRooms(AvailableRooms);
                                    break;
                                case "":

                                    break;
                            }
                        }
                        else
                        {
                            if (client.room != null) // if it is a member of a room...
                            {
                                foreach (var item in client.room.clients)
                                {
                                    if (item != client)
                                    {
                                        message.Acquire();
                                        item.Send(message);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        buffer.Release();
                    }
                }
            }

            // actions to be taken once outside the enumators (e.g. removing items from the lists...)

            foreach (var item in actions)
            {
                item();
            }
            actions.Clear();
        }


    }
}