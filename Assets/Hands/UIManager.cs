using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ubik.XR;
using Ubik.Samples;
using Transballer.PlaceableObjects;

public class UIManager : MonoBehaviour
{

    GameObject buildMenu, gun;
    LineRenderer gunLine;
    ParticleSystem gunParticles;
    Transform barrelExit;
    public GameObject spawnableButtonPrefab;
    GameObject[] buttons;

    public PrefabCatalogue placeables;

    HandController leftHand;
    HandController rightHand;

    PlacementManager placementManager;
    Text material;
    Transform mainCamera;
    public Sprite removeIcon;

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
        gun = rightHand.transform.Find("deag").gameObject;
        gunLine = gun.GetComponent<LineRenderer>();
        gunLine.positionCount = 0;
        gunParticles = gun.GetComponentInChildren<ParticleSystem>();
        barrelExit = gun.transform.Find("suppressorPos_3");
        buildMenu = leftHand.transform.Find("buildMenu").gameObject;

        buttons = new GameObject[placeables.prefabs.Count + 1];
        InitBuildMenu();

        placementManager.onMaterialChange += UpdateMaterial;
        placementManager.onEquippedChange += SelectItem;
        placementManager.onPlaced += PlacedObject;
    }

    void InitBuildMenu()
    {
        GridLayoutGroup buttonsContainer = buildMenu.GetComponentInChildren<GridLayoutGroup>();
        int i = 0;
        foreach (GameObject placeable in placeables.prefabs)
        {
            GameObject button = GameObject.Instantiate(spawnableButtonPrefab, buttonsContainer.transform);
            button.GetComponentInChildren<Text>().text = $"{placeable.name} {placeable.GetComponent<Placeable>().materialCost}";
            int localIndex = i;
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                placementManager.SelectObject(localIndex);
                EventSystem.current?.SetSelectedGameObject(null);
            });
            buttons[i] = button;
            i++;
        }

        GameObject removeButton = GameObject.Instantiate(spawnableButtonPrefab, buttonsContainer.transform);
        removeButton.GetComponentInChildren<Text>().text = $"remove";
        removeButton.GetComponentsInChildren<Image>()[1].sprite = removeIcon;
        removeButton.GetComponent<Button>().onClick.AddListener(delegate { placementManager.SelectRemover(); EventSystem.current.SetSelectedGameObject(null); });
        buttons[i] = removeButton;

        material = buildMenu.transform.Find("Canvas").Find("spawnablesMenu").Find("material").GetComponent<Text>();
    }

    void UpdateMaterial(int newMaterial, int newMaxMaterial)
    {
        material.text = $"{newMaterial}/{newMaxMaterial}";
    }

    public enum SelectedHand { none, left, right };

    [SerializeField]
    public static SelectedHand equipped = SelectedHand.none;

    public void HolsterTrigger(HandController hand)
    {
        if (!NetworkManager.connected)
        {
            return;
        }
        if (hand.Right)
        {
            if (equipped != SelectedHand.right)
            {
                equipped = SelectedHand.right;
            }
            else
            {
                equipped = SelectedHand.none;
            }

            gun.SetActive(equipped != SelectedHand.none);
            buildMenu.SetActive(equipped != SelectedHand.none);

            if (equipped == SelectedHand.right)
            {
                gun.transform.SetParent(rightHand.transform);
                gun.transform.localPosition = Vector3.down * 0.02f;
                gun.transform.localRotation = Quaternion.Euler(0, 180, 0);

                buildMenu.transform.SetParent(leftHand.transform);
                buildMenu.transform.localPosition = new Vector3(-0.128f, -0.136f, 0.008f);
                buildMenu.transform.localRotation = Quaternion.Euler(18.994f, 88.983f, 171.066f);
            }
        }
        else
        {
            if (equipped != SelectedHand.left)
            {
                equipped = SelectedHand.left;
            }
            else
            {
                equipped = SelectedHand.none;
            }

            gun.SetActive(equipped != SelectedHand.none);
            buildMenu.SetActive(equipped != SelectedHand.none);

            if (equipped == SelectedHand.left)
            {
                gun.transform.SetParent(leftHand.transform);
                gun.transform.localPosition = Vector3.down * 0.02f;
                gun.transform.localRotation = Quaternion.Euler(0, 180, 0);

                buildMenu.transform.SetParent(rightHand.transform);
                buildMenu.transform.localPosition = new Vector3(0.128f, -0.136f, -0.008f);
                buildMenu.transform.localRotation = Quaternion.Euler(18.994f, 180 + 88.983f, 171.066f);
            }
        }
        placementManager.DeselectObject();
    }

    void SelectItem(int index)
    {
        // Debug.Log($"UIManager selectItem called {index}");
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().color = Color.white;
            if (i == index || (index == -2 && i == buttons.Length - 1))
            {
                buttons[i].GetComponent<Image>().color = Color.green;
            }
        }
    }

    void PlacedObject(int type, Placeable target)
    {
        StartCoroutine(PlacedObjectCoroutine(type, target));
    }

    IEnumerator PlacedObjectCoroutine(int type, Placeable target)
    {
        RaycastHit hit;
        Vector3 pos = barrelExit.position - gun.transform.forward * 10f;
        if (Physics.Raycast(barrelExit.position, -gun.transform.forward, out hit, 100f))
        {
            pos = hit.point;
        }
        if (type == -2)
        {
            // removed
            gunLine.positionCount = 2;
            gunLine.SetPosition(0, barrelExit.position);
            gunLine.SetPosition(1, pos);
            gunLine.startColor = Color.red;
            gunLine.endColor = Color.red;
            ParticleSystem.MainModule main = gunParticles.main;
            main.startColor = Color.red;
            gunParticles.Play();

            yield return new WaitForSeconds(0.1f);
            gunLine.positionCount = 0;
        }
        else if (type >= 0)
        {
            // placed
            gunLine.positionCount = 2;
            gunLine.SetPosition(0, barrelExit.position);
            gunLine.SetPosition(1, pos);
            gunLine.startColor = Color.green;
            gunLine.endColor = Color.green;
            ParticleSystem.MainModule main = gunParticles.main;
            main.startColor = Color.green;
            gunParticles.Play();

            yield return new WaitForSeconds(0.1f);
            gunLine.positionCount = 0;
        }
        else if (type == -1)
        {
            // dud
            // ParticleSystem.MainModule main = gunParticles.main;
            // main.startColor = Color.blue;
            // gunParticles.Play();
        }
        yield return new WaitForEndOfFrame();
    }
}
