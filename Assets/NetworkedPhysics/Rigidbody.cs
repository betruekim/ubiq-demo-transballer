using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;
using Ubik.XR;


namespace Ubik.Physics
{
    public class Rigidbody : MonoBehaviour, INetworkObject, INetworkComponent, ISpawnable, IGraspable
    {
        public NetworkId Id { get; } = new NetworkId();

        public UnityEngine.Rigidbody rb;
        public float velSquareMag { get { return rb.velocity.sqrMagnitude; } }

        NetworkContext ctx;
        RigidbodyManager manager;
        public bool owner = true;
        public Hand graspingController;
        public bool graspedRemotely = false;

        private void Awake()
        {
            ctx = NetworkScene.Register(this);
            manager = GameObject.FindObjectOfType<RigidbodyManager>();
            rb = GetComponent<UnityEngine.Rigidbody>();
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgString = message.ToString();
            string messageType = Messages.GetType(msgString);

            switch (messageType)
            {
                case "rigidbodyUpdate":
                    if (owner)
                    {
                        throw new System.Exception("received rigidbody update for locally controlled gameobject");
                    }
                    Messages.RigidbodyUpdate rbUpdate = Messages.RigidbodyUpdate.Deserialize(msgString);
                    transform.position = rbUpdate.position;
                    transform.rotation = rbUpdate.rotation;
                    rb.velocity = rbUpdate.linearVelocity;
                    rb.angularVelocity = rbUpdate.angularVelocity;
                    break;
                case "graspUpdate":
                    // Debug.Log("received onGrasp notification, turning off owner");
                    Debug.Log(msgString);
                    owner = false;
                    graspingController = null;
                    Messages.GraspUpdate graspUpdate = Messages.GraspUpdate.Deserialize(msgString);
                    // disable gravity when picked up
                    rb.useGravity = !graspUpdate.grasped;
                    graspedRemotely = graspUpdate.grasped;
                    break;
                case "newOwner":
                    Debug.Log("newOwner");
                    owner = false;
                    break;
                case "setKinematic":
                    if (owner)
                    {
                        throw new System.Exception("received setKinematic update for locally controlled gameobject");
                    }
                    Messages.SetKinematic setKinematic = Messages.SetKinematic.Deserialize(msgString);
                    rb.isKinematic = setKinematic.state;
                    break;
                default:
                    throw new System.Exception($"error, message of type {messageType} unknown");
            }
        }

        public void SendUpdate()
        {
            if (owner)
            {
                ctx.Send(new Messages.RigidbodyUpdate(transform, rb).Serialize());
            }
        }

        public void OnSpawned(bool local)
        {
            owner = local;
            manager.Register(this);
        }

        public void Grasp(Hand controller)
        {
            // this client has grasped this object
            // become locally controlled and disable gravity
            owner = true;
            graspingController = controller;
            rb.useGravity = false;
            ctx.Send(new Messages.GraspUpdate(true).Serialize());
        }

        public void Release(Hand controller)
        {
            if (graspingController)
            {
                graspingController = null;
                rb.useGravity = true;
                ctx.Send(new Messages.GraspUpdate(false).Serialize());
            }
        }

        private void FixedUpdate()
        {
            if (graspingController != null)
            {
                // rb.velocity *= 0;
                rb.angularVelocity *= 0;
                rb.velocity = (graspingController.transform.position - transform.position) * 20;

                // transform.position = graspingController.transform.position;
            }
        }

        public void TakeControl()
        {
            if (!graspedRemotely)
            {
                owner = true;
                ctx.Send(new Messages.NewOwner().Serialize());
            }
        }

        public void SetKinematic(bool state)
        {
            if (owner)
            {
                rb.isKinematic = state;
                ctx.Send(new Messages.SetKinematic(state).Serialize());
            }
        }
    }
}
