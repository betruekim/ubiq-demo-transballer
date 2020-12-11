using UnityEngine;
using System.Collections.Generic;

namespace Transballer.Levels
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "LevelSO", order = 1)]
    public class Level : ScriptableObject
    {
        public GameObject prefab;
        public int allowedMaterial = 0;
        [System.Serializable]
        public struct EmissionBurst
        {
            public int count;
            public float duration;
        }
        [SerializeField]
        public List<EmissionBurst> emission = new List<EmissionBurst>();
    }
}