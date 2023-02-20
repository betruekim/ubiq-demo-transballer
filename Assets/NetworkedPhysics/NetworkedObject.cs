using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

namespace Transballer.NetworkedPhysics
{
    public abstract class NetworkedObject : MonoBehaviour, INetworkSpawnable
    {
        public NetworkId NetworkId { get; set; }
        protected NetworkContext ctx;
        public bool owner = true;
        public bool debug = false;

        public virtual void Start()
        {
            ctx = NetworkScene.Register(this);
        }

        public virtual void OnSpawned(bool local)
        {
            owner = local;
        }

        public virtual void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            // override this in derived classes and then run Base.processMessage in the default case
            string messageType = Messages.GetType(message.ToString());

            switch (messageType)
            {
                case "positionUpdate":
                    Messages.PositionUpdate positionUpdate = Messages.PositionUpdate.Deserialize(message.ToString());
                    OnMove(positionUpdate);
                    break;
                case "remove":
                    OnRemove();
                    break;
                case "newOwner":
                    OnTakeControl();
                    break;
                default:
                    throw new System.Exception($"error, message of type {messageType} unknown");
            }
        }

        public virtual void Move()
        {
            // sends position updates to other clients
            if (owner)
            {
                ctx.Send(new Messages.PositionUpdate(transform.position, transform.rotation).Serialize());
            }
            else
            {
                throw new System.Exception("called Move() on a remotely controlled placeable!");
            }
        }

        protected virtual void OnMove(Messages.PositionUpdate positionUpdate)
        {
            if (owner)
            {
                throw new System.Exception("Received position update for locally controlled gameobject");
            }
            transform.position = positionUpdate.position;
            transform.rotation = positionUpdate.rotation;
        }


        public virtual void TakeControl()
        {
            owner = true;
            ctx.Send(new Messages.NewOwner().Serialize());
        }

        public virtual void OnTakeControl()
        {
            owner = false;
        }

        public virtual void Remove()
        {
            ctx.Send(new Messages.Remove().Serialize());
            OnRemove();
        }

        protected virtual void OnRemove()
        {
            GameObject.Destroy(this.gameObject);
        }
    }

}
