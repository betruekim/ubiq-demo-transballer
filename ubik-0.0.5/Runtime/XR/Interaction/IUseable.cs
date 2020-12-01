using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ubik.XR
{
    public interface IUseable
    {
        void Use(Hand controller);
        void UnUse(Hand controller);
    }
}
