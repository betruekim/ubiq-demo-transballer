using UnityEngine;
using Ubiq.Messaging;

namespace Transballer
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
        public class Remove : Message
        {
            public override string messageType => "remove";

            public override string Serialize()
            {
                return "remove$";
            }
        }

        [System.Serializable]
        public class NewOwner : Message
        {
            public override string messageType => "newOwner";

            public override string Serialize()
            {
                return "newOwner$";
            }
        }

        [System.Serializable]
        public class RigidbodyUpdate : Message
        {
            public override string messageType => "rigidbodyUpdate";
            public Vector3 linearVelocity;
            public Vector3 angularVelocity;

            public RigidbodyUpdate(UnityEngine.Rigidbody rigidbody)
            {
                this.linearVelocity = rigidbody.velocity;
                this.angularVelocity = rigidbody.angularVelocity;
            }

            private RigidbodyUpdate()
            {
                // private default constructor to allow for Deserialize() below
            }

            public override string Serialize()
            {

                string message = $"{this.messageType}${JsonUtility.ToJson(this.linearVelocity)}${JsonUtility.ToJson(this.angularVelocity)}";

                return message;
            }

            public static RigidbodyUpdate Deserialize(string message)
            {
                string[] components = message.Split('$');
                RigidbodyUpdate update = new RigidbodyUpdate();
                update.linearVelocity = JsonUtility.FromJson<Vector3>(components[1]);
                update.angularVelocity = JsonUtility.FromJson<Vector3>(components[2]);
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
        public class SetKinematic : Message
        {
            public override string messageType => "setKinematic";
            public bool state;

            public override string Serialize()
            {
                return $"setKinematic${state}";
            }

            public SetKinematic(bool state)
            {
                this.state = state;
            }

            public static SetKinematic Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new SetKinematic(bool.Parse(components[1]));
            }
        }

        [System.Serializable]
        public class OnPlace : Message
        {
            public override string messageType => "onPlace";

            public int snapIndex;
            public NetworkId snappedTo;
            public int snappedToSnapIndex;

            public OnPlace(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
            {
                this.snapIndex = snapIndex;
                this.snappedTo = snappedTo;
                this.snappedToSnapIndex = snappedToSnapIndex;
            }

            public override string Serialize()
            {
                if (snapIndex >= 0)
                {
                    return $"onPlace${snapIndex}${snappedTo.ToString()}${snappedToSnapIndex}";
                }
                else
                {
                    return $"onPlace$-1$-1$-1";
                }
            }

            public static OnPlace Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new OnPlace(int.Parse(components[1]), new NetworkId(components[2]), int.Parse(components[3]));
            }
        }
    }
}