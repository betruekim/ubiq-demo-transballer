using UnityEngine;
using OdinSerializer;

namespace Ubik.Physics
{
    public class Messages
    {

        [System.Serializable]
        public abstract class Message
        {
            public abstract string messageType { get; }

            public string Serialize()
            {
                return System.Text.Encoding.ASCII.GetString(SerializationUtility.SerializeValue(this, DataFormat.JSON));
            }
        }

        public static Message Deserialize(string message)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);
            string messageType = SerializationUtility.DeserializeValue<Ubik.Physics.Messages.Message>(bytes, DataFormat.JSON).messageType;
            switch (messageType)
            {
                case "rigidbodyUpdate":
                    return SerializationUtility.DeserializeValue<Ubik.Physics.Messages.RigidbodyUpdate>(bytes, DataFormat.JSON);
                case "onGrasp":
                    return SerializationUtility.DeserializeValue<Ubik.Physics.Messages.OnGrasp>(bytes, DataFormat.JSON);
                default:
                    throw new System.Exception($"error, message of type {messageType} unknown");
            }
        }

        [System.Serializable]
        public class RigidbodyUpdate : Message
        {
            public override string messageType => "rigidbodyUpdate";

            public Vector3 position;
            public Quaternion rotation;

            public Vector3 linearVelocity;
            public Vector3 angularVelocity;

            public RigidbodyUpdate(Transform transform, UnityEngine.Rigidbody rigidbody)
            {
                this.position = transform.position;
                this.rotation = transform.rotation;

                this.linearVelocity = rigidbody.velocity;
                this.angularVelocity = rigidbody.angularVelocity;
            }
        }

        [System.Serializable]
        public class OnGrasp : Message
        {
            public override string messageType => "onGrasp";
        }
    }
}