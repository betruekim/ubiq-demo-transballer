using UnityEngine;


namespace Transballer.NetworkedPhysics
{
    public class Magnet : NetworkedRigidbody
    {
        public float radius = 5f;
        public float power = 10f;

        override protected void FixedUpdate()
        {
            base.FixedUpdate();
            if (owner)
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
    }
}