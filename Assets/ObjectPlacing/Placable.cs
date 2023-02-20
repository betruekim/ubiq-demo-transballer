using System;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Samples;

namespace Transballer.PlaceableObjects
{
    public abstract class Placeable : NetworkedPhysics.NetworkedObject
    {
        public Snap[] snaps;
        public List<Snap> attachedTo; // external snap nodes that we are connected to
        public virtual bool canBePlacedFreely { get; } = true;
        public abstract int materialCost { get; }

        public bool originalOwner = false;
        protected bool placed = false;

        public virtual Vector3 snappedRotateAxis { get; } = Vector3.forward;

        override public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            if (debug)
            {
                Debug.Log($"{NetworkId} {owner} {message}");
            }
            string type = Transballer.Messages.GetType(message.ToString());
            switch (type)
            {
                case "UI":
                    ui.ProcessMessage(message);
                    break;
                case "onPlace":
                    Debug.Log($"{NetworkId} {owner} {message}");
                    Transballer.Messages.OnPlace placeInfo = Transballer.Messages.OnPlace.Deserialize(message.ToString());
                    OnPlace(placeInfo.snapIndex, placeInfo.snappedTo, placeInfo.snappedToSnapIndex);
                    break;
                case "hoverInfo":
                    HoverInfo info = HoverInfo.Deserialize(message.ToString());
                    if (info.hovered)
                    {
                        OnHovered();
                    }
                    else
                    {
                        OffHovered();
                    }
                    break;
                default:
                    base.ProcessMessage(message);
                    break;
            }
        }

        protected PlaceableUI ui;

        public void UISendMessage(string message)
        {
            ctx.Send(message);
        }


        protected virtual void InitUI()
        {
            ui = GetComponentInChildren<PlaceableUI>();
        }

        void Awake()
        {
            attachedTo = new List<Snap>();
            snaps = GetComponentsInChildren<Snap>();
            int index = 0;
            foreach (Snap snap in snaps)
            {
                snap.placeable = this;
                snap.index = index;
                index++;
            }
            InitUI();
            ui?.gameObject.SetActive(false);

            // start out as a ghost before being placed
            MakeGhost();
        }

        override public void OnSpawned(bool local)
        {
            base.OnSpawned(local);
            // Debug.Log($"onSpawned {Id} {local}");
            PlaceableIndex.AddPlacedObject(this);
        }

        public virtual void Place(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            if (owner)
            {
                ctx.Send(new Transballer.Messages.OnPlace(snapIndex, snappedTo, snappedToSnapIndex).Serialize());
                OnPlace(snapIndex, snappedTo, snappedToSnapIndex);
                originalOwner = true;

                // Play placing sound
                gameObject.GetComponent<AudioSource>().Play();
            }
            else
            {
                throw new System.Exception("called Place() on a remotely controlled placeable!");
            }
        }

        public void Place()
        {
            if (canBePlacedFreely)
            {
                Place(-1, NetworkId.Null, -1);
            }
        }

        protected virtual void OnPlace(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
                // col.gameObject.layer = LayerMask.NameToLayer("Placeable");
                Snap s = col.gameObject.GetComponent<Snap>();
                if (s)
                {
                    col.gameObject.layer = LayerMask.NameToLayer("Snap");
                    s.HideGraphic();
                }
            }
            foreach (Transform t in transform.Find("model").GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.layer == LayerMask.NameToLayer("Default"))
                {
                    t.gameObject.layer = LayerMask.NameToLayer("Placeable");
                }
            }
            if (snapIndex >= 0)
            {
                Placeable placeableSnappedTo = PlaceableIndex.placedObjects[snappedTo];
                Attach(snaps[snapIndex], placeableSnappedTo.snaps[snappedToSnapIndex]);
                placeableSnappedTo.Attach(placeableSnappedTo.snaps[snappedToSnapIndex], snaps[snapIndex]);
            }
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = originalColors[i];
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

        Color[] originalColors;
        MeshRenderer[] renderers;


        public virtual void MakeGhost()
        {
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
                // col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            renderers = GetComponentsInChildren<MeshRenderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }
            placed = false;
        }

        public void SetMeshColors(Color color)
        {
            foreach (MeshRenderer mr in transform.Find("model").GetComponentsInChildren<MeshRenderer>())
            {
                mr.material.color = color;
            }
        }

        public virtual bool CanBePlacedOn(Snap target)
        {
            // override this on certain classes to ensure that only certain objects can be snapped
            // TODO should this exclude carts, so we can never attach something to a cart?
            return true;
        }

        override public void Remove()
        {
            if (originalOwner || !placed)
            {
                base.Remove();
            }
            else
            {
                Debug.Log("can't remove this object, you didn't place it!");
            }
        }

        public void RemoveSudo()
        {
            base.Remove();
        }

        override protected void OnRemove()
        {
            PlaceableIndex.RemovePlacedObject(this);
            for (int i = 0; i < attachedTo.Count; i++)
            {
                Snap otherSnap = attachedTo[i];
                if (otherSnap == null)
                {
                    attachedTo.RemoveAt(i);
                    i--;
                    continue;
                }
                for (int j = 0; j < otherSnap.placeable.attachedTo.Count; j++)
                {
                    Snap mySnap = otherSnap.placeable.attachedTo[j];
                    if (mySnap == null)
                    {
                        otherSnap.placeable.attachedTo.RemoveAt(j);
                        j--;
                        continue;
                    }
                    if (System.Array.IndexOf(snaps, mySnap) > -1)
                    {
                        Detach(mySnap, otherSnap);
                        otherSnap.placeable.Detach(otherSnap, mySnap);
                        i = 0;
                        j = 0;
                    }
                }
            }
            base.OnRemove();
        }

        public void Hover()
        {
            if (placed)
            {
                // Debug.Log("hover");
                OnHovered();
                // send message
                ctx.Send(new HoverInfo(true).Serialize());
            }
        }

        public void UnHover()
        {
            if (placed)
            {
                // Debug.Log("unhover");
                OffHovered();
                // send message
                ctx.Send(new HoverInfo(false).Serialize());
            }
        }

        protected virtual void OnHovered()
        {
            if (ui)
            {
                ui.gameObject.SetActive(true);
            }
        }

        protected virtual void OffHovered()
        {
            if (ui)
            {
                ui.gameObject.SetActive(false);
            }
        }

        [System.Serializable]
        class HoverInfo : Messages.Message
        {
            public override string messageType => "hoverInfo";

            public bool hovered;

            public override string Serialize()
            {
                return $"hoverInfo${hovered}";
            }

            public HoverInfo(bool hovered)
            {
                this.hovered = hovered;
            }

            public static HoverInfo Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new HoverInfo(bool.Parse(components[1]));
            }
        }
    }
}
