using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.XR;
using Ubik.Samples;
using Transballer.PlaceableObjects;

public class UIManager : MonoBehaviour
{

    public GameObject buildMenu, otherMenu, rightGun, leftGun;
    public GameObject spawnableButtonPrefab;
    public PrefabCatalogue placeables;
    HandController leftHand;
    HandController rightHand;
    PlacementManager placementManager;
    Text material;
    Transform mainCamera;

    void Start()
    {
        GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
        leftHand = playerObject.transform.Find("Left Hand").gameObject.GetComponent<HandController>();
        rightHand = playerObject.transform.Find("Right Hand").gameObject.GetComponent<HandController>();
        leftHand.gameObject.layer = LayerMask.NameToLayer("Hands");
        rightHand.gameObject.layer = LayerMask.NameToLayer("Hands");
        placementManager = GameObject.FindObjectOfType<PlacementManager>();
        placeables = placementManager.placeables;
        mainCamera = Camera.main.transform;

        InitUI();
    }

    void InitUI()
    {
        leftGun = leftHand.transform.Find("deag").gameObject;
        rightGun = rightHand.transform.Find("deag").gameObject;
        buildMenu = leftHand.transform.Find("buildMenu").gameObject;
        otherMenu = null;

        GridLayoutGroup buttonsContainer = buildMenu.GetComponentInChildren<GridLayoutGroup>();
        int i = 0;
        foreach (GameObject placeable in placeables.prefabs)
        {
            GameObject button = GameObject.Instantiate(spawnableButtonPrefab, buttonsContainer.transform);
            button.GetComponentInChildren<Text>().text = $"{placeable.name} {placeable.GetComponent<Placeable>().materialCost}";
            int localIndex = i;
            button.GetComponent<Button>().onClick.AddListener(delegate { placementManager.SelectObject(localIndex); });
            i++;
        }
        material = buildMenu.transform.Find("Canvas").Find("spawnablesMenu").Find("material").GetComponent<Text>();
        placementManager.onMaterialChange += UpdateMaterial;
    }

    void UpdateMaterial(int newMaterial, int newMaxMaterial)
    {
        material.text = $"{newMaterial}/{newMaxMaterial}";
    }


    bool HandWithinRange(HandController hand, float yMin, float yMax, float zMin, float zMax)
    {
        float y = hand.transform.localRotation.eulerAngles.y - mainCamera.localRotation.eulerAngles.y;
        y = (y + 360) % 360;
        float z = hand.transform.localRotation.eulerAngles.z;
        return y > yMin && y < yMax && z > zMin && z < zMax;
    }

    void Update()
    {
        //0 70 -140
        // otherMenu.SetActive(rightEquipped && HandWithinRange(rightHand, 360 - 120, 360 - 30, 60, 150));
        // buildMenu.SetActive(rightEquipped && HandWithinRange(leftHand, 30, 120, 360 - 150, 360 - 60));
    }

    [SerializeField]
    bool rightEquipped = false;
    [SerializeField]
    bool leftEquipped = false;

    public void HolsterTrigger(bool rightHolster, HandController hand)
    {
        Debug.Log($"{rightHolster} {hand}");
        if (rightHolster)
        {
            rightEquipped = !rightEquipped;
            rightGun.SetActive(rightEquipped);
            buildMenu.SetActive(rightEquipped);
        }
        else
        {
            leftEquipped = !leftEquipped;
            leftGun.SetActive(leftEquipped);
        }
    }
}
