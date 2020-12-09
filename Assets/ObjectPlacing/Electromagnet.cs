using UnityEngine;

namespace PlacableObjects
{
    public class Electromagnet : Placable
    {
        public override int materialCost => 10;
        public override bool canBePlacedFreely => false;
        Ubik.Physics.RigidbodyManager manager;

        public Transform attractionPoint;
        public float power = 10f;
        public const float radius = 3f;

        public float onDuration = 1f;
        public float offDuration = 1f;

        float elapsed = 0f;
        public bool on = true;

        override protected void Awake()
        {
            base.Awake();
            manager = GameObject.FindObjectOfType<Ubik.Physics.RigidbodyManager>();
        }

        private void FixedUpdate()
        {
            if (manager == null || !owner || !placed)
            {
                return;
            }
            elapsed += Time.fixedDeltaTime;
            if (on && elapsed > onDuration || !on && elapsed > offDuration)
            {
                elapsed = 0;
                on = !on;
            }

            if (on)
            {
                foreach (var meta in manager.rigidbodies)
                {
                    Vector3 force = attractionPoint.position - meta.rigidbody.transform.position;
                    if (force.sqrMagnitude < radius * radius)
                    {
                        if (meta.rigidbody.graspedRemotely || meta.rigidbody.graspingController)
                        {
                            continue;
                        }

                        if (!meta.rigidbody.owner)
                        {
                            meta.rigidbody.TakeControl();
                            continue;
                        }
                        force /= force.sqrMagnitude;
                        meta.rigidbody.rb.AddForce(force * power);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attractionPoint.position, radius);
        }
    }
}