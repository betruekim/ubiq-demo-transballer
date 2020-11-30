using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ubik.Constraints
{
    public class SpringConstraint
    {
        /// <summary>
        /// Anchor point relative to this Component's Transform
        /// </summary>
        public Vector3 localAnchor;

        /// <summary>
        /// Anchor point relative to the connected Component's RigidBody
        /// </summary>
        public Vector3 remoteAnchor;

        /// <summary>
        /// The first rigidbody connected by the constraint
        /// </summary>
        public Transform localBody;

        /// <summary>
        /// The other rigidbody connected by the constraint
        /// </summary>
        public Transform remoteBody;

        public float spring;
        public float damper;

        public void DrawGizmos()
        {
            if (localBody == null)
            {
                return;
            }

            if (remoteBody == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(localBody.TransformPoint(localAnchor), remoteBody.TransformPoint(remoteAnchor));
        }
    }
}