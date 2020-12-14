using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Samples;
using Transballer.NetworkedPhysics;

namespace Transballer.Levels
{
    [RequireComponent(typeof(BoxCollider))]
    public class Lava : MonoBehaviour
    {
        public GameObject lavaParticlesPrefab;
        private NetworkSpawner spawner;
        new private ParticleSystem particleSystem;
        private void Awake()
        {
            spawner = GameObject.FindObjectOfType<NetworkSpawner>();
            particleSystem = GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.scale = transform.lossyScale;
        }

        private void OnCollisionEnter(Collision other)
        {
            NetworkedRigidbody rb = other.gameObject.GetComponent<NetworkedRigidbody>();
            if (rb)
            {
                rb.Remove();
                GameObject particles = spawner.Spawn(lavaParticlesPrefab);
                particles.GetComponent<LavaParticles>().SetPosition(other.GetContact(0).point);
            }
        }
    }
}
