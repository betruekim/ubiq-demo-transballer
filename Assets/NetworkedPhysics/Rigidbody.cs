using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;
using Ubik.XR;
using OdinSerializer;


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
            Debug.Log(message);

            Messages.Message typeCheck = SerializationUtility.DeserializeValue<Messages.Message>(message.bytes, DataFormat.JSON);
            if (typeCheck == null)
            {
                throw new System.Exception("received empty/non-deserializable message!");
            }
            string messageType = typeCheck.messageType;

            switch (messageType)
            {
                case "rigidbodyUpdate":
                    if (owner)
                    {
                        throw new System.Exception("received update for locally controlled gameobject");
                    }
                    Messages.RigidbodyUpdate rbUpdate = SerializationUtility.DeserializeValue<Messages.RigidbodyUpdate>(message.bytes, DataFormat.JSON);
                    transform.position = rbUpdate.position;
                    transform.rotation = rbUpdate.rotation;
                    rb.velocity = rbUpdate.linearVelocity;
                    rb.angularVelocity = rbUpdate.angularVelocity;
                    break;
                case "onGrasp":
                    Debug.Log("received onGrasp notification, turning off owner");
                    owner = false;
                    Messages.OnGrasp graspUpdate = SerializationUtility.DeserializeValue<Messages.OnGrasp>(message.bytes, DataFormat.JSON);
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
            ctx.Send(new Messages.OnGrasp().Serialize());
        }

        public void Release(Hand controller)
        {
            graspingController = null;
            rb.useGravity = true;
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
