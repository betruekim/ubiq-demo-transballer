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

                        px.gameObject.name = "snapper";
                        py.gameObject.name = "grounded";

                        Vector3 position = Snap.GetMatchingPosition(py.snaps[sy], px.snaps[sx]);
                        Quaternion rotation = Snap.GetMatchingRotation(py.snaps[sy], px.snaps[sx]);

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
}
