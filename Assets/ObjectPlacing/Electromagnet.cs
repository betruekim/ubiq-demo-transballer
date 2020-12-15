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

        float elapsed = 0f;
        public bool on = true;

        GameObject ui;
        GameObject[] coils;

        override protected void Awake()
        {
            base.Awake();
            manager = GameObject.FindObjectOfType<Transballer.NetworkedPhysics.RigidbodyManager>();
            ui = transform.Find("ui").gameObject;

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
            SetCoilEffects(false);
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
                    SetCoilEffects(on);
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

        void SetCoilEffects(bool on)
        {
            foreach (var coil in coils)
            {
                if (on)
                {
                    coil.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    coil.GetComponent<ParticleSystem>().Stop();
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
            SetCoilEffects(manualOn);
        }

        void SendUpdate(bool timerControl, bool manualOn, float on, float off)
        {
            ctx.Send(new ElectromagnetUpdate(timerControl, manualOn, on, off).Serialize());
            OnUpdate(timerControl, manualOn, on, off);
        }

        public override void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string messageType = Messages.GetType(message.ToString());
            switch (messageType)
            {
                case "electromagnetUpdate":
                    ElectromagnetUpdate update = ElectromagnetUpdate.Deserialize(message.ToString());
                    OnUpdate(update.timerControl, update.manualOn, update.onDuration, update.offDuration);
                    break;
                default:
                    base.ProcessMessage(message);
                    break;

            }
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

        [System.Serializable]
        public class ElectromagnetUpdate : Messages.Message
        {
            public override string messageType => "electromagnetUpdate";
            public bool timerControl;
            public bool manualOn;
            public float onDuration;
            public float offDuration;

            public ElectromagnetUpdate(bool timerControl, bool manualOn, float on, float off)
            {
                this.timerControl = timerControl;
                this.manualOn = manualOn;
                this.onDuration = on;
                this.offDuration = off;
            }

            public override string Serialize()
            {
                return $"{messageType}${timerControl}${manualOn}${onDuration}${offDuration}";
            }

            public static ElectromagnetUpdate Deserialize(string message)
            {
                string[] components = message.Split('$');
                return new ElectromagnetUpdate(bool.Parse(components[1]), bool.Parse(components[2]), float.Parse(components[3]), float.Parse(components[4]));
            }
        }
    }
}