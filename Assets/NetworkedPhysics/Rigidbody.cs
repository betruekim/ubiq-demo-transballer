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
                        throw new System.Exception("received update for locally controlled gameobject");
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

        private void Update()
        {
            if (graspingController != null)
            {
                rb.velocity *= 0;
                rb.angularVelocity *= 0;

                transform.position = graspingController.transform.position;
            }
        }

        // float elapsed = 0;

        // private void FixedUpdate()
        // {
        //     if (owner)
        //     {
        //         elapsed += Time.fixedDeltaTime;
        //         // if (elapsed > 0.1f)
        //         // {
        //         elapsed = 0;
        //         SendUpdate();
        //         // }
        //     }
        // }
    }
}
