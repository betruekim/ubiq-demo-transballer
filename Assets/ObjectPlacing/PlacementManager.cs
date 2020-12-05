using UnityEngine;
using Ubik.Samples;
using Ubik.XR;

public class PlacementManager : MonoBehaviour
{
    NetworkSpawner networkSpawner;
    HandController rightHand;
    HandController leftHand;

    private void Awake()
    {
        networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
        HandController[] handControllers = GameObject.FindObjectsOfType<HandController>();
        foreach (var controller in handControllers)
        {
            if (controller.gameObject.name == "Right Hand")
            // if (controller.Right)
            {
                rightHand = controller;
                controller.TriggerPress.AddListener((bool pressed) => { if (pressed) { PlaceObject(); } });
                controller.PrimaryButtonPress.AddListener((bool pressed) => { if (pressed) { if (selectedObject >= 0) { DeselectObject(); } else { SelectObject(0); } } });
            }
            else
            if (controller.gameObject.name == "Left Hand")
            // if (controller.Left)
            {
                leftHand = controller;
            }
        }
    }

    public PrefabCatalogue placables;
    GameObject[] objects { get => placables.prefabs.ToArray(); }
    (Vector3, Quaternion)[] snapAs; // position and rotational offset of object from snapA
    (Vector3, Quaternion)[] snapBs; // position and rotational offset of object from snapB

    private void Start()
    {
        snapAs = new (Vector3, Quaternion)[objects.Length];
        snapBs = new (Vector3, Quaternion)[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            snapAs[i] = (objects[i].transform.GetChild(1).localPosition, objects[i].transform.GetChild(1).localRotation);
            snapBs[i] = (objects[i].transform.GetChild(2).localPosition, objects[i].transform.GetChild(2).localRotation);
        }
    }

    public int selectedObject { get; private set; } = -1;
    GameObject ghostObject = null;
    float placeDist = 1f;
    const float minPlaceDist = 0.2f;
    const float maxPlaceDist = 5f;
    Quaternion customRotation = Quaternion.identity;

    public void SelectObject(int index)
    {
        if (index < 0 || index >= objects.Length)
        {
            throw new System.Exception($"index {index} less than zero or greater than objects length ${objects.Length}");
        }
        selectedObject = index;

        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }
        customRotation = Quaternion.identity;
        placeDist = 1f;
        SpawnGhostObject();
    }

    private void SpawnGhostObject()
    {
        ghostObject = GameObject.Instantiate(objects[selectedObject], GetObjectPosition(), GetObjectRotation());
        Material transparentMat = new Material(Shader.Find("Transparent/Diffuse"));
        transparentMat.color = new Color(transparentMat.color.r, transparentMat.color.g, transparentMat.color.b, 0.5f);
        foreach (MeshRenderer mr in ghostObject.GetComponentsInChildren<MeshRenderer>())
        {
            mr.material = transparentMat;
        }
        foreach (Collider col in ghostObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }

    private void Update()
    {
        raycastedThisFrame = false;
        if (ghostObject)
        {
            ghostObject.transform.position = GetObjectPosition();
            ghostObject.transform.rotation = GetObjectRotation();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedObject < 0)
            {
                SelectObject(0);
            }
            else
            {
                DeselectObject();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
        if (rightHand.GripState)
        {
            placeDist += leftHand.Joystick.y * Time.deltaTime;
            placeDist = Mathf.Clamp(placeDist, minPlaceDist, maxPlaceDist);
        }
        if (leftHand.GripState)
        {
            customRotation *= Quaternion.Euler(0, leftHand.Joystick.x, leftHand.Joystick.y);
        }
    }

    private GameObject cachedHit;
    private bool raycastedThisFrame = false;
    const float maxRaycastDist = 2f;

    private void DoRaycast()
    {
        if (raycastedThisFrame)
        {
            return;
        }

        Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxRaycastDist, LayerMask.GetMask("Snap")))
        {
            cachedHit = hit.transform.gameObject;
        }
        else
        {
            cachedHit = null;
        }
    }

    private Vector3 GetObjectPosition()
    {
        DoRaycast();
        if (cachedHit)
        {
            // assume we are snapping snapA
            return cachedHit.transform.position - cachedHit.transform.rotation * snapAs[selectedObject].Item1;
        }

        return rightHand.transform.position + rightHand.transform.forward * placeDist;
    }

    private Quaternion GetObjectRotation()
    {
        DoRaycast();
        if (cachedHit)
        {
            return cachedHit.transform.rotation * snapAs[selectedObject].Item2;
        }
        return Quaternion.Euler(0, rightHand.transform.rotation.eulerAngles.y, 0) * customRotation;
    }

    public void DeselectObject()
    {
        // call this to empty hand
        selectedObject = -1;
        if (ghostObject)
        {
            Destroy(ghostObject);
        }
    }

    public void PlaceObject()
    {
        if (selectedObject >= 0)
        {
            GameObject placed = networkSpawner.Spawn(objects[selectedObject]);
            placed.transform.position = GetObjectPosition();
            placed.transform.rotation = GetObjectRotation();
            Destroy(ghostObject);
        }
    }

}