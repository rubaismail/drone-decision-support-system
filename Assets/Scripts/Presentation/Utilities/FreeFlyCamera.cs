using UnityEngine;

namespace Presentation.Utilities
{
    public class FreeFlyCamera : MonoBehaviour
    {
        public float moveSpeed = 10f;
        public float fastSpeed = 30f;
        public float mouseSensitivity = 2f;

        private float _yaw = 0f;
        private float _pitch = 0f;

        void Start()
        {
            // Do not permanently lock cursor at Start.
            Cursor.lockState = CursorLockMode.None;
        }

        void Update()
        {
            HandleMouseLook();
            HandleMovement();
        }

        void HandleMouseLook()
        {
            if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
            {
                Cursor.lockState = CursorLockMode.Locked; 
            }

            if (Input.GetMouseButtonUp(1)) // Right mouse button released
            {
                Cursor.lockState = CursorLockMode.None; 
            }

            if (Input.GetMouseButton(1)) // Hold right-click to rotate
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                _yaw += mouseX;
                _pitch -= mouseY;
                _pitch = Mathf.Clamp(_pitch, -89f, 89f);

                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }
        }

        void HandleMovement()
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : moveSpeed;

            Vector3 move = new Vector3(
                Input.GetAxis("Horizontal"),                     // A/D
                (Input.GetKey(KeyCode.Space) ? 1 : 0) -          // SPACE = up
                (Input.GetKey(KeyCode.LeftControl) ? 1 : 0),     // CTRL = down
                Input.GetAxis("Vertical")                        // W/S
            );

            transform.Translate(move * (speed * Time.deltaTime), Space.Self);
        }
    }
}
