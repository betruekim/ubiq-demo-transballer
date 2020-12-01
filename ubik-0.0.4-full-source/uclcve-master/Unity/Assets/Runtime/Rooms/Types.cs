using System;
using System.Collections.Generic;
using Ubik.Dictionaries;
using UnityEngine;

namespace Ubik.Rooms
{
    [Serializable]
    public struct Message
    {
        public string type;
        public string args;

        public Message(string type, object args)
        {
            this.type = type;
            this.args = JsonUtility.ToJson(args);
        }
    }

    [Serializable]
    public class PeerArgs
    {
        /// <summary>
        /// The self generated GUID of this client
        /// </summary>
        public string guid;

        /// <summary>
        /// The shared id of the network object/scene that hosts the room client. This can be imagined as the instance of the scene root, or process id.
        /// </summary>
        public int networkObject;

        /// <summary>
        /// The component id of the room client
        /// </summary>
        public int component;

        /// <summary>
        /// A list of key-value pairs that can be set by other components in the system
        /// </summary>
        public SerializableDictionary properties;

        [NonSerialized]
        public bool needsUpdate;

        /// <summary>
        /// Flag this peer definition as needing to be sent to other peers with updated properties. This method is cheap to call. The actual update is sent once on the subsequent frame.
        /// </summary>
        public void MarkUpdated()
        {
            needsUpdate = true;
        }

        public PeerArgs()
        {
            properties = new SerializableDictionary();
            needsUpdate = true;
        }
    }

    [Serializable]
    public class RoomArgs
    {
        public string guid;
        public string name;
        public SerializableDictionary properties;
        
        public RoomArgs()
        {
            properties = new SerializableDictionary();
        }
    }

    [Serializable]
    public struct JoinArgs
    {
        public string guid;
        public string password;
        public PeerArgs peer;
    }

    /// <summary>
    /// Joined Args represents the state of the room at the moment this peer joined
    /// </summary>
    [Serializable]
    public class AcceptedArgs
    {
        public RoomArgs room;
        public List<PeerArgs> peers;

        public AcceptedArgs()
        {
            room = new RoomArgs();
            peers = new List<PeerArgs>();
        }
    }

    [Serializable]
    public class RoomsRequestArgs
    {
    }

    [Serializable]
    public class RoomsResponseArgs
    {
        public string version;
        public List<RoomArgs> rooms;
    }

    [Serializable]
    public class Blob
    {
        public string room;
        public string guid;
        public string blob;

        public string GetKey()
        {
            return $"{room}:{guid}";
        }
    }
}