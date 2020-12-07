using System.Collections.Generic;
using UnityEngine;
using Ubik.Samples;
using Ubik.XR;

namespace PlacableObjects
{
    public class PlacementManager : MonoBehaviour
    {
        NetworkSpawner networkSpawner;
        public HandController rightHand;
        public HandController leftHand;

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
        public int selectedObject { get; private set; } = -1;
        Placable ghostObject = null;
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

            if (ghostObject != null)
            {
                DeselectObject();
            }
            selectedObject = index;
            customRotation = Quaternion.identity;
            placeDist = 1f;
            SpawnGhostObject();
        }

        private void SpawnGhostObject()
        {
            Debug.Log("SpawnGhostObject");
            ghostObject = networkSpawner.Spawn(objects[selectedObject]).GetComponent<Placable>();
            MoveGhostToHandPos();
        }

        private void Update()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (selectedObject != i)
                    {
                        SelectObject(i);
                    }
                    else
                    {
                        DeselectObject();
                    }
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

        private GameObject cachedHit; // the thing we hit using snap raycasts
        private int snapIndex = -1; // the index of the snap object on ghostObject
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
            // check rays from each snap
            for (int i = 0; i < ghostObject.snaps.Length; i++)
            {
                Ray ray = new Ray(ghostObject.snaps[i].transform.position - ghostObject.snaps[i].transform.right * 0.1f, ghostObject.snaps[i].transform.right);
                Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);
                RaycastHit[] hits = Physics.RaycastAll(ray, 0.25f, LayerMask.GetMask("Snap"));
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject != ghostObject.snaps[i].gameObject)
                    {
                        cachedHit = hit.transform.gameObject;
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
                Debug.Log($"{snapIndex}, {cachedHit.GetComponent<Snap>().index}");
                ghostObject.transform.rotation = Quaternion.Inverse(ghostObject.snaps[snapIndex].transform.localRotation) * cachedHit.transform.rotation * Quaternion.Euler(0, 180, 0);
                ghostObject.transform.position = cachedHit.transform.position - ghostObject.transform.rotation * ghostObject.snaps[snapIndex].transform.localPosition;
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
            }
        }

        public void PlaceObject()
        {
            if (selectedObject >= 0)
            {
                Debug.Log("placing the ting");
                if (cachedHit)
                {
                    Snap snappedTo = cachedHit.GetComponent<Snap>();
                    ghostObject.Place(snapIndex, snappedTo.placable.Id, snappedTo.index);
                }
                else
                {
                    ghostObject.Place();
                }
                selectedObject = -1;
                ghostObject = null;
            }
        }

    }
}