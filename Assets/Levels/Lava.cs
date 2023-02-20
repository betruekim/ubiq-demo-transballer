using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Transballer.NetworkedPhysics;
using Ubiq.Spawning;

namespace Transballer.Levels
{
    [RequireComponent(typeof(BoxCollider))]
    public class Lava : MonoBehaviour
    {
        public GameObject lavaParticlesPrefab;
        private NetworkSpawnManager spawner;
        new private ParticleSystem particleSystem;
        private void Awake()
        {
            spawner = GameObject.FindObjectOfType<NetworkSpawnManager>();
            particleSystem = GetComponentInChildren<ParticleSystem>();
            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.scale = transform.lossyScale;
        }

        private void OnCollisionEnter(Collision other)
        {
            NetworkedRigidbody rb = other.gameObject.GetComponent<NetworkedRigidbody>();
            if (rb)
            {
                if (rb.owner)
                {
                    rb.Remove();
                    // GameObject particles = spawner.Spawn(lavaParticlesPrefab);
                    // particles.GetComponent<LavaParticles>().SetPosition(other.GetContact(0).point);
                }
            }
        }
    }
}
