using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;
using Ubik.Messaging;

namespace Transballer.PlaceableObjects
{
    public class Cannon : Placeable
    {
        public override int materialCost => 10;
        public override bool canBePlacedFreely => false;

        public GameObject barrelPivot;
        public Transform launchPoint;
        public BoxCollider pickup;

        public float exitVelocity = 1;
        public Quaternion barrelAngle = Quaternion.Euler(-60, 0, 0);
        public float launchDelay = 1f;

        Transballer.NetworkedPhysics.NetworkedRigidbody currentBall = null;

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
                    Transballer.NetworkedPhysics.NetworkedRigidbody ball = col.gameObject.GetComponent<Transballer.NetworkedPhysics.NetworkedRigidbody>();

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
                    currentBall.transform.position = launchPoint.transform.position + launchPoint.transform.up;
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
                currentBall.transform.position = launchPoint.transform.position + launchPoint.transform.up;
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

        Hand hingeGraspingController = null;

        void HingeGrasped(Hand controller)
        {
            hingeGraspingController = controller;
        }

        void HingeReleased(Hand controller)
        {
            hingeGraspingController = null;
            ctx.Send(new Transballer.Messages.MoveCannonAngle(launchPoint.parent.parent.rotation).Serialize());
        }

        public override void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string messageType = Transballer.Messages.GetType(message.ToString());
            switch (messageType)
            {
                case "moveCannonAngle":
                    launchPoint.parent.parent.rotation = Transballer.Messages.MoveCannonAngle.Deserialize(message.ToString()).angle;
                    break;
                default:
                    base.ProcessMessage(message);
                    break;
            }

        }

        private void OnEnable()
        {
            launchPoint.GetComponent<RemoteGraspable>().OnGrasp += HingeGrasped;
            launchPoint.GetComponent<RemoteGraspable>().OnRelease += HingeReleased;
        }

        private void OnDisable()
        {
            launchPoint.GetComponent<RemoteGraspable>().OnGrasp -= HingeGrasped;
            launchPoint.GetComponent<RemoteGraspable>().OnRelease -= HingeReleased;
        }

        private void Update()
        {
            if (hingeGraspingController)
            {
                Vector3 dir = (hingeGraspingController.transform.position - launchPoint.parent.parent.position).normalized;
                launchPoint.parent.parent.rotation = Quaternion.LookRotation(transform.right, dir);
            }
        }
    }
}