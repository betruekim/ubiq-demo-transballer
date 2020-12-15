using UnityEngine;

namespace Transballer.Levels
{
    public class LevelDoor : MonoBehaviour
    {
        private LevelLoader loaderScript;
        public int levelIndex = 0;

        void Awake()
        {
            loaderScript = GameObject.FindObjectOfType<LevelLoader>();
        }

        public void SetIndex(int index)
        {
            levelIndex = index;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Right Hand")
            {
                // Load in the level
                loaderScript.LoadLevelOwner(levelIndex);
            }
        }
    }
}