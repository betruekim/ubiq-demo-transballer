using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;

namespace Transballer.Levels
{
    public class PhysicalButton : MonoBehaviour, IUseable
    {
        public Vector3 offset;
        Vector3 targetPosition;
        Vector3 startPosition;
        public UnityEngine.Events.UnityEvent onButtonDown;

        private void Awake()
        {
            startPosition = transform.localPosition;
            targetPosition = startPosition;
        }

        void IUseable.Use(Hand controller)
        {
            onButtonDown.Invoke();
            targetPosition = startPosition + offset;
        }

        void IUseable.UnUse(Hand controller)
        {
            targetPosition = startPosition;
        }

        private void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 0.4f);
        }
    }
}

