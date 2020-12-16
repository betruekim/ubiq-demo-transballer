using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Samples;
using Ubik.Messaging;
using Ubik.Rooms;

namespace Transballer.Levels
{
    public class LevelLoader : MonoBehaviour, INetworkComponent, INetworkObject
    {
        NetworkSpawner networkSpawner;
        NetworkContext ctx;
        NetworkId INetworkObject.Id { get; } = new NetworkId(10);

        public PrefabCatalogue levels;
        public GameObject doorPrefab;
        public PrefabCatalogue interactables;

        GameObject ui;
        GameObject environment;

        Transform doorsParent;
        GameObject[] doors;

        LevelManager currentLevel;


        void Awake()
        {
            ctx = NetworkScene.Register(this);

            networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
            ui = GameObject.Find("UI");
            environment = GameObject.Find("environment");

            GameObject.FindObjectOfType<RoomClient>().OnRoom.AddListener(SpawnDoors);
        }

        void SpawnDoors()
        {
            if (doorsParent != null)
            {
                doorsParent.gameObject.SetActive(true);
                return;
            }
            doors = new GameObject[levels.prefabs.Count];
            doorsParent = new GameObject("doors").transform;

            float radius = 5f;
            for (int i = 0; i < doors.Length; i++)
            {
                // placing doors in a circle around 0,0
                float angle = (float)i / (float)doors.Length * Mathf.PI * 2;
                Vector3 pos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;

                doors[i] = SpawnDoor(i, pos, Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0), doorsParent);
            }
        }

        GameObject SpawnDoor(int index, Vector3 pos, Quaternion rotation, Transform parent)
        {
            GameObject newDoor = GameObject.Instantiate(doorPrefab, pos, rotation, parent);
            newDoor.GetComponentInChildren<LevelDoor>().SetIndex(index);
            newDoor.GetComponentInChildren<TextMesh>().text = $"Level {index + 1}";
            return newDoor;
        }

        GameObject nextLevelDoor;

        public void SpawnNextLevelDoor(int index, Vector3 pos, Quaternion rot)
        {
            nextLevelDoor = SpawnDoor(index, pos, rot, transform);
        }

        Transballer.NetworkedPhysics.Table interactablesTable;

        public void LoadLevelOwner(int levelIndex)
        {
            if (NetworkManager.connected && NetworkManager.roomOwner)
            {
                // clean up spawned stuff
                if (currentLevel)
                {
                    while (currentLevel.ballList.Count > 0)
                    {
                        var ball = currentLevel.ballList[0];
                        ball.Remove();
                        currentLevel.ballList.RemoveAt(0);
                    }
                }
                var ids = new List<int>(PlaceableObjects.PlaceableIndex.placedObjects.Keys);
                foreach (var id in ids)
                {
                    // TODO THIS STILL BREAKS CONTINUITY MAYBE?
                    PlaceableObjects.PlaceableIndex.placedObjects[id].RemoveSudo();
                }

                if (interactablesTable)
                {
                    interactablesTable.Remove();
                }

                if (levelIndex == -1)
                {
                    ctx.Send(new BackToLevelSelect().Serialize());
                    StartCoroutine(OnBackToLevelSelect());
                    return;
                }
                else if (levelIndex >= 0 && levelIndex < levels.prefabs.Count)
                {
                    networkSpawner.Spawn(levels.prefabs[levelIndex]);
                    ctx.Send(new OnLoad().Serialize());
                    OnLoadLevel();
                }
            }
        }

        private void OnLoadLevel()
        {
            environment.SetActive(false);
            ui.SetActive(false);
            doorsParent.gameObject.SetActive(false);
            if (nextLevelDoor)
            {
                Destroy(nextLevelDoor);
            }
        }

        // this is called by levelmanager in OnSpawned
        public void LevelSpawned(LevelManager levelManager)
        {
            if (currentLevel)
            {
                Destroy(currentLevel.gameObject);
            }
            currentLevel = levelManager;
            movePlayer();

            UIManager UIManager = GameObject.FindObjectOfType<UIManager>();
            foreach (GameObject g in UIManager.buttons)
            {
                if (g.name == "remove")
                {
                    g.SetActive(true);
                    continue;
                }
                if (!levelManager.allowedPlaceables.Contains(g.name) && levelManager.allowedPlaceables.Count > 0)
                {
                    g.SetActive(false);
                }
                else if (levelManager.disallowedPlaceables.Contains(g.name) && levelManager.disallowedPlaceables.Count > 0)
                {
                    g.SetActive(false);
                }
                else
                {
                    g.SetActive(true);
                }
            }

            NetworkManager.inLevel = true;

            GameObject.FindObjectOfType<PlaceableObjects.PlacementManager>().SetMaxMaterial(Mathf.FloorToInt(levelManager.allowedMaterial / (NetworkManager.peers.Keys.Count + 1)));

            if (NetworkManager.roomOwner)
            {
                interactablesTable = networkSpawner.Spawn(NestedPrefabCatalogue.GetPrefabFromName("interactablesTable")).GetComponent<Transballer.NetworkedPhysics.Table>();
            }
        }

        private void movePlayer()
        {
            GameObject player = GameObject.Find("Player");
            if (currentLevel)
            {
                Transform playerPosition = currentLevel.transform.Find("spawnPoint");
                player.transform.position = playerPosition.transform.position;
                player.transform.rotation = playerPosition.transform.rotation;
            }
            else
            {
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;
            }
        }

        IEnumerator OnBackToLevelSelect()
        {
            environment.SetActive(true);
            ui.SetActive(true);
            doorsParent.gameObject.SetActive(true);

            if (currentLevel)
            {
                Destroy(currentLevel.gameObject);
                while (GameObject.FindObjectOfType<LevelManager>())
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            NetworkManager.inLevel = false;
            movePlayer();
        }

        void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgType = Messages.GetType(message.ToString());
            switch (msgType)
            {
                case "levelLoad":
                    OnLoadLevel();
                    break;
                case "levelBack":
                    StartCoroutine(OnBackToLevelSelect());
                    break;
                default:
                    throw new System.Exception($"unknown message type {msgType}");
            }
        }

        [System.Serializable]
        public class OnLoad : Transballer.Messages.Message
        {
            public override string messageType => "levelLoad";
            public override string Serialize()
            {
                return "levelLoad$";
            }
        }
        [System.Serializable]
        public class BackToLevelSelect : Transballer.Messages.Message
        {
            public override string messageType => "levelBack";
            public override string Serialize()
            {
                return "levelBack$";
            }
        }
    }
}
