using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;

namespace PlacableObjects
{
    public abstract class Placable : MonoBehaviour, INetworkComponent, INetworkObject, ISpawnable
    {
        public NetworkId Id { get; } = new NetworkId();
        NetworkContext ctx;

        public Snap[] snaps;
        public List<Snap> attachedTo; // external snap nodes that we are connected to

        public bool owner = false;


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
                    Debug.Log($"{Id} {owner} {message}");
                    if (owner)
                    {
                        throw new System.Exception("Received onPlace update for locally controlled placable");
                    }
                    Messages.OnPlace placeInfo = Messages.OnPlace.Deserialize(message.ToString());
                    OnPlace(placeInfo.snapIndex, placeInfo.snappedTo, placeInfo.snappedToSnapIndex);
                    break;
                default:
                    throw new System.Exception($"unknown message type {type}");
            }
        }

        protected virtual void Awake()
        {
            attachedTo = new List<Snap>();
            snaps = GetComponentsInChildren<Snap>();
            int index = 0;
            foreach (Snap snap in snaps)
            {
                snap.placable = this;
                snap.index = index;
                index++;
            }
            ctx = NetworkScene.Register(this);
            PlacableManager.AddPlacedObject(this);

            // start out as a ghost before being placed
            MakeGhost();
        }

        private void OnDestroy()
        {
            PlacableManager.RemovePlacedObject(this);
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

        public virtual void Place(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            if (owner)
            {
                ctx.Send(new Messages.OnPlace(snapIndex, snappedTo, snappedToSnapIndex).Serialize());
                // Debug.Log(new Messages.OnPlace(snapIndex, snappedTo, snappedToSnapIndex).Serialize());
                OnPlace(snapIndex, snappedTo, snappedToSnapIndex);
            }
            else
            {
                throw new System.Exception("called Place() on a remotely controlled placable!");
            }
        }

        public void Place()
        {
            Place(-1, null, -1);
        }

        protected virtual void OnPlace(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
                col.gameObject.layer = LayerMask.NameToLayer("Default");
                if (col.gameObject.GetComponent<Snap>())
                {
                    col.gameObject.layer = LayerMask.NameToLayer("Snap");
                }
            }
            if (snapIndex >= 0)
            {
                Placable placableSnappedTo = PlacableManager.placedObjects[snappedTo];
                Attach(snaps[snapIndex], placableSnappedTo.snaps[snappedToSnapIndex]);
                placableSnappedTo.Attach(placableSnappedTo.snaps[snappedToSnapIndex], snaps[snapIndex]);
            }

        }

        public void Attach(Snap mine, Snap other)
        {
            // mine is our snap object, other is the snap object we are attaching to
            mine.GetComponent<Collider>().enabled = false;
            attachedTo.Add(other);
        }

        public virtual void MakeGhost()
        {
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
                col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }
}
