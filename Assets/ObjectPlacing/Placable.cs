using System;
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
        public virtual bool canBePlacedFreely { get; } = true;
        public abstract int materialCost { get; }

        public bool originalOwner = false;
        public bool owner = false;
        protected bool placed = false;

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
                case "onRemove":
                    Debug.Log($"{Id} {owner} {message}");
                    OnRemove();
                    break;
                case "newOwner":
                    owner = false;
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

            // start out as a ghost before being placed
            MakeGhost();
        }

        private void OnDestroy()
        {
            PlacableIndex.RemovePlacedObject(this);
        }

        public virtual void OnSpawned(bool local)
        {
            Debug.Log($"onSpawned {Id} {local}");
            owner = local;
            PlacableIndex.AddPlacedObject(this);
        }

        public virtual void Move()
        {
            if (owner)
            {
                ctx.Send(new Messages.PositionUpdate(transform.position, transform.rotation).Serialize());
            }
            else
            {
                throw new System.Exception("called Move() on a remotely controlled placable!");
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

        public void TakeControl()
        {
            ctx.Send(new Messages.NewOwner().Serialize());
            owner = true;
        }

        public virtual void Place(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            if (owner)
            {
                ctx.Send(new Messages.OnPlace(snapIndex, snappedTo, snappedToSnapIndex).Serialize());
                // Debug.Log(new Messages.OnPlace(snapIndex, snappedTo, snappedToSnapIndex).Serialize());
                OnPlace(snapIndex, snappedTo, snappedToSnapIndex);
                originalOwner = true;

                foreach (MeshRenderer mr in transform.Find("model").GetComponentsInChildren<MeshRenderer>())
                {
                    mr.material.color = Color.white;
                }
            }
            else
            {
                throw new System.Exception("called Place() on a remotely controlled placable!");
            }
        }

        public void Place()
        {
            if (canBePlacedFreely)
            {
                Place(-1, null, -1);
            }
        }

        protected virtual void OnPlace(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
                col.gameObject.layer = LayerMask.NameToLayer("Placable");
                Snap s = col.gameObject.GetComponent<Snap>();
                if (s)
                {
                    col.gameObject.layer = LayerMask.NameToLayer("Snap");
                    s.HideGraphic();
                }
            }
            if (snapIndex >= 0)
            {
                Placable placableSnappedTo = PlacableIndex.placedObjects[snappedTo];
                Attach(snaps[snapIndex], placableSnappedTo.snaps[snappedToSnapIndex]);
                placableSnappedTo.Attach(placableSnappedTo.snaps[snappedToSnapIndex], snaps[snapIndex]);
            }
            placed = true;
        }

        public virtual void Attach(Snap mine, Snap other)
        {
            // mine is our snap object, other is the snap object we are attaching to
            mine.GetComponent<Collider>().enabled = false;
            attachedTo.Add(other);
        }

        public virtual void Detach(Snap mine, Snap other)
        {
            // mine is our snap object, other is the snap object we are attaching to
            if (!attachedTo.Contains(other))
            {
                throw new System.Exception("Tried to detach a snap we are not attached to!");
            }
            mine.GetComponent<Collider>().enabled = true;
            attachedTo.Remove(other);
        }

        public virtual void MakeGhost()
        {
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
                col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            placed = false;
        }

        public void PlaceGood()
        {
            foreach (MeshRenderer mr in transform.Find("model").GetComponentsInChildren<MeshRenderer>())
            {
                mr.material.color = Color.green;
            }
        }

        public void PlaceBad()
        {
            foreach (MeshRenderer mr in transform.Find("model").GetComponentsInChildren<MeshRenderer>())
            {
                mr.material.color = Color.red;
            }
        }

        public virtual bool CanBePlacedOn(Snap target)
        {
            // override this on certain classes to ensure that only certain objects can be snapped
            // TODO should this exclude carts, so we can never attach something to a cart?
            return true;
        }

        public virtual void Remove()
        {
            if (originalOwner)
            {
                ctx.Send(new Messages.OnRemove().Serialize());
                OnRemove();
            }
            else
            {
                Debug.Log("can't remove this object, you didn't place it!");
            }
        }

        public virtual void OnRemove()
        {
            for (int i = 0; i < attachedTo.Count; i++)
            {
                Snap otherSnap = attachedTo[i];
                for (int j = 0; j < otherSnap.placable.attachedTo.Count; j++)
                {
                    Snap mySnap = otherSnap.placable.attachedTo[j];
                    if (System.Array.IndexOf(snaps, mySnap) > -1)
                    {
                        Detach(mySnap, otherSnap);
                        otherSnap.placable.Detach(otherSnap, mySnap);
                        i = 0;
                        j = 0;
                    }
                }
            }
            Destroy(this.gameObject);
        }

        public virtual void OnHovered()
        {

        }

        public virtual void OffHovered()
        {

        }
    }
}
