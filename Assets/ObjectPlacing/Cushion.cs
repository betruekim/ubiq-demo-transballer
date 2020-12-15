using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Transballer.PlaceableObjects
{

    public class Cushion : Static
    {
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ball"))
            {
                HintManager.SetComplete(HintManager.cushionUsed, true);
            }
        }
    }
}