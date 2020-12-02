using UnityEngine;
using System.Collections.Generic;

// singleton class that manages which rigidbodies get updated each frame
public class NetPhysManager : MonoBehaviour
{
    public class NetRBMetadata
    {
        public NetPhysRigidbody rigidbody;
        public float elapsed;
        public int priority;

        public NetRBMetadata(NetPhysRigidbody rb)
        {
            this.rigidbody = rb;
            this.elapsed = 0;
            this.priority = 0;
        }
    }

    List<NetRBMetadata> rigidbodies = new List<NetRBMetadata>();

    public void Register(NetPhysRigidbody rigidbody)
    {
        foreach (var meta in rigidbodies)
        {
            if (Object.ReferenceEquals(meta.rigidbody, rigidbody))
            {

                throw new System.Exception($"Err, tried to register rigidbody twice! Object {rigidbody.gameObject.name}");
            }
        }

        rigidbodies.Add(new NetRBMetadata(rigidbody));
        rigidbody.SendUpdate();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            if (rigidbodies[i].rigidbody.owner)
            {
                rigidbodies[i].elapsed += Time.fixedDeltaTime;
                rigidbodies[i].priority = Mathf.CeilToInt(rigidbodies[i].rigidbody.velSquareMag * rigidbodies[i].elapsed);
            }
            else
            {
                rigidbodies[i].priority = 0;
            }
        }
        rigidbodies.Sort((a, b) => a.priority - b.priority);
        for (int i = 0; i < Mathf.Min(10, rigidbodies.Count); i++)
        {
            if (rigidbodies[i].rigidbody.velSquareMag > 0)
            {
                rigidbodies[i].rigidbody.SendUpdate();
                rigidbodies[i].elapsed = 0;
            }
        }
    }
}