using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlacableObjects
{
    public class Cannon : Placable
    {
        public GameObject barrelPivot;
        public Transform launchPoint;
        public BoxCollider pickup;

        public float period = 1f;
        public float exitVelocity = 1;
        public Quaternion barrelAngle = Quaternion.Euler(-60, 0, 0);
        public float launchDelay = 1f;

        Ubik.Physics.Rigidbody currentBall = null;

        protected override void Awake()
        {
            base.Awake();
            pickup.enabled = false;
        }

        float elapsed = 0f;
        float launchedElapsed = 0f;
        private void FixedUpdate()
        {
            if (!owner || !placed)
            {
                return;
            }

            if (currentBall == null)
            {
                elapsed += Time.fixedDeltaTime;
                if (elapsed > period)
                {
                    // check for things inside trigger
                    LayerMask ballMask = (1 << LayerMask.NameToLayer("Ball"));
                    Collider[] balls = Physics.OverlapBox(pickup.transform.position, pickup.size, pickup.transform.rotation, ballMask);
                    Debug.Log(balls.Length);
                    foreach (var col in balls)
                    {
                        Debug.Log(col.gameObject);
                        Ubik.Physics.Rigidbody ball = col.gameObject.GetComponent<Ubik.Physics.Rigidbody>();
                        Debug.Log(ball);

                        if (ball.graspedRemotely || ball.graspingController)
                        {
                            continue;
                        }

                        if (!ball.owner)
                        {
                            ball.TakeControl();
                            continue;
                        }
                        ball.SetKinematic(true);
                        currentBall = ball;
                        elapsed = 0f;
                    }
                }
            }
            else
            {
                launchedElapsed += Time.fixedDeltaTime;
                if (launchedElapsed > launchDelay)
                {
                    // launch ball
                    currentBall.transform.position = launchPoint.transform.position;
                    currentBall.transform.rotation = launchPoint.transform.rotation;
                    currentBall.SetKinematic(false);
                    currentBall.rb.velocity = launchPoint.transform.up * exitVelocity;
                    currentBall = null;
                    launchedElapsed = 0f;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(pickup.transform.position, pickup.size);
        }
    }
}
