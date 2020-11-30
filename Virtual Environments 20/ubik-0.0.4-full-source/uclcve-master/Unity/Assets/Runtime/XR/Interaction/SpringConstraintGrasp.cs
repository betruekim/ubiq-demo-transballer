using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Constraints;

namespace Ubik.XR
{
    //apply this to the object with the constraints
    public class SpringConstraintGrasp : MonoBehaviour
    {
        public float Spring = 25f;
        public float Damper = 0.5f;

        private ConstraintManager manager;

        private Transform grasped;
        private Collider contacted;
        private SpringConstraint[] constraints;
        private Vector3[] offsets;
        private Vector3[] localAttachmentPoints;

        private void Awake()
        {
            offsets = new Vector3[3];
            offsets[0] = Vector3.zero;
            localAttachmentPoints = new Vector3[3];

            manager = GetComponentInParent<ConstraintManager>();

            constraints = new SpringConstraint[3];
            for (int i = 0; i < 3; i++)
            {
                constraints[i] = new SpringConstraint();
            }

            var controller = GetComponent<HandController>();
            if(controller)
            {
                controller.GripPress.AddListener(Grasp);
            }
        }

        public void Grasp(bool grasp)
        {
            if(grasp)
            {
                if(contacted != null)
                {
                    grasped = contacted.transform;

                    var attachmentPoint = contacted.ClosestPoint(transform.position);

                    offsets[1] = -transform.up * 0.2f + transform.right * 0.2f;
                    offsets[2] = -transform.up * 0.2f - transform.right * 0.2f;

                    for (int i = 0; i < 3; i++)
                    {
                        localAttachmentPoints[i] = (attachmentPoint + offsets[i]);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        constraints[i].localAnchor = transform.InverseTransformPoint(localAttachmentPoints[i]);
                        constraints[i].localBody = transform;
                        constraints[i].remoteAnchor = contacted.transform.InverseTransformPoint(localAttachmentPoints[i]);
                        constraints[i].remoteBody = contacted.transform;
                        manager.AddConstraint(constraints[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    manager.DeleteConstraint(constraints[i]);
                }

                grasped = null;
            }
        }

        private void Update()
        {
            if(grasped != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    constraints[i].spring = Spring;
                    constraints[i].damper = Damper;
                }
            }
        }

        // *this* collider is the trigger
        private void OnTriggerEnter(Collider collider)
        {
            if(manager.CanCreateConstraint(collider.transform))
            {
                contacted = collider;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if(contacted == collider)
            {
                contacted = null;
            }
        }
    }
}