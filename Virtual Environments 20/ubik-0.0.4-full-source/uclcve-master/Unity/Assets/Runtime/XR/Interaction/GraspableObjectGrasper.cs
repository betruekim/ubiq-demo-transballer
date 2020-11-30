using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ubik.XR
{
    /// <summary>
    /// This interacts with Components that implement IGraspable, when its trigger collider enters their collider.
    /// </summary>
    public class GraspableObjectGrasper : MonoBehaviour
    {
        private Collider contacted;
        private IGraspable grasped;
        private HandController controller;

        private void Awake()
        {
            controller = GetComponent<HandController>();
        }

        // Start is called before the first frame update
        void Start()
        {
            controller.GripPress.AddListener(Grasp);
        }

        public void Grasp(bool grasp)
        {
            if (grasp)
            {
                if (contacted != null)
                {
                    // parent because physical bodies consist of a rigid body, and colliders *below* it in the scene graph
                    grasped = contacted.gameObject.GetComponentsInParent<MonoBehaviour>().Where(mb => mb is IGraspable).FirstOrDefault() as IGraspable;
                    grasped.Grasp(controller);
                }
            }
            else
            {
                if (grasped != null)
                {
                    grasped.Release(controller);
                    grasped = null;
                }
            }
        }

        // *this* collider is the trigger
        private void OnTriggerEnter(Collider collider)
        {
            if(collider.gameObject.GetComponentsInParent<MonoBehaviour>().Where(mb => mb is IGraspable).FirstOrDefault() != null)
            {
                contacted = collider;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (contacted == collider)
            {
                contacted = null;
            }
        }
    }
}