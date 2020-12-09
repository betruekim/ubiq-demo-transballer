using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.XR;
using Ubik.Samples;
using PlacableObjects;

public class UIManager : MonoBehaviour
{

    public GameObject menu;
    public GameObject spawnableButtonPrefab;
    public PrefabCatalogue placables;
    HandController leftHand;
    PlacementManager placementManager;
    Text material;

    void Start()
    {
        GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
        leftHand = playerObject.transform.Find("Left Hand").gameObject.GetComponent<HandController>();
        placementManager = GameObject.FindObjectOfType<PlacementManager>();
        placables = placementManager.placables;

        InitUI();
    }

    void InitUI()
    {
        menu = this.gameObject;
        GridLayoutGroup buttonsContainer = GetComponentInChildren<GridLayoutGroup>();
        int i = 0;
        foreach (GameObject placable in placables.prefabs)
        {
            GameObject button = GameObject.Instantiate(spawnableButtonPrefab, buttonsContainer.transform);
            button.GetComponentInChildren<Text>().text = $"{placable.name} {placable.GetComponent<Placable>().materialCost}";
            int localIndex = i;
            button.GetComponent<Button>().onClick.AddListener(delegate { placementManager.SelectObject(localIndex); });
            i++;
        }
        material = transform.Find("Canvas").Find("spawnablesMenu").Find("material").GetComponent<Text>();
        placementManager.onMaterialChange += UpdateMaterial;
    }

    void UpdateMaterial(int newMaterial, int newMaxMaterial)
    {
        material.text = $"{newMaterial}/{newMaxMaterial}";
    }

    void Update()
    {
        //0 70 -140
        menu.SetActive(leftHand.transform.rotation.x > 0.5);
    }
}
