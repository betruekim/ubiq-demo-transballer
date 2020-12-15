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

        float elapsed = 0f;
        public bool on = true;

        GameObject ui;

        override protected void Awake()
        {
            base.Awake();
            manager = GameObject.FindObjectOfType<Transballer.NetworkedPhysics.RigidbodyManager>();
            ui = transform.Find("ui").gameObject;
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
                }

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

        void OnUpdate(bool timerControl, bool manualOn, float on, float off)
        {
            this.timerControl = timerControl;
            this.onDuration = on;
            this.offDuration = off;
            this.on = manualOn;
            this.elapsed = 0;
            // set colors
        }

        protected override void OnHovered()
        {
            ui.SetActive(true);
            ui.GetComponent<ElectromagnetMenu>().onElectromagnetUpdate += OnUpdate;
        }

        protected override void OffHovered()
        {
            ui.GetComponent<ElectromagnetMenu>().onElectromagnetUpdate -= OnUpdate;
            ui.SetActive(false);
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