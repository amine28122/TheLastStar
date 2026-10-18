using UnityEngine;

namespace TheLastStar.Player
{
    public class ThirdPersonCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance & Angles")]
        [SerializeField] private float distance = 4.5f;
        [SerializeField] private float height = 1.6f;
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float smoothSpeed = 10f;

        private float currentYaw = 0f;
        private float currentPitch = 15f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Handle unlock cursor on Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = !Cursor.visible;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }

            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * height;
            Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * distance);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.rotation = rotation;
        }
    }
}
