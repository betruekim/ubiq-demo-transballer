using UnityEngine;
using Ubik.XR;

public class HolsterController : MonoBehaviour
{
    public bool track = true;
    UIManager manager;
    public Vector3 cameraOffset;
    public GameObject mainCamera;

    private void Awake()
    {
        manager = GameObject.FindObjectOfType<UIManager>();
        mainCamera = Camera.main.gameObject;
    }

    private void Update()
    {
        if (track)
        {
            transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y, 0);

            transform.position = mainCamera.transform.position + transform.rotation * cameraOffset;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hands"))
        {
            manager.HolsterTrigger(other.GetComponent<HandController>());
        }
    }
}