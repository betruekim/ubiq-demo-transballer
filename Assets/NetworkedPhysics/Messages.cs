using UnityEngine;

namespace Ubik.Messaging
{
    public class Messages
    {

        [System.Serializable]
        public abstract class Message
        {
            public abstract string messageType { get; }

            public abstract string Serialize();
        }

        public static string GetType(string message)
        {
            return message.Split('$')[0];
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

            private RigidbodyUpdate()
            {
                // private default constructor to allow for Deserialize() below
            }

            public override string Serialize()
            {

                string message = $"{this.messageType}${JsonUtility.ToJson(this.position)}${JsonUtility.ToJson(this.rotation)}${JsonUtility.ToJson(this.linearVelocity)}${JsonUtility.ToJson(this.angularVelocity)}";

                return message;
            }

            public static RigidbodyUpdate Deserialize(string message)
            {
                string[] components = message.Split('$');
                RigidbodyUpdate update = new RigidbodyUpdate();
                update.position = JsonUtility.FromJson<Vector3>(components[1]);
                update.rotation = JsonUtility.FromJson<Quaternion>(components[2]);
                update.linearVelocity = JsonUtility.FromJson<Vector3>(components[3]);
                update.angularVelocity = JsonUtility.FromJson<Vector3>(components[4]);
                return update;
            }
        }

        [System.Serializable]
        public class GraspUpdate : Message
        {
            public override string messageType => "graspUpdate";
            public bool grasped;

            public GraspUpdate(bool grasped)
            {
                this.grasped = grasped;
            }

            public override string Serialize()
            {
                return $"{this.messageType}${grasped.ToString()}";
            }

            public static GraspUpdate Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new GraspUpdate(bool.Parse(components[1]));
            }
        }

        [System.Serializable]
        public class OnDestroy : Message
        {
            public override string messageType => "onDestroy";

            public override string Serialize()
            {
                return "onDestroy$";
            }
        }

        [System.Serializable]
        public class PositionUpdate : Message
        {
            public override string messageType => "positionUpdate";

            public Vector3 position;
            public Quaternion rotation;

            public PositionUpdate(Vector3 position, Quaternion rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }

            public override string Serialize()
            {
                return $"{this.messageType}${JsonUtility.ToJson(this.position)}${JsonUtility.ToJson(this.rotation)}";
            }

            public static PositionUpdate Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new PositionUpdate(JsonUtility.FromJson<Vector3>(components[1]), JsonUtility.FromJson<Quaternion>(components[2])); ;
            }
        }

        [System.Serializable]
        public class OnPlace : Message
        {
            public override string messageType => "onPlace";

            public override string Serialize()
            {
                return "onPlace$";
            }
        }
    }
}