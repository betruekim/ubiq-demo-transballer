using UnityEngine;
using Ubik.Messaging;
using System.Collections.Generic;

namespace Transballer.PlaceableObjects
{
    public class Cart : Placeable
    {
        public override int materialCost => 10;
        public override bool canBePlacedFreely => false;
        // need to move this bad boy back to original position on level start
        private Snap originalSnap;

        public override bool CanBePlacedOn(Snap target)
        {
            // we can only snap a cart to a cartTrack, and we can only snap to the top snap
            return target.index == 2 && System.Object.ReferenceEquals(target.placeable.GetType(), typeof(Transballer.PlaceableObjects.CartTrack));
        }

        override public void Place(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            base.Place(snapIndex, snappedTo, snappedToSnapIndex);  // base checks for ownership so we dont need to
            originalSnap = PlaceableIndex.placedObjects[snappedTo].snaps[snappedToSnapIndex];
            // not sure if we need to claim ownership of all connected tracks?
            HashSet<Placeable> connectedObjects = new HashSet<Placeable>();
            AddConnected(connectedObjects, this);
            foreach (Placeable connected in connectedObjects)
            {
                if (!connected.owner)
                {
                    connected.TakeControl();
                }
            }
        }

        private void AddConnected(HashSet<Placeable> set, Placeable target)
        {
            if (set.Contains(target))
            {
                return;
            }
            set.Add(target);
            foreach (Snap child in target.attachedTo)
            {
                AddConnected(set, child.placeable);
            }
        }

        List<Placeable> attachedToTop = new List<Placeable>();
        public override void Attach(Snap mine, Snap other)
        {
            base.Attach(mine, other);
            if (snaps.Length == 2)
            {
                if (mine == snaps[1])
                {
                    // if the thing attached was on the top snap point
                    // take control of it, make it a child of us and add it to a list?
                    other.placeable.TakeControl();
                    attachedToTop.Add(other.placeable);
                    other.placeable.transform.SetParent(transform);
                }
            }
        }

        [SerializeField]
        CartTrack lastTrack;
        [SerializeField]
        float progress = 0f;
        [SerializeField]
        float speed = 1f;
        private void FixedUpdate()
        {
            if (!owner || !placed)
            {
                return;
            }
            progress += Time.fixedDeltaTime * speed;
            CartTrack currentTrack = null;
            RaycastHit hit;
            Ray trackRay = new Ray(transform.position + transform.forward * 0.04f * Mathf.Sign(speed), -transform.up);
            Debug.DrawRay(trackRay.origin, trackRay.direction, Color.red, Time.fixedDeltaTime);
            if (Physics.Raycast(trackRay.origin, trackRay.direction, out hit, 1f, 1 << LayerMask.NameToLayer("TrackCollider")))
            {
                currentTrack = hit.collider.transform.parent.gameObject.GetComponent<CartTrack>();
                if (currentTrack != lastTrack)
                {
                    if (lastTrack == null)
                    {
                        progress = 0.5f;
                    }
                    else
                    {
                        progress = 0.5f * (1 - Mathf.Sign(speed));
                    }
                    lastTrack = currentTrack;
                }
            }

            Ray endRay = new Ray(transform.position + transform.forward * 0.3f * Mathf.Sign(speed), transform.forward * Mathf.Sign(speed));
            Debug.DrawRay(endRay.origin, endRay.direction, Color.red, Time.fixedDeltaTime);
            if (Physics.Raycast(endRay.origin, endRay.direction, out hit, 0.1f, 1 << LayerMask.NameToLayer("Snap")))
            {
                speed *= -1;
            }

            if (currentTrack)
            {
                transform.position = currentTrack.GetCartPos(progress);
                transform.rotation = currentTrack.GetCartRot(progress);
            }
            base.Move();
            foreach (Placeable attached in attachedToTop)
            {
                if (attached.owner)
                {
                    attached.Move();
                }
            }
        }
    }
}