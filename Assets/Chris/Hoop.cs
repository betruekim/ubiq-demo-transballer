using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour
{
    public GameObject ballDisplay;

    public bool collider1Hit;
    public bool collider2Hit;
    public bool collider3Hit;

    public int ballsToGo;
    public List<string> gate1 = new List<string>();

    public void levelComplete()
    {

    }
}
