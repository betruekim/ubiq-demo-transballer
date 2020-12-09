using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.XR;
using Ubik.Samples;
using PlacableObjects;

public class UIManager : MonoBehaviour
{

    public GameObject rightHandMenu, leftHandMenu, rightGun, leftGun;
    public GameObject spawnableButtonPrefab;
    public PrefabCatalogue placables;
    HandController leftHand;
    HandController rightHand;
    PlacementManager placementManager;
    Text material;

    void Start()
    {
        GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
        leftHand = playerObject.transform.Find("Left Hand").gameObject.GetComponent<HandController>();
        rightHand = playerObject.transform.Find("Right Hand").gameObject.GetComponent<HandController>();
        placementManager = GameObject.FindObjectOfType<PlacementManager>();
        placables = placementManager.placables;

        InitUI();
    }

    void InitUI()
    {
        GridLayoutGroup buttonsContainer = rightHandMenu.GetComponentInChildren<GridLayoutGroup>();
        int i = 0;
        foreach (GameObject placable in placables.prefabs)
        {
            GameObject button = GameObject.Instantiate(spawnableButtonPrefab, buttonsContainer.transform);
            button.GetComponentInChildren<Text>().text = $"{placable.name} {placable.GetComponent<Placable>().materialCost}";
            int localIndex = i;
            button.GetComponent<Button>().onClick.AddListener(delegate { placementManager.SelectObject(localIndex); });
            i++;
        }
        material = rightHandMenu.transform.Find("Canvas").Find("spawnablesMenu").Find("material").GetComponent<Text>();
        placementManager.onMaterialChange += UpdateMaterial;
    }

    void UpdateMaterial(int newMaterial, int newMaxMaterial)
    {
        material.text = $"{newMaterial}/{newMaxMaterial}";
    }

    void Update()
    {
        //0 70 -140
        bool rightRotation = true;
        Debug.Log(rightHand.transform.eulerAngles);
        rightRotation = rightRotation && rightHand.transform.rotation.eulerAngles.y < 360 - 30 && rightHand.transform.rotation.eulerAngles.y > 360 - 120;
        rightRotation = rightRotation && rightHand.transform.rotation.eulerAngles.z > 60 && rightHand.transform.rotation.eulerAngles.z < 150;
        rightHandMenu.SetActive(rightEquipped && rightRotation);
    }

    [SerializeField]
    bool rightEquipped = false;

    public void HolsterTrigger(bool rightHolster, HandController hand)
    {
        Debug.Log($"{rightHolster} {hand}");
        if (rightHolster)
        {
            rightEquipped = !rightEquipped;
            rightGun.SetActive(rightEquipped);
        }
    }
}
