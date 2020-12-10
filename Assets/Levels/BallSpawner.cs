using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.Samples;
using Ubik.Messaging;
using Transballer.NetworkedPhysics;

namespace Transballer.Levels
{
    public class BallSpawner : MonoBehaviour, INetworkObject, INetworkComponent
    {
        public NetworkId Id { get; } = new NetworkId(11);
        NetworkContext ctx;

        public NetworkSpawner networkSpawner;
        public RigidbodyManager rigidbodyManager;

        public GameObject ball;
        public GameObject spawnPoint;
        private LevelManager levelManager;

        private void Awake()
        {
            networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
            levelManager = GameObject.FindObjectOfType<LevelManager>();
            ctx = NetworkScene.Register(this);
        }

        public void SpawnBalls()
        {
            float waitTime = 0;
            foreach (var burst in levelManager.level.emission)
            {
                StartCoroutine(SpawnBalls(burst.count, burst.duration, waitTime));
                waitTime += burst.duration;
            }
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

        private void spawnBall(int ballNumber)
        {
            float jitter = 0.1f;
            Vector3 offset = new Vector3(Random.Range(-jitter, jitter), 0.0f, Random.Range(-jitter, jitter));
            GameObject spawnedBall = networkSpawner.Spawn(ball);
            spawnedBall.transform.position = spawnPoint.transform.position + offset;

            // Add unique name for each ball
            spawnedBall.name = spawnedBall.name + ballNumber.ToString();

            // Add to the ball list
            levelManager.ballList.Add(spawnedBall.GetComponent<Ball>());
        }

        // private void displayTime(int seconds)
        // {
        //     transform.Find("Timer").GetComponent<TextMesh>().text = string.Format("{0}", seconds);
        // }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgString = message.ToString();
            string messageType = Transballer.Messages.GetType(msgString);

            if (messageType == "spawnerTime")
            {
                // displayTime(SpawnerTime.Deserialize(msgString).time);
            }
        }
    }

    [System.Serializable]
    public class SpawnerTime : Transballer.Messages.Message
    {
        public override string messageType => "spawnerTime";

        public int time;

        public SpawnerTime(int time)
        {
            this.time = time;
        }

        public override string Serialize()
        {
            return "spawnerTime$" + time.ToString() + "$";
        }

        public static SpawnerTime Deserialize(string message)
        {
            string[] components = message.Split('$');

            return new SpawnerTime(int.Parse(components[1]));
        }

    }
}