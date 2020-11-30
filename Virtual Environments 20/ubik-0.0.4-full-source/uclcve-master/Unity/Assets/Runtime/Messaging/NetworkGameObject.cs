using UnityEngine;
using System;
using Ubik.Networking;
using System.Text;

namespace Ubik.Messaging
{
    public interface INetworkComponent
    {
        /// <summary>
        /// Process a message directed at this object. Use the data directly in the implementation. Once the call returns the data in message will be undefined.
        /// If necessary, Acquire can be called after which message may be stored, and after which Release must be called when it is done with.
        /// Release does not have to be called if the message is processed entirely within the implementation.
        /// </summary>
        void ProcessMessage(ReferenceCountedSceneGraphMessage message);
    }

    /// <summary>
    /// Wraps a reference counted message for when interacting with the scene graph bus
    /// </summary>
    // this is a wrapper rather than a subclass, because the networking code will allocated reference counted objects, with no knowledge of the sgb
    public struct ReferenceCountedSceneGraphMessage : IReferenceCounter
    {
        internal const int header = 8;
        internal ReferenceCountedMessage buffer;

        public ReferenceCountedSceneGraphMessage(ReferenceCountedMessage buffer)
        {
            this.buffer = buffer;
            start = buffer.start + header;
            length = buffer.length - header;
        }

        public int start
        {
            get;
            private set;
        }

        public int length
        {
            get;
            private set;
        }

        public byte[] bytes
        {
            get
            {
                return buffer.bytes;
            }
        }

        public int objectid
        {
            get
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(new Span<byte>(buffer.bytes, buffer.start, 4));
            }
            set
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(buffer.bytes, buffer.start, 4), value);
            }
        }

        public int componentid
        {
            get
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(new Span<byte>(buffer.bytes, buffer.start + 4, 4));
            }
            set
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(buffer.bytes, buffer.start + 4, 4), value);
            }
        }

        public void Acquire()
        {
            buffer.Acquire();
        }

        public void Release()
        {
            buffer.Release();
        }

        public static ReferenceCountedSceneGraphMessage Rent(int length)
        {
            // expected header length is 4 bytes for the node id and 4 bytes for the entity id
            return new ReferenceCountedSceneGraphMessage(MessagePool.Shared.Rent(length + ReferenceCountedSceneGraphMessage.header));
        }

        public static ReferenceCountedSceneGraphMessage Rent(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var msg = Rent(bytes.Length);
            Array.Copy(bytes, 0, msg.bytes, msg.start, bytes.Length);
            return msg;
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(bytes, start, length);
        }

        public T FromJson<T>()
        {
            return JsonUtility.FromJson<T>(this.ToString());
        }
    }

    /// <summary>
    /// This instance holds the identity of a Network Object - like a passport. The NetworkId may change at runtime.
    /// Ids can be unique on the network, or shared, depending on whether messages should be one-to-one or one-to-many
    /// between instances.
    ///
    /// </summary>
    [Serializable]
    public class NetworkId
    {
        [SerializeField]
        protected int id;

        public NetworkId(int id)
        {
            this.id = id;
        }

        public NetworkId()
        {
        }

        public void Set(int id)
        {
            this.id = id;
        }

        public static implicit operator int(NetworkId d) => d.id;

        public override string ToString()
        {
            return id.ToString();
        }
    }

    public interface INetworkObject
    {
        NetworkId Id { get; }
    }

    [Serializable]
    public class NetworkObjectId : NetworkId
    {
        public enum Type
        {
            Auto,
            Fixed
        }

        /// <summary>
        /// This is a hint as to the use of this object identity. If this is set to Auto, it must be set, e.g. with a Room Client, Manager object, or similar.
        /// </summary>
        public Type type;

        public NetworkObjectId()
        {
            type = Type.Auto;
        }
    }


    /// <summary>
    /// The SceneGraphBusNode acts as a SceneGraphBus client.
    /// </summary>
    [Serializable]
    public class NetworkGameObject : MonoBehaviour, INetworkObject
    {
        public NetworkId Id { get => id; }

        [SerializeField]
        private NetworkObjectId id = new NetworkObjectId();
    }
}
