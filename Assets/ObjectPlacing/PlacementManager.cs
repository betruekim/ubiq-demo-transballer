using UnityEngine;
using Ubik.Samples;
using Ubik.XR;

namespace PlacableObjects
{
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
                    if (controller.TriggerPress != null)
                    {
                        controller.TriggerPress.AddListener((bool pressed) => { if (pressed) { PlaceObject(); } });
                    }
                    if (controller.PrimaryButtonPress != null)
                    {
                        controller.PrimaryButtonPress.AddListener((bool pressed) => { if (pressed) { if (selectedObject >= 0) { DeselectObject(); } else { SelectObject(0); } } });
                    }
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
        Placable ghostObject = null;
        Snap[] ghostSnaps = null;
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
                DeselectObject();
            }
            customRotation = Quaternion.identity;
            placeDist = 1f;
            SpawnGhostObject();
        }

        private void SpawnGhostObject()
        {
            Debug.Log("SpawnGhostObject");
            // ghostObject = GameObject.Instantiate(objects[selectedObject]);
            ghostObject = networkSpawner.Spawn(objects[selectedObject]).GetComponent<Placable>();
            // Material transparentMat = new Material(Shader.Find("Transparent/Diffuse"));
            // transparentMat.color = new Color(transparentMat.color.r, transparentMat.color.g, transparentMat.color.b, 0.5f);
            // foreach (MeshRenderer mr in ghostObject.GetComponentsInChildren<MeshRenderer>())
            // {
            //     mr.material = transparentMat;
            // }
            foreach (Collider col in ghostObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
                col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            MoveGhostToHandPos();
        }

        private void Update()
        {
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
        const float maxRaycastDist = 2f;

        private void MoveGhostToHandPos()
        {
            ghostObject.transform.position = rightHand.transform.position + rightHand.transform.forward * placeDist;
            ghostObject.transform.rotation = Quaternion.Euler(0, rightHand.transform.rotation.eulerAngles.y, 0) * customRotation;
        }

        private void DoRaycast()
        {
            if (cachedHit)
            {
                return;
            }
            // first move the object to where it would be just using hands
            MoveGhostToHandPos();
            if (ghostSnaps != null)
            {
                // check rays from each snap
                for (int i = 0; i < ghostSnaps.Length; i++)
                {
                    Ray ray = new Ray(ghostSnaps[i].transform.position - ghostSnaps[i].transform.right * 0.1f, ghostSnaps[i].transform.right);
                    Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 0.25f, LayerMask.GetMask("Snap"));
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider.gameObject != ghostSnaps[i].gameObject)
                        {
                            cachedHit = hit.transform.gameObject;
                            return;
                        }
                    }
                }
            }
            cachedHit = null;
        }

        private void MoveGhostToSnapPos()
        {
            DoRaycast();
            if (cachedHit)
            {
                ghostObject.transform.position = cachedHit.transform.position - cachedHit.transform.rotation * snapAs[selectedObject].Item1;
                ghostObject.transform.rotation = cachedHit.transform.rotation * snapAs[selectedObject].Item2;
            }
            ghostObject.Move();
        }

        private void FixedUpdate()
        {
            cachedHit = null;
            if (ghostObject)
            {
                MoveGhostToSnapPos();
            }
        }

        public void DeselectObject()
        {
            // call this to empty hand
            selectedObject = -1;
            if (ghostObject)
            {
                ghostObject.Deselect();
                ghostSnaps = null;
            }
        }

        public void PlaceObject()
        {
            if (selectedObject >= 0)
            {
                Debug.Log("placing the ting");
                // GameObject placed = networkSpawner.Spawn(objects[selectedObject]);
                // placed.transform.position = ghostObject.transform.position;
                // placed.transform.rotation = ghostObject.transform.rotation;
                ghostObject.Place();
                selectedObject = -1;
                ghostObject = null;
            }
        }

    }
}