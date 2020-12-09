using UnityEngine;
using System.Collections.Generic;

namespace Transballer.NetworkedPhysics
{
    // singleton class that manages which rigidbodies get updated each frame
    public class RigidbodyManager : MonoBehaviour
    {
        const int syncsPerFixedUpdate = 50;


        [System.Serializable]
        public class NetRBMetadata
        {
            public NetworkedRigidbody rigidbody;
            public float elapsed;
            public int priority;

            public NetRBMetadata(NetworkedRigidbody rb)
            {
                this.rigidbody = rb;
                this.elapsed = 0;
                this.priority = 0;
            }
        }

        public List<NetRBMetadata> rigidbodies = new List<NetRBMetadata>();

        public void Register(NetworkedRigidbody rigidbody)
        {
            foreach (var meta in rigidbodies)
            {
                if (Object.ReferenceEquals(meta.rigidbody, rigidbody))
                {

                    throw new System.Exception($"Err, tried to register rigidbody twice! Object {rigidbody.gameObject.name}");
                }
            }

            rigidbodies.Add(new NetRBMetadata(rigidbody));
            if (rigidbody.owner)
            {
                rigidbody.SendUpdate();

            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                if (rigidbodies[i].rigidbody.owner)
                {
                    rigidbodies[i].priority = Mathf.CeilToInt((rigidbodies[i].rigidbody.velSquareMag + 1) * rigidbodies[i].elapsed);
                    if (rigidbodies[i].rigidbody.graspingController)
                    {
                        // aggressively update grasped items
                        rigidbodies[i].priority = int.MaxValue;
                    }
                }
                else
                {
                    rigidbodies[i].priority = 0;
                }
            }
            rigidbodies.Sort((a, b) => b.priority - a.priority);
            for (int i = 0; i < Mathf.Min(syncsPerFixedUpdate, rigidbodies.Count); i++)
            {
                if (rigidbodies[i].rigidbody.owner)
                {
                    rigidbodies[i].rigidbody.SendUpdate();
                    rigidbodies[i].elapsed = 0;
                }
            }
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                rigidbodies[i].elapsed += Time.fixedDeltaTime;
            }
        }
    }
}