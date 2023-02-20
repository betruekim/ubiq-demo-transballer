using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Transballer.NetworkedPhysics;
using Ubiq.Rooms;

namespace Transballer.Levels
{
    public class BallSpawner : MonoBehaviour
    {
        public GameObject ball;
        public GameObject spawnPoint;
        private LevelManager levelManager;
        private NetworkSpawnManager spawner;
        private RoomClient client;

        [SerializeField]
        List<LevelManager.EmissionBurst> emissions;

        private void Start()
        {
            levelManager = GameObject.FindObjectOfType<LevelManager>();
            spawner = NetworkSpawnManager.Find(this);
            client = RoomClient.Find(this);
            spawner.OnSpawned.AddListener(OnSpawned);
        }

        public void SetEmissions(List<LevelManager.EmissionBurst> emissions)
        {
            Debug.Log($"emissions set {emissions.Count}");
            this.emissions = emissions;
        }

        public void SpawnBalls()
        {
            Debug.Log("spawnBalls called");
            float waitTime = 0;
            foreach (var burst in emissions)
            {
                StartCoroutine(SpawnBalls(burst.count, burst.duration, waitTime));
                waitTime += burst.duration;
            }
        }

        public void StopSpawning()
        {
            StopAllCoroutines();
        }

        private IEnumerator SpawnBalls(int count, float duration, float delay)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < count; i++)
            {
                spawnBall(i);
                yield return new WaitForSeconds(duration / (float)count);
            }
            yield break;
        }

        private void OnSpawned(GameObject g, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
        {
            if(g.GetComponent<Ball>() is Ball b)
            {
                b.OnSpawned(peer == client.Me);
            }
        }

        private void spawnBall(int ballNumber)
        {
            float jitter = 0.1f;
            Vector3 offset = new Vector3(Random.Range(-jitter, jitter), 0.0f, Random.Range(-jitter, jitter));
            var spawnedBall = spawner.SpawnWithPeerScope(ball);

            spawnedBall.transform.position = spawnPoint.transform.position + offset;

            // Add unique name for each ball
            spawnedBall.name = spawnedBall.name + ballNumber.ToString();

            // Add to the ball list
            levelManager.ballList.Add(spawnedBall.GetComponent<Ball>());
        }
    }
}