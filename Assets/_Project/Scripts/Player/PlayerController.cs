using UnityEngine;
using TheLastStar.Interaction;

namespace TheLastStar.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.5f;
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float jumpHeight = 1.6f;
        [SerializeField] private float gravity = -20f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.25f;
        [SerializeField] private LayerMask groundMask;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 2.5f;
        [SerializeField] private LayerMask interactionMask = ~0;

        [Header("Camera Reference")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float turnSmoothVelocity;
        private IInteractable currentInteractable;

        public bool IsGrounded => isGrounded;
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            HandleGroundCheck();
            HandleMovement();
            HandleInteractionCheck();
        }

        private void HandleGroundCheck()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                isGrounded = controller.isGrounded;
            }

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Keeps player snapped to ground
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                if (cameraTransform != null)
                {
                    targetAngle += cameraTransform.eulerAngles.y;
                }

                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * (targetSpeed * Time.deltaTime));
                CurrentSpeed = targetSpeed;
            }
            else
            {
                CurrentSpeed = 0f;
            }

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleInteractionCheck()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, interactionRange, interactionMask);
            currentInteractable = null;

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                var interactable = hit.GetComponent<IInteractable>() ?? hit.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    currentInteractable = interactable;
                    break;
                }
            }

            if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + transform.forward * 1.0f, interactionRange);

            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
