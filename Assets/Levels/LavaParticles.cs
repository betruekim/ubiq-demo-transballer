using UnityEngine;
using System.Collections;
using Transballer.NetworkedPhysics;

namespace Transballer.Levels
{
    public class LavaParticles : NetworkedObject
    {
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            base.Move();
        }

        override protected void Awake()
        {
            base.Awake();
            ParticleSystem.MainModule main = GetComponentInChildren<ParticleSystem>().main;
            StartCoroutine(RemoveDelayed(main.duration));
        }

        IEnumerator RemoveDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            base.Remove();
        }
    }
}