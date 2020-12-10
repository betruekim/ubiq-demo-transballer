using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Samples;

namespace Transballer.Levels
{
    public class LevelLoader : MonoBehaviour
    {
        public NetworkSpawner networkSpawner;
        public PrefabCatalogue levels;
        public GameObject levelSelect;

        private bool waitingForLoad = false;

        void Awake()
        {
            networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
        }

        void Update()
        {
            if (waitingForLoad)
            {
                loadLevel();
            }
        }

        public void loadLevelOwner(int levelIndex)
        {
            // Disable level selector
            levelSelect.SetActive(false);

            // Disable UI
            GameObject ui = GameObject.Find("UI");
            ui.SetActive(false);

            // Load in the prefab
            GameObject spawnedLevel = networkSpawner.Spawn(levels.prefabs[levelIndex]);

            // Give the spawned level references so it can reenable at the end
            spawnedLevel.GetComponent<LevelManager>().levelSelect = levelSelect;
            spawnedLevel.GetComponent<LevelManager>().ui = ui;

            // Move the player
            movePlayer();

        }

        public void loadLevel()
        {
            waitingForLoad = true;

            if (GameObject.FindObjectOfType<LevelManager>() != null)
            {
                // Disable level selector
                levelSelect.SetActive(false);

                // Disable UI
                GameObject ui = GameObject.Find("UI");
                ui.SetActive(false);

                // Give the spawned level references so it can reenable at the end
                // Hold on while until spawns in? Hacky workaround
                while (GameObject.FindObjectOfType<LevelManager>() == null) { }
                GameObject.FindObjectOfType<LevelManager>().levelSelect = levelSelect;
                GameObject.FindObjectOfType<LevelManager>().ui = ui;

                // Move the player to his spot
                movePlayer();
                waitingForLoad = false;
            }

        }

        private void movePlayer()
        {
            GameObject playerPosition = GameObject.Find("PlayerPosition");
            GameObject player = GameObject.Find("Player");
            player.transform.position = playerPosition.transform.position;
            player.transform.rotation = playerPosition.transform.rotation;
        }
    }
}
