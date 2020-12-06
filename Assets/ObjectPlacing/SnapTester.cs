using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlacableObjects;

// had to create this to debug snapping direction and position
// TODO: talk about this in report
public class SnapTester : MonoBehaviour
{
    public GameObject pipePrefab;

    GameObject[] groundedPipes;
    GameObject[] snapPipes;
    Vector3[] snapDefaultPosition;
    Quaternion[] snapDefaultRotation;

    private void Awake()
    {
        groundedPipes = new GameObject[4];
        snapPipes = new GameObject[4];
        snapDefaultPosition = new Vector3[4];
        snapDefaultRotation = new Quaternion[4];
        for (int i = 0; i < 4; i++)
        {
            groundedPipes[i] = GameObject.Instantiate(pipePrefab, Vector3.forward * (i * 4 + 2) + Vector3.up, Quaternion.identity);

            Vector3 offset = Vector3.right * 1.1f;
            Quaternion rotation = Quaternion.identity;
            if (i % 2 == 0)
            {
                offset = -offset;
            }
            if (i >= 2)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }

            snapDefaultPosition[i] = Vector3.forward * (i * 4 + 2) + Vector3.up + offset;
            snapDefaultRotation[i] = rotation;

            snapPipes[i] = GameObject.Instantiate(pipePrefab, snapDefaultPosition[i], snapDefaultRotation[i]);
        }
    }

    private void Update()
    {
        HandPlace();
        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < 4; i++)
            {
                Snap(snapPipes[i], groundedPipes[i]);
            }
        }
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
