﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using UnityEngine;

namespace Transballer.Levels
{
    public class LevelLoader : MonoBehaviour
    {
        NetworkSpawnManager networkSpawner;
        NetworkContext ctx;
        public NetworkId NetworkId { get; } = new NetworkId(10);

        public PrefabCatalogue levels;
        public GameObject doorPrefab;
        public PrefabCatalogue interactables;
        public GameObject ui;

        GameObject environment;

        Transform doorsParent;
        GameObject[] doors;

        LevelManager currentLevel;

        void Awake()
        {
            environment = GameObject.Find("Environment");
        }

        private void Start()
        {
            ctx = NetworkScene.Register(this);
            RoomClient.Find(this).OnJoinedRoom.AddListener(SpawnDoors);
            networkSpawner = NetworkSpawnManager.Find(this);
            networkSpawner.OnSpawned.AddListener(OnSpawned);
        }

        void OnSpawned(GameObject g, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
        {
            var lm = g.GetComponentInChildren<LevelManager>();
            if(lm) // We have spawned a level
            {
                LevelSpawned(lm);
            }
        }

        void SpawnDoors(IRoom room)
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

                foreach (var id in PlaceableObjects.PlaceableIndex.placedObjects.Keys.ToArray())
                {
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
                    networkSpawner.SpawnWithRoomScope(levels.prefabs[levelIndex]);
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

            if (NetworkManager.roomOwner && currentLevel.name == "levelSandbox")
            {
                interactablesTable = networkSpawner.SpawnWithPeerScope(NestedPrefabCatalogue.GetPrefabFromName("interactablesTable")).GetComponent<Transballer.NetworkedPhysics.Table>();
                GameObject tableSpawnPoint = GameObject.Find("tableSpawnPoint");
                if (tableSpawnPoint)
                {
                    interactablesTable.transform.position = tableSpawnPoint.transform.position;
                    interactablesTable.transform.rotation = tableSpawnPoint.transform.rotation;
                    interactablesTable.Move();
                }
                interactablesTable.SpawnInteractables();
            }
        }

        private void movePlayer()
        {
            GameObject player = GameObject.Find("Player");
            int myPlayerIndex = GameObject.FindObjectOfType<NetworkManager>().GetMyPlayerIndex();
            if (currentLevel)
            {
                GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
                player.transform.position = spawnPoints[myPlayerIndex % spawnPoints.Length].transform.position;
                player.transform.rotation = spawnPoints[myPlayerIndex % spawnPoints.Length].transform.rotation;
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

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
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
