using UnityEngine;
using Ubik.XR;

public class HolsterController : MonoBehaviour
{
    public bool right;
    UIManager manager;

    private void Awake()
    {
        manager = gameObject.GetComponentInParent<UIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hands"))
        {
            manager.HolsterTrigger(right, other.GetComponent<HandController>());
        }
    }
}