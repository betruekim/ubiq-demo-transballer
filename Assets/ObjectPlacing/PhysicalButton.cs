using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;

namespace Transballer.Levels
{
    public class PhysicalButton : MonoBehaviour, IUseable
    {

        void IUseable.Use(Hand controller)
        {
            GameObject.FindObjectOfType<BallSpawner>()?.SpawnBalls();
        }

        void IUseable.UnUse(Hand controller)
        {
            throw new System.NotImplementedException();
        }
    }
}

