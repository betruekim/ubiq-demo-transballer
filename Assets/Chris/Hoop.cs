using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Transballer.Levels
{
    public class Hoop : MonoBehaviour
    {
        public GameObject ballDisplay;
        public GameObject level;

        public bool collider1Hit;
        public bool collider2Hit;

        public int ballsToGo;
        public List<string> gate1 = new List<string>();

    }
}
