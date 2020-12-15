using UnityEngine;
using Ubik.XR;

namespace Transballer.NetworkedPhysics
{
    public class Magnet : NetworkedRigidbody, IUseable
    {
        public float radius = 5f;
        public float power = 10f;
        public bool on = true;
        public override Vector3 graspPoint => Vector3.back * 0.05f;
        public override Vector3 graspForward => Vector3.forward;
        public override Vector3 graspUp => Vector3.right;

        override protected void FixedUpdate()
        {
            base.FixedUpdate();
            if (owner && on)
            {
                var ballsInRadius = Physics.OverlapSphere(transform.position, radius, 1 << LayerMask.NameToLayer("Ball"));
                foreach (var ballCol in ballsInRadius)
                {
                    Ball ball = ballCol.gameObject.GetComponent<Ball>();
                    if (!ball.owner)
                    {
                        ball.TakeControl();
                        continue;
                    }
                    Vector3 attraction = transform.position - ball.transform.position;
                    attraction /= attraction.sqrMagnitude;
                    attraction *= power;
                    ball.rb.AddForce(attraction);
                }
            }
        }

        void IUseable.UnUse(Hand controller)
        {
            // TODO: should this be a toggle or a press and hold?
        }

        void IUseable.Use(Hand controller)
        {
            on = !on;
        }
    }
}