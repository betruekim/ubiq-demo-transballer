using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;

namespace Transballer.Levels
{
    public class PhysicalButton : MonoBehaviour, IUseable
    {

        Vector3 targetPosition = Vector3.up * 0.5f;

        void IUseable.Use(Hand controller)
        {
            GameObject.FindObjectOfType<BallSpawner>()?.SpawnBalls();
            targetPosition = Vector3.up * 0.451f;
        }

        void IUseable.UnUse(Hand controller)
        {
            targetPosition = Vector3.up * 0.5f;
        }

        private void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 0.4f);
        }
    }
}

