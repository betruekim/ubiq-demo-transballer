using Ubik.Messaging;
using UnityEngine;

namespace Transballer.PlaceableObjects
{
    public class Electromagnet : Placeable
    {
        public override int materialCost => 10;
        public override bool canBePlacedFreely => false;
        Transballer.NetworkedPhysics.RigidbodyManager manager;

        public Transform attractionPoint;
        public float power = 10f;
        public const float radius = 3f;

        public bool timerControl = true;
        public float onDuration = 1f;
        public float offDuration = 1f;
        public bool stayOn = false;
        public bool on = true;

        float elapsed = 0f;

        GameObject[] coils;

        override protected void Awake()
        {
            base.Awake();
            manager = GameObject.FindObjectOfType<Transballer.NetworkedPhysics.RigidbodyManager>();

            coils = new GameObject[3];
            int i = 0;
            foreach (Transform t in transform.Find("model").GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name.StartsWith("Torus"))
                {
                    coils[i] = t.gameObject;
                    i++;
                }
            }
            SetCoilEffects();
        }

        protected override void InitUI()
        {
            base.InitUI();
            ui.placeable = this;
            ui.placeableType = typeof(Electromagnet);
            ui.AddBoolean("timerControl");
            ui.AddBoolean("stayOn");
            ui.AddFloat("offDuration", 0, 10);
            ui.AddFloat("onDuration", 0, 10);
            ui.GenerateUI();
            ui.OnUiUpdate += SetCoilEffects;
        }

        private void FixedUpdate()
        {
            if (manager == null || !owner || !placed)
            {
                return;
            }
            if (timerControl)
            {
                elapsed += Time.fixedDeltaTime;
                if (on && elapsed > onDuration || !on && elapsed > offDuration)
                {
                    elapsed = 0;
                    on = !on;
                    SetCoilEffects();
                }

            }
            if (on || stayOn)
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

        void SetCoilEffects()
        {
            foreach (var coil in coils)
            {
                if (on || stayOn)
                {
                    coil.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    coil.GetComponent<ParticleSystem>().Stop();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attractionPoint.position, radius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attractionPoint.position, 0.1f);
        }
    }
}