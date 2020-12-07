using UnityEngine;

namespace PlacableObjects
{
    public class Snap : MonoBehaviour
    {
        public Placable placable;
        public int index;

        public static Quaternion GetMatchingRotation(GameObject grounded, GameObject snapper)
        {
            Quaternion inverseOfOther = Quaternion.AngleAxis(180, grounded.transform.up) * grounded.transform.rotation;
            return inverseOfOther * Quaternion.Inverse(snapper.transform.localRotation);
        }

        public static Vector3 GetMatchingPosition(GameObject grounded, GameObject snapper)
        {
            return grounded.transform.position - GetMatchingRotation(grounded, snapper) * snapper.transform.localPosition;
        }
    }
}