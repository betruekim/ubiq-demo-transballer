using UnityEngine;


namespace Transballer.NetworkedPhysics
{
    public class Bat : NetworkedRigidbody
    {
        public override Vector3 graspPoint => Vector3.down * 0.1f;
        public override Vector3 graspForward => Vector3.right;
        public override Vector3 graspUp => Vector3.up;
    }
}