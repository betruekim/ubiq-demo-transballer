using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Transballer.Levels
{
    public class Hoop : MonoBehaviour
    {
        public int index;

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void Complete()
        {
            GameObject.FindObjectOfType<LevelManager>().HoopComplete(index);
        }

        public GameObject ballDisplay;

        public bool collider1Hit;
        public bool collider2Hit;

        public int ballsToGo;
        public List<string> gate1 = new List<string>();

    }
}
