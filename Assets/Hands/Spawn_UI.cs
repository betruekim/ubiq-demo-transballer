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


        //leftHand.TriggerPress.AddListener(spawnSpawner);
    }

    void spawnSpawner(bool grasped)
    {
        menu.SetActive(grasped);

    }
    // Update is called once per frame
    void Update()
    {
        //0 70 -140
        Debug.Log("x" + leftHand.transform.rotation.eulerAngles.x);
        //Debug.Log("y" + leftHand.transform.rotation.y);
        //Debug.Log("z" + leftHand.transform.rotation.z);
        //Debug.Log("w" + leftHand.transform.rotation.w);


        if (leftHand.transform.rotation.x > 0.5)
        {
            Debug.Log("in method");
            menu.SetActive(true);

        }
        else
        {
            menu.SetActive(false);
        }
    }
}
