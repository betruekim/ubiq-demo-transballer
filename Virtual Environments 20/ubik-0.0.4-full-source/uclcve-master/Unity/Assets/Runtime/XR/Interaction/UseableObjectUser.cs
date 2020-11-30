using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ubik.XR
{
    public class UseableObjectUser : MonoBehaviour
    {
        public List<Collider> contacted;
        public List<IUseable> used;

        private HandController controller;

        private void Awake()
        {
            controller = GetComponent<HandController>();
            contacted = new List<Collider>();
            used = new List<IUseable>();
        }

        // Start is called before the first frame update
        void Start()
        {
            controller.TriggerPress.AddListener(Use);
        }

        void Use(bool state)
        {
            if(state)
            {
                foreach (var item in contacted)
                {
                    foreach (var component in item.GetComponentsInParent<MonoBehaviour>())
                    {
                        if(component is IUseable)
                        {
                            var useable = (component as IUseable);
                            useable.Use(controller);
                            used.Add(useable);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in used)
                {
                    item.UnUse(controller);
                }
                used.Clear();
            }
        }

        // *this* collider is the trigger
        private void OnTriggerEnter(Collider collider)
        {
            if(!contacted.Contains(collider))
            {
                contacted.Add(collider);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            contacted.Remove(collider);
        }
    }
}