using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;

namespace PlacableObjects
{
    // TODO: one day make this an abstract class so that pipes with functionality inherit
    public abstract class Placable : MonoBehaviour, INetworkComponent, INetworkObject, ISpawnable
    {
        public NetworkId Id { get; } = new NetworkId();
        NetworkContext ctx;

        public Snap snapA, snapB;
        public List<Snap> snappedTo;

        bool owner = false;


        public virtual void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            // Debug.Log($"{Id} {owner} {message}");
            string type = Messages.GetType(message.ToString());
            switch (type)
            {
                case "positionUpdate":
                    if (owner)
                    {
                        throw new System.Exception("Received position update for locally controlled placable");
                    }
                    Messages.PositionUpdate update = Messages.PositionUpdate.Deserialize(message.ToString());
                    transform.position = update.position;
                    transform.rotation = update.rotation;
                    break;
                case "onDestroy":
                    if (owner)
                    {
                        throw new System.Exception("Received destroy update for locally controlled placable");
                    }
                    Destroy(this.gameObject);
                    break;
                case "onPlace":
                    if (owner)
                    {
                        throw new System.Exception("Received onPlace update for locally controlled placable");
                    }
                    OnPlace();
                    break;
                default:
                    throw new System.Exception($"unknown message type {type}");
            }
        }

        protected virtual void Awake()
        {
            snappedTo = new List<Snap>();
            snapA = transform.GetChild(1).GetComponent<Snap>();
            snapB = transform.GetChild(2).GetComponent<Snap>();
            snapA.placable = this;
            snapB.placable = this;
            ctx = NetworkScene.Register(this);
        }

        public virtual void OnSpawned(bool local)
        {
            Debug.Log($"onSpawned {Id} {local}");
            owner = local;
        }

        public virtual void Move()
        {
            if (owner)
            {
                ctx.Send(new Messages.PositionUpdate(transform.position, transform.rotation).Serialize());
            }
            else
            {
                throw new System.Exception("called Place() on a remotely controlled placable!");
            }
        }

        public virtual void Deselect()
        {
            // destroy this object
            if (owner)
            {
                ctx.Send(new Messages.OnDestroy().Serialize());
            }
            else
            {
                throw new System.Exception("called Place() on a remotely controlled placable!");
            }

            Destroy(this.gameObject);
        }

        // there is currently a race condition between place and OnSpawned
        // solution will be: spawn ghostObject across the network and reuse that object when placed
        public virtual void Place()
        {
            if (owner)
            {
                ctx.Send(new Messages.PositionUpdate(transform.position, transform.rotation).Serialize());
                OnPlace();
            }
            else
            {
                throw new System.Exception("called Place() on a remotely controlled placable!");
            }
        }

        protected virtual void OnPlace()
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }
            // figure out snapping stuff here
        }
    }
}
