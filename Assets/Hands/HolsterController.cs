using UnityEngine;
using Ubik.XR;

public class HolsterController : MonoBehaviour
{
    public bool right;
    UIManager manager;
    public Vector3 cameraOffset;
    public GameObject mainCamera;

    private void Awake()
    {
        manager = gameObject.GetComponentInParent<UIManager>();
        mainCamera = Camera.main.gameObject;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y, 0);

        transform.position = mainCamera.transform.position + transform.rotation * cameraOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hands"))
        {
            manager.HolsterTrigger(right, other.GetComponent<HandController>());
        }
    }
}