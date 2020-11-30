using System;
using System.Linq;
using UnityEngine;

namespace Ubik.XR
{
    /// <summary>
    /// This Desktop Player Controller supports a mouse and keyboard interaction with a scene.
    /// </summary>

    public class DesktopPlayerController : MonoBehaviour
    {
        [NonSerialized]

        private Vector3 velocity;
        private Vector3 userLocalPosition;

        public Camera headCamera;
        public Transform cameraContainer;
        public AnimationCurve cameraRubberBand;

        [Tooltip("Joystick and Keyboard Speed in m/s")]
        public float movementSpeed = 2f;

        [Tooltip("Rotation Speed in degrees per second")]
        public float rotationSpeed = 360f;

        private void OnMouse()
        {
            if (Input.GetMouseButton(1))
            {
                float xRotation = Input.GetAxis("Mouse X");
                float yRotation = Input.GetAxis("Mouse Y");

                transform.RotateAround(transform.position, Vector3.up, xRotation * rotationSpeed * Time.deltaTime);
                cameraContainer.RotateAround(cameraContainer.position, transform.right, -yRotation * rotationSpeed * Time.deltaTime);

                Cursor.lockState = CursorLockMode.Locked;
            } else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void OnKeys()
        {
            Vector3 movement = new Vector3(0f, 0f, 0f);
            if (Input.GetKey(KeyCode.A))
            {
                movement += new Vector3(-1f, 0f, 0f);
            }
            if (Input.GetKey(KeyCode.D))
            {
                movement += new Vector3(1f, 0f, 0f);
            }
            if (Input.GetKey(KeyCode.W))
            {
                movement += new Vector3(0f, 0f, 1f);
            }
            if (Input.GetKey(KeyCode.S))
            {
                movement += new Vector3(0f, 0f, -1f);
            }
            movement = movement.normalized * (movementSpeed) * Time.fixedDeltaTime;
            movement = headCamera.transform.TransformDirection(movement);
            movement.y = 0f;

            transform.position += movement;
        }

        private void OnGround()
        {
            var height = Mathf.Clamp(transform.InverseTransformPoint(headCamera.transform.position).y, 0.1f, float.PositiveInfinity);
            var origin = transform.position + userLocalPosition + Vector3.up * height;
            var direction = Vector3.down;

            RaycastHit hitInfo;
            if(Physics.Raycast(new Ray(origin, direction), out hitInfo))
            {
                var virtualFloorHeight = hitInfo.point.y;

                if (transform.position.y < virtualFloorHeight)
                {
                    transform.position += Vector3.up * (virtualFloorHeight - transform.position.y) * Time.deltaTime * 3f;
                    velocity = Vector3.zero;
                }
                else
                {
                    velocity += Physics.gravity * Time.deltaTime;
                }
            }
            else
            {
                velocity = Vector3.zero; // if there is no 'ground' in the scene, then do nothing
            }

            transform.position += velocity * Time.deltaTime;
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

            OnMouse();
            OnKeys();

            //OnGround(); //todo: finish implementation
        }

    }
}
