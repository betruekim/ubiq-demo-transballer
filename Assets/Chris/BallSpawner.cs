using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.Samples;

public class BallSpawner : MonoBehaviour
{
    public NetworkSpawner networkSpawner;
    public GameObject timerText;
    public float timeUntilSpawn;
    public int ballsToSpawn;
    public GameObject ball;
    public GameObject spawnPoint;
    public Level level;
    public float spawnRate;

    private bool timerRunning = false;
    private Transform spawnTransform;

    private void Awake()
    {
        networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
        level = Resources.Load<Level>("Levels/ExampleLevel");
    }

    void Start()
    {
        GameObject empty = new GameObject();
        spawnTransform = empty.transform;
        spawnTransform.position = spawnPoint.transform.position;

        timerRunning = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine("spawnBalls");
        }

        // if (timerRunning)
        // {
        //     if (timeUntilSpawn > 0.0f)
        //     {
        //         timeUntilSpawn -= Time.deltaTime;
        //         displayTime(timeUntilSpawn);
        //     }
        //     else
        //     {
        //         timeUntilSpawn = 0.0f;
        //         Debug.Log("Time is up. Balls imminent.");
        //         timerRunning = false;

        //         // Start spawning balls
        //         StartCoroutine("spawnBalls");
        //     }
        // }
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
        GameObject spawnedBall = networkSpawner.SpawnPersistent(ball);
        spawnedBall.transform.position = spawnTransform.position + offset;

        // Add unique name for each ball
        spawnedBall.name = spawnedBall.name + ballNumber.ToString();
    }

    private void displayTime(float timeUntilSpawn)
    {
        int seconds;
        if (timeUntilSpawn < 0.0f)
        {
            seconds = 0;
        }
        else
        {
            seconds = Mathf.FloorToInt(timeUntilSpawn);
        }

        transform.Find("Timer").GetComponent<TextMesh>().text = string.Format("{0}", seconds);
    }
}
