using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.Samples;
using Ubik.Messaging;
using Transballer.NetworkedPhysics;

public class BallSpawner : MonoBehaviour, INetworkObject, INetworkComponent
{
    public NetworkId Id { get; } = new NetworkId(11);
    NetworkContext ctx;

    public GameObject level;
    public NetworkSpawner networkSpawner;
    public RigidbodyManager rigidbodyManager;
    public GameObject timerText;
    public float timeUntilSpawn;
    public int ballsToSpawn;
    public GameObject ball;
    public GameObject spawnPoint;
    public float spawnRate;
    private LevelManager levelManager;

    public bool timerRunning = false;
    private Transform spawnTransform;

    private void Awake()
    {
        networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
        levelManager = level.GetComponent<LevelManager>();
        ctx = NetworkScene.Register(this);
    }

    void Start()
    {
        GameObject empty = new GameObject();
        spawnTransform = empty.transform;
        spawnTransform.position = spawnPoint.transform.position;
    }

    void Update()
    {
        if (timerRunning)
        {
            if (timeUntilSpawn > 0.0f)
            {
                timeUntilSpawn -= Time.deltaTime;

                int seconds;
                if (timeUntilSpawn < 0.0f)
                {
                    seconds = 0;
                }
                else
                {
                    seconds = Mathf.FloorToInt(timeUntilSpawn);
                }
                ctx.Send(new SpawnerTime(seconds).Serialize());
                displayTime(seconds);
            }
            else
            {
                timeUntilSpawn = 0.0f;
                Debug.Log("Time is up. Balls imminent.");
                timerRunning = false;

                // Start spawning balls
                StartCoroutine("spawnBalls");
            }
        }
    }

    IEnumerator spawnBalls()
    {
        while (ballsToSpawn != 0)
        {
            spawnBall(ballsToSpawn);
            ballsToSpawn -= 1;
            yield return new WaitForSeconds(1.0f / spawnRate);
        }

        yield break;
    }

    private void spawnBall(int ballNumber)
    {
        float jitter = 0.1f;
        Vector3 offset = new Vector3(Random.Range(-jitter, jitter), 0.0f, Random.Range(-jitter, jitter));
        GameObject spawnedBall = networkSpawner.Spawn(ball);
        spawnedBall.transform.position = spawnTransform.position + offset;

        // Add unique name for each ball
        spawnedBall.name = spawnedBall.name + ballNumber.ToString();

        // Add to the ball list
        levelManager.ballList.Add(spawnedBall);
    }

    private void displayTime(int seconds)
    {
        transform.Find("Timer").GetComponent<TextMesh>().text = string.Format("{0}", seconds);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        string msgString = message.ToString();
        string messageType = Transballer.Messages.GetType(msgString);

        if (messageType == "spawnerTime")
        {
            displayTime(SpawnerTime.Deserialize(msgString).time);
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