using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;

public class NetPhysRigidbody : MonoBehaviour, INetworkObject, INetworkComponent, ISpawnable
{
    public NetworkId Id { get; } = new NetworkId();

    public Rigidbody rb;
    public float velSquareMag { get { return rb.velocity.sqrMagnitude; } }

    NetworkContext ctx;
    NetPhysManager manager;
    public bool owner = true;

    private void Awake()
    {
        ctx = NetworkScene.Register(this);
        manager = GameObject.FindObjectOfType<NetPhysManager>();
        rb = GetComponent<Rigidbody>();
    }

    public struct RigidbodyUpdateMessage
    {
        public Vector3 position;
        public Quaternion rotation;

        public Vector3 linearVelocity;
        public Vector3 angularVelocity;

        public RigidbodyUpdateMessage(Transform transform, Rigidbody rigidbody)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;

            this.linearVelocity = rigidbody.velocity;
            this.angularVelocity = rigidbody.angularVelocity;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var decoded = message.FromJson<RigidbodyUpdateMessage>();

        transform.position = decoded.position;
        transform.rotation = decoded.rotation;

        rb.velocity = decoded.linearVelocity;
        rb.angularVelocity = decoded.angularVelocity;
    }

    public void SendUpdate()
    {
        ctx.SendJson(new RigidbodyUpdateMessage(transform, rb));
    }

    public void OnSpawned(bool local)
    {
        owner = local;
        manager.Register(this);
    }

    float elapsed = 0;

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
