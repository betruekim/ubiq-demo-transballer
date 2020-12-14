using System.Collections;
using System.Collections.Generic;
using Ubik.Messaging;
using UnityEngine;

namespace Transballer.PlaceableObjects
{
    public class CartTrack : Placeable
    {
        public override int materialCost => 10;
        public override bool CanBePlacedOn(Snap target)
        {
            // we can only snap a cart to a cartTrack, and we can only snap to the top snap
            return target.index != 2 && System.Object.ReferenceEquals(target.placeable.GetType(), typeof(Transballer.PlaceableObjects.CartTrack));
        }

        protected override void OnPlace(int snapIndex, NetworkId snappedTo, int snappedToSnapIndex)
        {
            base.OnPlace(snapIndex, snappedTo, snappedToSnapIndex);
            if (snapIndex > -1)
            {
                // ^ is XOR operator
                if (snapIndex == snappedToSnapIndex ^ PlaceableIndex.placedObjects[snappedTo].GetComponent<CartTrack>().reversed)
                {
                    trackPoints.Reverse();
                    reversed = true;
                    foreach (Transform trackPoint in trackPoints)
                    {
                        trackPoint.localRotation = trackPoint.localRotation * Quaternion.AngleAxis(180, Vector3.up);
                    }
                }
            }
        }

        public List<Transform> trackPoints;
        public bool reversed = false;
        float frac = 1f;

        protected override void Awake()
        {
            base.Awake();
            trackPoints = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.layer == LayerMask.NameToLayer("TrackPoint"))
                {
                    trackPoints.Add(transform.GetChild(i));
                }
            }
            frac = 1f / (float)trackPoints.Count;
        }

        private (int, int, float) GetStint(float progress)
        {
            float partitions = trackPoints.Count - 1;

            if (progress >= 1)
            {
                return (trackPoints.Count - 2, (int)partitions, 1.1f);
            }
            else if (progress <= 0)
            {
                return (0, 1, -0.1f);
            }

            int start = Mathf.FloorToInt(progress * partitions);
            int end = start + 1;
            float stint = (progress - ((float)start / partitions)) * partitions;

            return (start, end, stint);
        }

        private Vector3 GetCartPos(int start, int end, float stint)
        {
            return Vector3.Lerp(trackPoints[start].position, trackPoints[end].position, stint);
        }

        private Quaternion GetCartRot(int start, int end, float stint)
        {
            return Quaternion.Lerp(trackPoints[start].rotation, trackPoints[end].rotation, stint);
        }

        public Vector3 GetCartPos(float progress)
        {
            var (start, end, stint) = GetStint(progress);
            return GetCartPos(start, end, stint) + GetCartRot(start, end, stint) * Vector3.up * 0.1f;
        }

        public Quaternion GetCartRot(float progress)
        {
            var (start, end, stint) = GetStint(progress);
            return GetCartRot(start, end, stint);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var point in trackPoints)
            {
                Vector3 pos = point.transform.position;
                Vector3 dir = point.transform.forward * 0.4f;
                float arrowHeadAngle = 20f;
                float arrowHeadLength = 0.2f;
                Gizmos.DrawRay(pos, dir);
                Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Gizmos.DrawRay(pos + dir, right * arrowHeadLength);
                Gizmos.DrawRay(pos + dir, left * arrowHeadLength);
            }
        }

        public override void Attach(Snap mine, Snap other)
        {
            base.Attach(mine, other);

            mine.transform.Find("end")?.gameObject.SetActive(false);
            other.transform.Find("end")?.gameObject.SetActive(false);
        }

        public override void Detach(Snap mine, Snap other)
        {
            base.Detach(mine, other);
            mine.transform.Find("end")?.gameObject.SetActive(true);
            other.transform.Find("end")?.gameObject.SetActive(true);
        }
    }

}

