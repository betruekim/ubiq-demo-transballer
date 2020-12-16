using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Samples;

namespace Transballer.NetworkedPhysics
{
    public class Table : NetworkedObject
    {
        public PrefabCatalogue interactables;
        [SerializeField]
        List<NetworkedRigidbody> spawned;


        public void SpawnInteractables()
        {
            StartCoroutine(SpawnInteractablesIE());
        }

        IEnumerator SpawnInteractablesIE()
        {
            while (!owner)
            {
                // remember owner is set after OnSpawned is called, so this will only run after onspawned
                // also this is only ever called by owner, so no infinite waiting
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("ON SPAWNED TABLE");
            // spawn extras
            NetworkSpawner spawner = GameObject.FindObjectOfType<NetworkSpawner>();
            spawned = new List<NetworkedRigidbody>();
            for (int i = 0; i < NetworkManager.peers.Count + 1; i++)
            {
                foreach (var prefab in interactables.prefabs)
                {
                    if (prefab.name == "Ball")
                    {
                        continue;
                    }
                    Vector3 pos = transform.position + Vector3.up + Random.insideUnitSphere * 0.05f;
                    GameObject spawnedPrefab = spawner.Spawn(prefab);
                    spawnedPrefab.transform.position = pos;
                    spawnedPrefab.transform.rotation = Random.rotation;
                    NetworkedRigidbody networkedRigidbody = spawnedPrefab.GetComponent<NetworkedRigidbody>();
                    networkedRigidbody.Move();
                    spawned.Add(networkedRigidbody);
                }
            }
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            foreach (var item in spawned)
            {
                item.Remove();
            }
        }
    }
}
