using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;


public class Spawn_UI : MonoBehaviour
{

    public GameObject menu;
    HandController leftHand;


    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
        leftHand = playerObject.transform.Find("Left Hand").gameObject.GetComponent<HandController>();

        leftHand.TriggerPress.AddListener(spawnSpawner);
    }

    void spawnSpawner(bool grasped)
    {
        menu.SetActive(grasped);

    }
    // Update is called once per frame
    void Update()
    {

    }
}
