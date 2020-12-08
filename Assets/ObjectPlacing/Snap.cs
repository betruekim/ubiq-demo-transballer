using UnityEngine;

namespace PlacableObjects
{
    public class Snap : MonoBehaviour
    {
        public Placable placable;
        public int index;
        public Vector3 flipDir = Vector3.up;

        public static Quaternion GetMatchingRotation(Snap grounded, Snap snapper)
        {
            Quaternion inverseOfOther = Quaternion.AngleAxis(180, grounded.transform.rotation * grounded.flipDir) * grounded.transform.rotation;
            return inverseOfOther * Quaternion.Inverse(snapper.transform.localRotation);
        }

        public static Vector3 GetMatchingPosition(Snap grounded, Snap snapper)
        {
            return grounded.transform.position - GetMatchingRotation(grounded, snapper) * snapper.transform.localPosition;
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