using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlacableObjects;
using Ubik.Samples;

// had to create this to debug snapping direction and position
// TODO: talk about this in report
public class SnapTester : MonoBehaviour
{
    public PrefabCatalogue catalogue;

    GameObject[] groundedPipes;
    GameObject[] snapPipes;
    Vector3[] snapDefaultPosition;
    Quaternion[] snapDefaultRotation;

    private void Awake()
    {
        int count = catalogue.prefabs.Count * catalogue.prefabs.Count * 4;
        groundedPipes = new GameObject[count];
        snapPipes = new GameObject[count];
        snapDefaultPosition = new Vector3[count];
        snapDefaultRotation = new Quaternion[count];
        int index = 0;
        for (int x = 0; x < catalogue.prefabs.Count; x++)
        {
            for (int y = 0; y < catalogue.prefabs.Count; y++)
            {
                int i = 0;
                for (int sx = 0; sx < catalogue.prefabs[x].GetComponent<Placable>().snaps.Length; sx++)
                {
                    for (int sy = 0; sy < catalogue.prefabs[y].GetComponent<Placable>().snaps.Length; sy++)
                    {
                        GameObject parentObj = new GameObject($"{x},{y}:{sx},{sy}");

                        Vector3 groundPos = Vector3.forward * (i * 4 + 2) + Vector3.up + Vector3.right * index * 4;
                        // groundedPipes[index] = GameObject.Instantiate(catalogue.prefabs[x], groundPos, Quaternion.identity);
                        Placable py = GameObject.Instantiate(catalogue.prefabs[y], groundPos, Quaternion.identity, parentObj.transform).GetComponent<Placable>();
                        Placable px = GameObject.Instantiate(catalogue.prefabs[x], groundPos, Quaternion.identity, parentObj.transform).GetComponent<Placable>();

                        Quaternion rotation = Quaternion.Inverse(px.snaps[sx].transform.localRotation) * py.snaps[sy].transform.rotation * Quaternion.Euler(0, 180, 0);
                        Vector3 position = py.snaps[sy].transform.position - rotation * px.snaps[sx].transform.localPosition;

                        snapDefaultPosition[index] = position;
                        snapDefaultRotation[index] = rotation;

                        px.transform.position = position;
                        px.transform.rotation = rotation;

                        // snapPipes[index] = GameObject.Instantiate(catalogue.prefabs[y], snapDefaultPosition[index], snapDefaultRotation[index]);
                        i++;
                    }
                }
                index++;
            }
        }
    }

    private void Update()
    {
        // HandPlace();
        // if (Input.GetKey(KeyCode.Space))
        // {
        //     for (int i = 0; i < 4; i++)
        //     {
        //         Snap(snapPipes[i], groundedPipes[i]);
        //     }
        // }
    }

    void HandPlace()
    {
        for (int i = 0; i < 4; i++)
        {
            snapPipes[i].transform.position = snapDefaultPosition[i];
            snapPipes[i].transform.rotation = snapDefaultRotation[i];
        }
    }

    void Snap(GameObject snapper, GameObject grounded)
    {
        Placable ghost = snapper.GetComponent<Placable>();
        // check rays from each snap
        GameObject cachedHit = null;
        int snapIndex = -1;
        for (int i = 0; i < ghost.snaps.Length; i++)
        {
            Ray ray = new Ray(ghost.snaps[i].transform.position - ghost.snaps[i].transform.right * 0.1f, ghost.snaps[i].transform.right);
            Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);
            RaycastHit[] hits = Physics.RaycastAll(ray, 0.25f, LayerMask.GetMask("Snap"));
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject != ghost.snaps[i].gameObject)
                {
                    cachedHit = hit.transform.gameObject;
                    snapIndex = i;
                    break;
                }
            }
        }
        if (cachedHit)
        {
            Debug.Log($"{snapIndex}, {cachedHit.GetComponent<Snap>().index}");
            ghost.transform.rotation = ghost.snaps[snapIndex].transform.localRotation * cachedHit.transform.rotation * Quaternion.Euler(0, 180, 0);
            ghost.transform.position = cachedHit.transform.position - ghost.transform.rotation * ghost.snaps[snapIndex].transform.localPosition;
        }
    }
}
