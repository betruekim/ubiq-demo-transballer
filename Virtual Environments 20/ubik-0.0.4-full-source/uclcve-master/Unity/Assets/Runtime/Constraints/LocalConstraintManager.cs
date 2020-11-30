using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ubik.Constraints
{
    public abstract class ConstraintManager : MonoBehaviour // abstract class so we can use GetComponentInParent
    {
        public abstract void AddConstraint(SpringConstraint constraint);
        public abstract void DeleteConstraint(SpringConstraint constraint);

        public abstract bool CanCreateConstraint(Transform other);
    }

    /// <summary>
    /// Example implementation that applies rigidbody constraints to local rigid bodies.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class LocalConstraintManager : ConstraintManager
    {
        private Dictionary<SpringConstraint, SpringJoint> springs;
        private Rigidbody worldBody;

        private void Awake()
        {
            springs = new Dictionary<SpringConstraint, SpringJoint>();
            worldBody = GetComponent<Rigidbody>();
        }

        public override bool CanCreateConstraint(Transform remote) // whoever is calling this should verify the local end is supported...
        {
            return (transform.GetComponentInParent<Rigidbody>() != null);
        }

        public override void AddConstraint(SpringConstraint constraint)
        {
            if(!springs.ContainsKey(constraint))
            {
                var joint = worldBody.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                springs.Add(constraint, joint);
            }
        }

        public override void DeleteConstraint(SpringConstraint constraint)
        {
            Destroy(springs[constraint]);
            springs.Remove(constraint);
        }

        private void Update()
        {
            foreach (var item in springs)
            {
                var constraint = item.Key;
                var joint = item.Value;

                joint.anchor = constraint.localBody.TransformPoint(constraint.localAnchor);
                joint.spring = constraint.spring;
                joint.damper = constraint.damper;

                if (joint.connectedBody != constraint.remoteBody.GetComponentInParent<Rigidbody>())
                {
                    joint.connectedBody = constraint.remoteBody.GetComponentInParent<Rigidbody>();
                }

                joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(constraint.remoteBody.TransformPoint(constraint.remoteAnchor));

                // changing the spring joint local anchor position is not enough to wake up the connected body, so we must do it
                if (joint.connectedBody.IsSleeping())
                {
                    joint.connectedBody.WakeUp();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                foreach (var item in springs)
                {
                    item.Key.DrawGizmos();
                }
            }
        }
    }

}