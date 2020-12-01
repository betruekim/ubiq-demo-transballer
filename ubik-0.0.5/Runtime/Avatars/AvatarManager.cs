using System.Collections.Generic;
using System;
using Ubik.Rooms;
using Ubik.Messaging;
using Ubik.Dictionaries;
using UnityEngine;
using UnityEngine.Events;

namespace Ubik.Avatars
{
    /// <summary>
    /// Manages the avatars for a client
    /// </summary>
    [NetworkComponentId(typeof(AvatarManager), 2)]
    public class AvatarManager : MonoBehaviour
    {
        public AvatarCatalogue Avatars;
        public string localPrefabGuid;

        private RoomClient client;
        private Dictionary<int, Avatar> avatars;
        private Dictionary<int, PeerArgs> peers;

        [SerializeField, HideInInspector]
        public Avatar LocalAvatar;

        private class AvatarArgs
        {
            public int objectId;
            public string prefabGuid;
            public SerializableDictionary properties;

            public AvatarArgs()
            {
                properties = new SerializableDictionary();
            }
        }

        private AvatarArgs localAvatarArgs;


        private void Awake()
        {
            client = GetComponentInParent<RoomClient>();
            avatars = new Dictionary<int, Avatar>();
            peers = new Dictionary<int, PeerArgs>();
            localAvatarArgs = new AvatarArgs();
        }

        private void Start()
        {
            client.OnPeer.AddListener(OnPeer);
            client.OnPeerRemoved.AddListener(OnPeerRemoved);
            client.OnJoinedRoom.AddListener(OnJoinedRoom);

            localAvatarArgs.objectId = NetworkScene.GenerateUniqueId();
            localAvatarArgs.prefabGuid = localPrefabGuid;

            if (localAvatarArgs.prefabGuid.Length > 0)
            {
                UpdateAvatar(localAvatarArgs, true);
                UpdatePeer(LocalAvatar);
            }
        }

        /// <summary>
        /// Creates a local Avatar for this peer based on the supplied prefab.
        /// </summary>
        public void CreateLocalAvatar(GameObject prefab)
        {
            localAvatarArgs.prefabGuid = prefab.GetComponent<Avatar>().guid;
            localPrefabGuid = localAvatarArgs.prefabGuid;
            UpdateAvatar(localAvatarArgs, true);
            UpdatePeer(avatars[localAvatarArgs.objectId]);
        }

        private void UpdateAvatar(AvatarArgs args, bool local)
        {
            // if we have an existing instance, but it is the wrong model, destory it so we can start again

            if (avatars.ContainsKey(args.objectId))
            {
                var existing = avatars[args.objectId];
                if (existing.guid != args.prefabGuid)
                {
                    Destroy(existing.gameObject);
                    avatars.Remove(args.objectId);
                }
            }

            // create an instance of the correct prefab for this avatar

            if (!avatars.ContainsKey(args.objectId))
            {
                var prefab = Avatars.GetPrefab(args.prefabGuid);
                var created = Instantiate(prefab, transform).GetComponentInChildren<Avatar>();
                created.Id.Set(args.objectId);
                avatars.Add(created.Id, created);
                created.OnUpdated.AddListener(UpdatePeer);

                if (local)
                {
                    if (LocalAvatar != null)
                    {
                        created.transform.localPosition = LocalAvatar.transform.localPosition;
                        created.transform.localRotation = LocalAvatar.transform.localRotation;
                    }
                    LocalAvatar = created;
                }
            }

            // update the avatar instance

            var avatar = avatars[args.objectId];

            avatar.local = local;
            if (local)
            {
                avatar.gameObject.name = "My Avatar #" + avatar.Id.ToString();
            }
            else
            {
                avatar.gameObject.name = "Remote Avatar #" + avatar.Id.ToString();
            }

            avatar.Merge(args.properties);
        }

        private AvatarArgs GetAvatarArgs(Avatar avatar)
        {
            AvatarArgs args;
            if(avatar == LocalAvatar)
            {
                args = localAvatarArgs;
            }
            else
            {
                args = new AvatarArgs();
            }
            args.properties = avatar.Properties;
            args.objectId = avatar.Id;
            args.prefabGuid = avatar.guid;
            localPrefabGuid = args.prefabGuid;
            return args;
        }

        private void Update()
        {
            foreach (var item in avatars)
            {
                if(item.Value.Properties.IsUpdated())
                {
                    if (item.Value.local)
                    {
                        UpdatePeer(item.Value);
                    }
                    else if (peers.ContainsKey(item.Key))
                    {
                        UpdatePeer(peers[item.Key], avatars[item.Key]);
                    }
                }
            }
        }

        private void UpdatePeer(Avatar avatar)
        {
            if (avatar.local)
            {
                UpdatePeer(client.me, avatar);
            }
        }

        private void UpdatePeer(PeerArgs peer, Avatar avatar)
        {
            if(peer.properties.Update("avatar-params", JsonUtility.ToJson(GetAvatarArgs(avatar))))
            {
                peer.MarkUpdated();
            }
        }

        private void OnJoinedRoom()
        {
            foreach (var item in client.Peers)
            {
                OnPeer(item);
            }
        }

        private void OnPeer(PeerArgs peer)
        {
            var parms = peer.properties["avatar-params"];
            if (parms != null)
            {
                var args = JsonUtility.FromJson<AvatarArgs>(parms);
                if (peer.guid == client.me.guid)
                {
                    UpdateAvatar(args, true);
                }
                else
                {
                    UpdateAvatar(args, false);
                }
                peers[args.objectId] = peer;
            }
        }

        private void OnPeerRemoved(PeerArgs peer)
        {
            var parms = peer.properties["avatar-params"];
            if (parms != null)
            {
                var args = JsonUtility.FromJson<AvatarArgs>(parms);
                if(avatars.ContainsKey(args.objectId))
                {
                    Destroy(avatars[args.objectId].gameObject);
                    avatars.Remove(args.objectId);
                }
                if (peers.ContainsKey(args.objectId))
                {
                    peers.Remove(args.objectId);
                }
            }
        }
    }

}