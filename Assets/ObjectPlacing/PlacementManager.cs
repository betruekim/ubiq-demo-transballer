using System;
using System.Collections.Generic;
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
                        controller.TriggerPress.AddListener((bool pressed) => { if (pressed) { PlaceObject(); } });
                    }
                    if (controller.GripPress != null)
                    {
                        controller.GripPress.AddListener((bool pressed) => { if (pressed) { DeselectObject(); } });
                    }
                    // if (controller.PrimaryButtonPress != null)
                    // {
                    //     controller.PrimaryButtonPress.AddListener((bool pressed) => { if (pressed) { if (selectedObject >= 0) { DeselectObject(); } else { SelectObject(0); } } });
                    // }
                }
                else
                if (controller.gameObject.name == "Left Hand")
                // if (controller.Left)
                {
                    leftHand = controller;
                    if (controller.PrimaryButtonPress != null)
                    {
                        controller.PrimaryButtonPress.AddListener((bool pressed) => { if (pressed) { SwitchSnaps(); } });
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
        const float maxPlaceDist = 5f;
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
            placeDist = 1f;
            SpawnGhostObject();
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
            removing = true;
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
            if (leftHand.GripState)
            {
                placeDist += rightHand.Joystick.y * Time.deltaTime;
                placeDist = Mathf.Clamp(placeDist, minPlaceDist, maxPlaceDist);
                customRotation *= Quaternion.Euler(0, leftHand.Joystick.x, 0);
            }
            else if (leftHand.TriggerState)
            {
                customRotation *= Quaternion.Euler(0, 0, leftHand.Joystick.y);
            }
        }

        private Snap cachedHit; // the thing we hit using snap raycasts
        private int snapIndex = -1; // the index of the snap object on ghostObject
        const float maxRaycastDist = 2f;
        bool canBePlaced = false;

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
                        cachedHit = hit.transform.gameObject.GetComponent<Snap>();
                        snapIndex = i;
                        return;
                    }
                }
            }
            cachedHit = null;
            snapIndex = -1;
        }

        private void MoveGhostToSnapPos()
        {
            DoRaycast();
            if (cachedHit)
            {
                if (ghostObject.CanBePlacedOn(cachedHit))
                {
                    ghostObject.transform.rotation = Snap.GetMatchingRotation(cachedHit, ghostObject.snaps[snapIndex]);
                    ghostObject.transform.position = Snap.GetMatchingPosition(cachedHit, ghostObject.snaps[snapIndex]);
                }
            }
            ghostObject.Move();
        }

        Placeable hovered = null;

        private void FixedUpdate()
        {
            cachedHit = null;
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
                // compute canBePlaced
                canBePlaced = ghostObject.materialCost <= material;
                if (cachedHit)
                {
                    canBePlaced = canBePlaced && ghostObject.CanBePlacedOn(cachedHit);
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

        public void DeselectObject()
        {
            // call this to empty hand
            selectedObject = -1;
            if (ghostObject)
            {
                ghostObject.Deselect();
            }
            foreach (Placeable placeable in PlaceableIndex.placedObjects.Values)
            {
                foreach (Snap s in placeable.snaps)
                {
                    s.HideGraphic();
                }
            }
        }

        public void PlaceObject()
        {
            // TODO: check if we are aiming at a canvas
            if (selectedObject >= 0 && ghostObject && canBePlaced)
            {
                if (ghostObject.materialCost <= material)
                {
                    if (cachedHit)
                    {
                        ghostObject.Place(snapIndex, cachedHit.placeable.Id, cachedHit.index);
                    }
                    else
                    {
                        ghostObject.Place();
                    }
                    material -= ghostObject.materialCost;
                    onMaterialChange?.Invoke(material, maxMaterial);
                    ghostObject = null;
                    DeselectObject();
                }
            }
            else if (removing)
            {
                RemoveObject(hovered);
            }

        }

        public void RemoveObject(Placeable placeable)
        {
            if (placeable.originalOwner)
            {
                placeable.Remove();
                material += placeable.materialCost;
                onMaterialChange?.Invoke(material, maxMaterial);
            }
        }
    }
}