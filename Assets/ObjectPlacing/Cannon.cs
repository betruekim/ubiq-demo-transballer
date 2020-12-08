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

        public float exitVelocity = 1;
        public Quaternion barrelAngle = Quaternion.Euler(-60, 0, 0);
        public float launchDelay = 1f;

        Ubik.Physics.Rigidbody currentBall = null;

        LineRenderer arcRenderer;

        protected override void Awake()
        {
            base.Awake();
            pickup.enabled = false;
            arcRenderer = GetComponentInChildren<LineRenderer>();
        }

        float launchedElapsed = 0f;
        private void FixedUpdate()
        {
            if (!owner || !placed)
            {
                return;
            }

            if (currentBall == null)
            {
                // check for things inside trigger
                LayerMask ballMask = (1 << LayerMask.NameToLayer("Ball"));
                Collider[] balls = Physics.OverlapBox(pickup.transform.position, pickup.transform.lossyScale / 2, pickup.transform.rotation, ballMask);
                foreach (var col in balls)
                {
                    Ubik.Physics.Rigidbody ball = col.gameObject.GetComponent<Ubik.Physics.Rigidbody>();

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
                    ball.transform.position = launchPoint.transform.position;
                    currentBall = ball;
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

        public override void Move()
        {
            base.Move();
            if (currentBall)
            {
                currentBall.transform.position = launchPoint.transform.position;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(pickup.transform.position, pickup.transform.lossyScale);
        }

        public override void OffHovered()
        {
            base.OffHovered();
            arcRenderer.enabled = false;
        }

        public override void OnHovered()
        {
            // shows arc over one second of travel
            base.OnHovered();
            if (placed)
            {
                arcRenderer.enabled = true;
                arcRenderer.positionCount = 20;
                for (int i = 0; i < arcRenderer.positionCount; i++)
                {
                    float t = 2 * (float)i / (float)arcRenderer.positionCount;
                    // s = ut + 0.5at^2
                    arcRenderer.SetPosition(i, launchPoint.transform.position + launchPoint.transform.up * exitVelocity * t + 0.5f * Physics.gravity * t * t);
                }
            }
        }
    }
}