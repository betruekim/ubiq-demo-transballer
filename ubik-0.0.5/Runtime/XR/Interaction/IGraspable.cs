using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ubik.XR
{
    public interface IGraspable
    {
        void Grasp(Hand controller);
        void Release(Hand controller);
    }


}
