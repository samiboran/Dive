using UnityEngine;
using UnityEngine.InputSystem;

namespace UnderwaterExtraction.Core
{
    /// <summary>
    /// Player controller for underwater movement with buoyancy and momentum.
    /// v0.1 - "Çalışsın, sonra polish" felsefesi.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float swimSpeed = 5.0f;
        [SerializeField] private float verticalSpeed = 3.0f;
        [SerializeField] private float acceleration = 8.0f;
        [SerializeField] private float maxVelocity = 6.0f;

        [Header("Look Settings")]
        [SerializeField] private float lookSpeedX = 2.0f;
        [SerializeField] private float lookSpeedY = 2.0f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Buoyancy Settings")]
        [SerializeField] private float surfaceBuoyancy = 1.0f;
        [SerializeField] private float maxDepthBuoyancy = 3.0f;
        [SerializeField] private float maxDepth = 8.0f;
        [SerializeField] private float waterSurfaceHeight = 8.0f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private Rigidbody rb;
        private PlayerControllerIntegration integration;
        private Vector2 moveInput;
        private float verticalInput;
        private Vector2 lookInput;
        private float currentPitch;
        private float currentYaw;

        private const float GRAVITY = -9.81f;

        /// <summary>Depth below the water surface (0 at surface, increasing going down). Shared with PlayerControllerIntegration for oxygen depth calculations.</summary>
        public float DepthBelowSurface => Mathf.Max(0f, waterSurfaceHeight - transform.position.y);

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            integration = GetComponent<PlayerControllerIntegration>();
            ConfigureRigidbody();

            if (cameraTransform == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraTransform = cam.transform;
                else
                    Debug.LogError("Camera Transform not assigned and no Camera found in children!");
            }
        }

        private void Start()
        {
            currentYaw = transform.eulerAngles.y;
            if (cameraTransform != null)
                currentPitch = cameraTransform.localEulerAngles.x;
        }

        private void FixedUpdate()
        {
            // Apply buoyancy first to maintain depth
            ApplyBuoyancy();

            // Then apply movement
            ApplyMovement();

            // Apply drag
            ApplyWaterDrag();
        }

        private void LateUpdate()
        {
            // Handle rotation in LateUpdate for smoother camera
            HandleRotation();
        }

        #region Rigidbody Configuration

        private void ConfigureRigidbody()
        {
            rb.useGravity = false;
            rb.drag = 3f;
            rb.angularDrag = 1f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        #endregion

        #region Input Callbacks (New Input System)

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnVertical(InputValue value)
        {
            verticalInput = value.Get<float>();
        }

        public void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        #endregion

        #region Movement Logic

        private void ApplyMovement()
        {
            // Calculate horizontal movement direction relative to player's forward
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Flatten forward for horizontal movement (don't climb/dive with forward)
            Vector3 flatForward = new Vector3(forward.x, 0, forward.z).normalized;

            // Panic/injury/bandage speed penalty or boost from PlayerControllerIntegration, if present
            float speedMultiplier = integration != null ? integration.SpeedMultiplier : 1f;

            // Calculate horizontal input direction
            Vector3 horizontalVelocity = (flatForward * moveInput.y + right * moveInput.x) * swimSpeed * speedMultiplier;

            // Calculate vertical velocity
            float verticalVelocity = verticalInput * verticalSpeed * speedMultiplier;

            // Combine velocities
            Vector3 desiredVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);

            // Apply clamping to prevent exceeding max velocity
            if (desiredVelocity.magnitude > maxVelocity)
                desiredVelocity = desiredVelocity.normalized * maxVelocity;

            // Smoothly interpolate current velocity toward desired velocity (momentum/inertia)
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        }

        private void ApplyBuoyancy()
        {
            // Depth below the water surface: 0 at the surface, increasing going down.
            float depthBelowSurface = Mathf.Max(0f, waterSurfaceHeight - transform.position.y);

            if (depthBelowSurface <= 0f)
            {
                // Breached the surface: fall back down like normal gravity instead of floating away.
                rb.AddForce(Vector3.up * GRAVITY, ForceMode.Acceleration);
                return;
            }

            // The deeper below the surface, the stronger the buoyant push back up.
            float depthRatio = Mathf.Clamp01(depthBelowSurface / maxDepth);
            float buoyancyForce = Mathf.Lerp(surfaceBuoyancy, maxDepthBuoyancy, depthRatio);
            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Acceleration);
        }

        private void ApplyWaterDrag()
        {
            // Additional drag when moving fast to simulate water resistance
            float speed = rb.velocity.magnitude;
            if (speed > swimSpeed * 0.5f)
            {
                float extraDrag = (speed - swimSpeed * 0.5f) * 0.1f;
                rb.velocity *= (1 - Mathf.Min(extraDrag, 0.3f) * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Rotation Logic

        private void HandleRotation()
        {
            if (cameraTransform == null) return;

            // Yaw (body rotation) - horizontal look
            currentYaw += lookInput.x * lookSpeedX * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, currentYaw, 0);

            // Pitch (camera rotation) - vertical look
            currentPitch -= lookInput.y * lookSpeedY * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            // Apply camera rotation separately from body
            cameraTransform.localRotation = Quaternion.Euler(currentPitch, 0, 0);
        }

        #endregion

        #region Public Methods (For Debug/Testing)

        public void SetPosition(Vector3 position)
        {
            rb.position = position;
        }

        public void ResetRotation()
        {
            currentYaw = 0;
            currentPitch = 0;
            transform.rotation = Quaternion.identity;
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.identity;
        }

        #endregion
    }
}
