using System;
using UnityEngine;
using Ubik.Samples;
using Ubik.XR;

namespace Transballer.PlaceableObjects
{
    public class PlacementManager : MonoBehaviour
    {
        NetworkSpawner networkSpawner;
        public HandController rightHand;
        public HandController leftHand;

        public int maxMaterial;
        public int material;

        public delegate void MaterialUpdate(int newMaterial, int newMaxMaterial);
        public event MaterialUpdate onMaterialChange;

        public delegate void EquippedUpdate(int newSlot); // called when a new item is selected
        public event EquippedUpdate onEquippedChange;

        public delegate void PlaceUpdate(int type, Placeable target); // -2 removed, -1 nada, 0-count object
        public event PlaceUpdate onPlaced;

        public bool useSnaps = true;

        private void Start()
        {
            networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
            HandController[] handControllers = GameObject.FindObjectsOfType<HandController>();

            foreach (var controller in handControllers)
            {
                Debug.Log(controller.PrimaryButtonPress);
                if (controller.gameObject.name == "Right Hand")
                // if (controller.Right)
                {
                    rightHand = controller;
                    if (controller.TriggerPress != null)
                    {
                        controller.TriggerPress.AddListener((bool pressed) =>
                        {
                            if (pressed)
                            {
                                if (UIManager.equipped == UIManager.SelectedHand.right) { PlaceObject(); }
                            }
                        });
                    }
                    if (controller.GripPress != null)
                    {
                        controller.GripPress.AddListener((bool pressed) =>
                        {
                            if (pressed)
                            {
                                if (UIManager.equipped == UIManager.SelectedHand.right) { DeselectObject(); }
                            }
                        });
                    }
                    if (controller.PrimaryButtonPress != null)
                    {
                        controller.PrimaryButtonPress.AddListener((bool pressed) =>
                        {
                            if (pressed)
                            {
                                if (UIManager.equipped == UIManager.SelectedHand.left) { SwitchSnaps(); }
                            }
                        });
                        // controller.PrimaryButtonPress.AddListener((bool pressed) => { if (pressed) { if (selectedObject >= 0) { DeselectObject(); } else { SelectObject(0); } } });
                    }
                }
                else
                if (controller.gameObject.name == "Left Hand")
                // if (controller.Left)
                {
                    leftHand = controller;
                    if (controller.TriggerPress != null)
                    {
                        controller.TriggerPress.AddListener((bool pressed) =>
                        {
                            if (pressed)
                            {
                                if (UIManager.equipped == UIManager.SelectedHand.left) { PlaceObject(); }
                            }
                        });
                    }
                    if (controller.GripPress != null)
                    {
                        controller.GripPress.AddListener((bool pressed) =>
                        {
                            if (pressed)
                            {
                                if (UIManager.equipped == UIManager.SelectedHand.left) { DeselectObject(); }
                            }
                        });
                    }
                    if (controller.PrimaryButtonPress != null)
                    {
                        controller.PrimaryButtonPress.AddListener((bool pressed) =>
                        {
                            if (pressed)
                            {
                                if (UIManager.equipped == UIManager.SelectedHand.right) { SwitchSnaps(); }
                            }
                        });
                        // controller.PrimaryButtonPress.AddListener((bool pressed) => { if (pressed) { if (selectedObject >= 0) { DeselectObject(); } else { SelectObject(0); } } });
                    }
                }
            }
        }

        private void SwitchSnaps()
        {
            useSnaps = !useSnaps;
            if (ghostObject)
            {
                foreach (Placeable placeable in PlaceableIndex.placedObjects.Values)
                {
                    foreach (Snap s in placeable.snaps)
                    {
                        if (useSnaps)
                        {
                            if (ghostObject.CanBePlacedOn(s))
                            {
                                s.ShowGraphic();
                            }
                        }
                        else
                        {
                            s.HideGraphic();
                        }
                    }
                }
            }
        }

        public void SetMaxMaterial(int maxMaterial)
        {
            this.maxMaterial = maxMaterial;
            material = maxMaterial;
            // ? is syntactic sugar for 'run this only if the object is not null'
            onMaterialChange?.Invoke(maxMaterial, maxMaterial);
        }

        public PrefabCatalogue placeables;
        GameObject[] objects { get => placeables.prefabs.ToArray(); }
        public int selectedObject { get; private set; } = -1;
        Placeable ghostObject = null;
        float placeDist = 1f;
        const float minPlaceDist = 0.2f;
        const float maxPlaceDist = 2f;
        Vector3 horizAngle, vertAngle = Vector3.zero;
        Vector3 lastHorizAngle = Vector3.zero;
        Vector2 startAngle = Vector2.zero;
        Quaternion customRotation = Quaternion.identity;
        bool removing = false;

        public void SelectObject(int index)
        {
            Debug.Log(Environment.StackTrace);
            SetMaxMaterial(400); // TODO REMOVE THIS
            if (index < 0 || index >= objects.Length)
            {
                throw new System.Exception($"index {index} less than zero or greater than objects length ${objects.Length}");
            }

            if (ghostObject != null)
            {
                DeselectObject();
            }
            selectedObject = index;
            removing = false;
            customRotation = Quaternion.identity;
            horizAngle = Vector3.zero;
            lastHorizAngle = Vector3.zero;
            vertAngle = Vector3.zero;
            placeDist = 1f;
            SpawnGhostObject();
            onEquippedChange?.Invoke(index);
            if (useSnaps)
            {
                foreach (Placeable placeable in PlaceableIndex.placedObjects.Values)
                {
                    foreach (Snap s in placeable.snaps)
                    {
                        if (ghostObject.CanBePlacedOn(s))
                        {
                            s.ShowGraphic();
                        }
                    }
                }
            }
        }

        public void SelectRemover()
        {
            DeselectObject();
            removing = true;
            onEquippedChange?.Invoke(-2);
        }

        private void SpawnGhostObject()
        {
            Debug.Log("SpawnGhostObject");
            ghostObject = networkSpawner.Spawn(objects[selectedObject]).GetComponent<Placeable>();
            MoveGhostToHandPos();
        }

        private void Update()
        {
            // for (int i = 0; i < objects.Length; i++)
            // {
            //     if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            //     {
            //         if (selectedObject != i)
            //         {
            //             SelectObject(i);
            //         }
            //         else
            //         {
            //             DeselectObject();
            //         }
            //     }
            // }
            // if (Input.GetMouseButtonDown(0))
            // {
            //     PlaceObject();
            // }

            HandController alternateHand = UIManager.equipped == UIManager.SelectedHand.left ? rightHand : leftHand;
            if (alternateHand)
            {
                if (alternateHand.GripState)
                {
                    if (hitElapsed > 0.3f)
                    {
                        Debug.Log("snap angle changing");
                        // if we have been snapped to something for longer than a second
                        snapAngle += alternateHand.Joystick.x + alternateHand.Joystick.y;
                    }
                    else
                    {
                        if (Mathf.Abs(alternateHand.Joystick.y) > 0.3f && Mathf.Abs(alternateHand.Joystick.y) > Mathf.Abs(alternateHand.Joystick.x))
                        {
                            placeDist += alternateHand.Joystick.y * Time.deltaTime * 2f;
                            placeDist = Mathf.Clamp(placeDist, minPlaceDist, maxPlaceDist);
                        }
                        else
                        {
                            vertAngle += Vector3.up * alternateHand.Joystick.x;
                        }
                    }
                }
                else if (alternateHand.TriggerState)
                {
                    // https://answers.unity.com/questions/1259992/rotate-object-towards-joystick-input-using-c.html
                    float ang = Mathf.Atan2(startAngle.y, startAngle.x);
                    if (Mathf.Max(Mathf.Abs(alternateHand.Joystick.x), Mathf.Abs(alternateHand.Joystick.y)) > 0.7f)
                    {
                        Vector3 next = lastHorizAngle + (Mathf.Atan2(alternateHand.Joystick.normalized.y, alternateHand.Joystick.normalized.x) - ang) * 180 / Mathf.PI * Vector3.forward;
                        // horizAngle = Vector3.Lerp(horizAngle, next, 0.2f);
                        horizAngle = next;
                    }
                    if (startAngle.sqrMagnitude < 0.01f && alternateHand.Joystick.sqrMagnitude > 0.25f)
                    {
                        // we just flicked to the side
                        startAngle = alternateHand.Joystick.normalized;
                    }
                    else if (alternateHand.Joystick.sqrMagnitude < 0.25f && startAngle.sqrMagnitude > 0.01f)
                    {
                        // we just flicked back to zero
                        startAngle = Vector2.zero;
                        lastHorizAngle = horizAngle;
                    }
                }
                else
                {
                    startAngle = Vector2.zero;
                }
            }
            customRotation = Quaternion.Euler(horizAngle + vertAngle);
        }

        private Snap snapHit; // the thing we hit using snap raycasts
        private int snapIndex = -1; // the index of the snap object on ghostObject
        private float hitElapsed;
        private float snapAngle;
        const float maxRaycastDist = 2f;
        bool canBePlaced = false;

        private void MoveGhostToHandPos()
        {
            HandController hand = UIManager.equipped == UIManager.SelectedHand.left ? leftHand : rightHand;
            ghostObject.transform.position = hand.transform.position + hand.transform.forward * placeDist;
            ghostObject.transform.rotation = Quaternion.Euler(0, hand.transform.rotation.eulerAngles.y, 0) * customRotation;
        }

        private void DoRaycast()
        {
            // if (snapHit)
            // {
            //     return;
            // }
            // first move the object to where it would be just using hands
            MoveGhostToHandPos();
            // check rays from each snap
            for (int i = 0; i < ghostObject.snaps.Length; i++)
            {
                Ray ray = new Ray(ghostObject.snaps[i].transform.position - ghostObject.snaps[i].transform.forward * 0.1f, ghostObject.snaps[i].transform.forward);
                Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);
                RaycastHit[] hits = Physics.RaycastAll(ray, 1f, LayerMask.GetMask("Snap"));
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject != ghostObject.snaps[i].gameObject)
                    {
                        Snap newHit = hit.transform.gameObject.GetComponent<Snap>();
                        if (snapHit != newHit)
                        {
                            snapHit = newHit;
                            snapIndex = i;
                            hitElapsed = 0;
                            snapAngle = 0;
                        }
                        else
                        {
                            hitElapsed += Time.fixedDeltaTime;
                        }
                        return;
                    }
                }
            }
            snapHit = null;
            snapIndex = -1;
            hitElapsed = 0;
            snapAngle = 0;
        }

        private void MoveGhostToSnapPos()
        {
            DoRaycast();
            if (snapHit)
            {
                if (ghostObject.CanBePlacedOn(snapHit))
                {
                    ghostObject.transform.rotation = Snap.GetMatchingRotation(snapHit, ghostObject.snaps[snapIndex]);
                    ghostObject.transform.position = Snap.GetMatchingPosition(snapHit, ghostObject.snaps[snapIndex]);
                    Snap.SetExtraRotation(snapHit, ghostObject.snaps[snapIndex], snapAngle);
                }
            }
        }

        Placeable hovered = null;

        private void FixedUpdate()
        {
            // don't need to cache anymore since we raycast once
            // snapHit = null;
            if (ghostObject)
            {
                if (useSnaps)
                {
                    MoveGhostToSnapPos();
                }
                else
                {
                    MoveGhostToHandPos();
                }
                ghostObject.Move();
                // compute canBePlaced
                canBePlaced = ghostObject.materialCost <= material;
                if (snapHit)
                {
                    canBePlaced = canBePlaced && ghostObject.CanBePlacedOn(snapHit);
                }
                else
                {
                    canBePlaced = canBePlaced && ghostObject.canBePlacedFreely;
                }
                if (canBePlaced)
                {
                    ghostObject.SetMeshColors(Color.green);
                }
                else
                {
                    ghostObject.SetMeshColors(Color.red);
                }
            }
            else
            {
                Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
                Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f, 1 << LayerMask.NameToLayer("Placeable")))
                {
                    Placeable placeableHovered = hit.collider.gameObject.GetComponentInParent<Placeable>();
                    if (placeableHovered != hovered)
                    {
                        if (hovered)
                        {
                            hovered.OffHovered();
                        }
                        placeableHovered.OnHovered();
                        hovered = placeableHovered;
                    }
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        RemoveObject(placeableHovered);
                    }
                }
                else
                {
                    if (hovered)
                    {
                        hovered.OffHovered();
                        hovered = null;
                    }
                }
            }

        }

        public void DeselectObject()
        {
            // call this to empty hand
            selectedObject = -1;
            if (ghostObject)
            {
                ghostObject.Remove();
            }
            foreach (Placeable placeable in PlaceableIndex.placedObjects.Values)
            {
                foreach (Snap s in placeable.snaps)
                {
                    s.HideGraphic();
                }
            }

            onEquippedChange?.Invoke(-1);
        }

        public void PlaceObject()
        {
            // TODO: check if we are aiming at a canvas
            if (selectedObject >= 0 && ghostObject && canBePlaced)
            {
                if (ghostObject.materialCost <= material)
                {
                    if (snapHit)
                    {
                        ghostObject.Place(snapIndex, snapHit.placeable.Id, snapHit.index);
                    }
                    else
                    {
                        ghostObject.Place();
                    }
                    material -= ghostObject.materialCost;
                    onMaterialChange?.Invoke(material, maxMaterial);
                    onPlaced?.Invoke(selectedObject, ghostObject);
                    ghostObject = null;
                    DeselectObject();
                }
            }
            else if (removing && hovered)
            {
                RemoveObject(hovered);
            }
            else
            {
                // call onplaced to show dud effect
                onPlaced?.Invoke(-1, null);
            }

        }

        public void RemoveObject(Placeable placeable)
        {
            if (placeable.originalOwner)
            {
                onPlaced?.Invoke(-2, placeable);
                placeable.Remove();
                material += placeable.materialCost;
                onMaterialChange?.Invoke(material, maxMaterial);
            }
            else
            {
                onPlaced?.Invoke(-1, null);
            }
        }
    }
}