using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using UnityEngine.Events;
using Ubik.Rooms;
using Ubik.Dictionaries;

namespace Ubik.Avatars
{
    public class Avatar : MonoBehaviour, INetworkObject
    {
        public string guid;

        public NetworkId Id { get; } = new NetworkId();

        [Serializable]
        public class AvatarEvent : UnityEvent<Avatar> { }

        public AvatarEvent OnUpdated;
        public SerializableDictionary Properties;

        private void Awake()
        {
            if (OnUpdated == null)
            {
                OnUpdated = new AvatarEvent();
            }
        }

        /// <summary>
        /// Indicates the avatar was instantiated to represent a player on this computer. This flag is informational only. Child components do not have to use it.
        /// </summary>
        public bool local;

        private void Reset()
        {
            local = false; // better not to transmit by accident than to transmit by accident!
            guid = Guid.NewGuid().ToString();
        }

        public void Merge(SerializableDictionary properties)
        {
            if(Properties.Update(properties))
            {
                OnUpdated.Invoke(this);
            }
        }
    }
}