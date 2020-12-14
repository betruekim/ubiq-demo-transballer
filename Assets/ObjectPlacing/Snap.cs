using UnityEngine;
using System;
using System.Reflection;

namespace Transballer.PlaceableObjects
{
    public class Snap : MonoBehaviour
    {
        public Placeable placeable;
        public int index;
        public Vector3 flipDir = Vector3.up;

        public static Quaternion GetMatchingRotation(Snap grounded, Snap snapper)
        {
            // Quaternion inverseOfOther = Quaternion.AngleAxis(180, grounded.transform.rotation * grounded.flipDir) * grounded.transform.rotation;
            Quaternion inverseOfOther = Quaternion.LookRotation(-grounded.transform.forward, grounded.transform.up);
            return inverseOfOther * Quaternion.Inverse(snapper.transform.localRotation);
        }

        public static Vector3 GetMatchingPosition(Snap grounded, Snap snapper)
        {
            return grounded.transform.position - GetMatchingRotation(grounded, snapper) * snapper.transform.localPosition;
        }

        static MethodInfo rotateAround = typeof(Transform).GetMethod("RotateAround", new Type[] { typeof(Vector3), typeof(Vector3), typeof(float) });
        public static void SetExtraRotation(Snap grounded, Snap snapper, float angle)
        {
            // this has to be run after moving the snapper using GetMatchingPos and GetMatchingRot
            // since RotateAround is an internal function, we have to use Reflection to access it
            Vector3 axis = snapper.transform.rotation * snapper.placeable.snappedRotateAxis;
            rotateAround.Invoke(snapper.placeable.transform, new object[] { snapper.transform.position, axis, angle });
        }

        GameObject snapGraphic;
        Vector3 snapTargetSize = Vector3.zero;
        Vector3 snapSize = Vector3.zero;
        private void Awake()
        {
            snapGraphic = transform.GetChild(0).gameObject;
        }

        public void ShowGraphic()
        {
            snapTargetSize = Vector3.one * 0.1f;
        }

        public void HideGraphic()
        {
            snapTargetSize = Vector3.zero;
        }

        private void Update()
        {
            snapSize = Vector3.Lerp(snapSize, snapTargetSize, Time.deltaTime * 20f);
            snapGraphic.transform.localScale = snapSize;
        }
    }
}