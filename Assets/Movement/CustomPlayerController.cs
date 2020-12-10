using System;
using System.Linq;
using UnityEngine;
using Ubik.XR;
/// <summary>
/// This VR Player Controller supports a typical Head and Two Hand tracked rig.
/// </summary>
public class CustomPlayerController : PlayerController
{

    private static GameObject singleton;

    private Vector3 velocity;
    private Vector3 userLocalPosition;

    private void Start()
    {
        foreach (var item in GetComponentsInChildren<TeleportRay>())
        {
            if (item.transform.parent.name == "Right Hand")
            {
                item.OnTeleport.AddListener(OnTeleport);
            }
        }
    }

    new public void OnTeleport(Vector3 position)
    {
        userLocalPosition = transform.InverseTransformPoint(headCamera.transform.position);
        userLocalPosition.y = 0;

        var movement = position - transform.TransformPoint(userLocalPosition);  // move so the foot position is over the new teleport location
        transform.position += movement;
    }

    private void FixedUpdate()
    {
        // Update the foot position. This is done by pulling the feet using a rubber band.
        // Decoupling the feet in this way allows the user to do things like lean over edges, when the ground check is enabled.
        // This can be effectively disabled by setting the animation curve to a constant high value.

        var headProjectionXZ = transform.InverseTransformPoint(headCamera.transform.position);
        headProjectionXZ.y = 0;
        userLocalPosition.x += (headProjectionXZ.x - userLocalPosition.x) * Time.deltaTime * cameraRubberBand.Evaluate(Mathf.Abs(headProjectionXZ.x - userLocalPosition.x));
        userLocalPosition.z += (headProjectionXZ.z - userLocalPosition.z) * Time.deltaTime * cameraRubberBand.Evaluate(Mathf.Abs(headProjectionXZ.z - userLocalPosition.z));
        userLocalPosition.y = 0;

        foreach (var item in handControllers)
        {
            // only snap with right controller
            if (item.JoystickSwipe.Trigger && item.Right)
            {
                transform.RotateAround(headCamera.transform.position, Vector3.up, 45f * Mathf.Sign(item.JoystickSwipe.Value));
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        //Gizmos.DrawWireCube(Vector3.zero, new Vector3(Radius, 0.1f, Radius));
        Gizmos.DrawLine(userLocalPosition, transform.InverseTransformPoint(headCamera.transform.position));
    }
}