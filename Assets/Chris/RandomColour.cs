using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RandomColour : MonoBehaviour
{
    [SerializeField] public Renderer ball;
    private Color[] colours = new Color[] { Color.green, Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow };

    void Start()
    {
        Random rand = new Random();
        ball.material.color = colours[rand.Next(colours.Length)];
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision with trigger: " + other.gameObject.name);
    }

}