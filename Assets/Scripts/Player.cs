using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;

namespace GameU
{
    [SelectionBase, RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour, PlayerControls.IGameplayActions
    {
        [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running")]
        float runSpeed = 4f;

        [SerializeField, Range(0f, 3f), Tooltip("Maximum jump height when doing a normal jump action")]
        float normalJumpHeight = 1f;

        //[SerializeField, Range(0f, 3f), Tooltip("Additional jump height when doing a high jump action")]
        //float extraJumpHeight = 1f;

        [SerializeField]
        bool isDoubleJumpEnabled = true;

        [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
        float turnSpeed = 360f;

        [SerializeField, Range(1f, 5f)]
        float dashSpeedMultiplier = 3f;

        [SerializeField, Range(0f, 1f)]
        float dashDuration = 0.25f;

        //[SerializeField, Range(0f, 1f), Tooltip("Amount of the dash in which the player is invincible to damage.")]
        //float dashInvicibility = 0.5f;

        [SerializeField, Range(0f, 1f)]
        float dashCooldown = 0.1f;

        [SerializeField, Range(0f, 1f)]
        float staminaPerDash = 0.2f;

        [SerializeField, Range(0f, 1f), Tooltip("Stamina per second")]
        float staminaRecoveryRate = 0.1f;

        [SerializeField]
        LayerMask groundLayers;

        [SerializeField, Range(1f, 50f)]
        float gravityScalarWhenOnGround = 30f;

        [SerializeField, Range(0f, 50f)]
        float inAirAcceleration = 15f;

        [SerializeField]
        bool computeGroundNormal;

        [SerializeField]
        bool followElevators = true;

        public GameObject groundedMarker; // HACK to see state of isGrounded

        public float Stamina { get; private set; } = 1f;
        public bool IsGrounded { get; private set; }
        public bool IsDashReady => dash.State == Countdown.Phase.Ready && Stamina >= staminaPerDash;
        public bool IsDashActive => dash.State == Countdown.Phase.Active;
        public bool IsDashInCooldown => dash.State == Countdown.Phase.Cooling;

        private PlayerControls controls;
        private CharacterController body;
        private Vector2 input_move;
        private float input_dolly;
        private Vector3 velocity;
        private bool jumpRequested;
        private bool isSecondJumpReady;
        private CollisionFlags collisionFlags;
        private Countdown dash;
        private float jumpCooldown;
        private Camera cam;

        private CinemachineOrbitalTransposer vCam_transposer;
        private Vector3 vCam_offsetDirection;
        private float vCam_offsetDistance;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            body.minMoveDistance = 0f; // force this to zero to ensure movement with small deltaTime (i.e., during high frame rate)
            controls = new PlayerControls();
            dash = new Countdown(dashDuration, dashCooldown);
        }

        private void Start()
        {
            cam = Camera.main;
            Cursor.lockState = CursorLockMode.Locked;

            // Support camera dolly
            var vCam = FindObjectOfType<CinemachineVirtualCamera>();
            vCam_transposer = vCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            vCam_offsetDirection = vCam_transposer.m_FollowOffset;
            vCam_offsetDistance = vCam_offsetDirection.magnitude;
            vCam_offsetDirection.Normalize();
        }

        private void OnEnable()
        {
            controls.gameplay.SetCallbacks(this);
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
            controls.gameplay.RemoveCallbacks(this);
        }

        #region MovingPlatform support

        private MovingPlatform activePlatform;
        public Vector3 PlatformVelocity => (activePlatform != null) ? activePlatform.Velocity : Vector3.zero;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var platform = hit.collider.GetComponentInParent<MovingPlatform>();
            if (platform != null)
            {
                activePlatform = platform;
            }
        }

        #endregion

        #region Input callbacks

        /// <summary>
        /// Invoked only when the Move context changes
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            input_move = context.ReadValue<Vector2>();
            // Don't let diagonal movement be faster than orthognal movement
            input_move = Vector2.ClampMagnitude(input_move, 1f);
            //print($"MOVE {input_move}");
        }

        /// <summary>
        /// Invoked only when the Jump context changes
        /// </summary>
        /// <param name="context"></param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                //print($"JUMP {normalJumpHeight}");
                jumpRequested = IsGrounded || isSecondJumpReady;
            }
        }
      
        public void OnLook(InputAction.CallbackContext context)
        {
            // horizontal is used by Cinemachine to orbit camera
        }

        public void OnDolly(InputAction.CallbackContext context)
        {
            input_dolly = context.ReadValue<float>();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton() && IsDashReady)
            {
                dash.Reset();
                Stamina = Mathf.MoveTowards(Stamina, 0f, staminaPerDash);
                //print($"DASH x{dashSpeedMultiplier} for {dashDuration:0.00}s with {Stamina:0.00} stamina remaining");
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // TODO
        }

        public void OnExit(InputAction.CallbackContext context)
        {
            Application.Quit();
        }

        #endregion

        private void Update()
        {
            IsGrounded = body.isGrounded;

            // Movement is relative to the camera and parallel to the ground
            Vector2 camForward = cam.transform.forward.ToVector2().normalized;
            Vector3 groundNormal = ComputeGroundNormal();
            Quaternion localToGround = Quaternion.LookRotation(camForward.ToVector3(), groundNormal);
            Vector3 inputVelocityWS = localToGround * input_move.ToVector3() * runSpeed;

            if (IsGrounded)
            {
                velocity = inputVelocityWS;
            }
            else if (inAirAcceleration > 0f)
            {
                ApplyInAirMovement(camForward);
            }

            UpdateDash();
            UpdateStamina();

            if (jumpRequested)
            {
                float jumpImpulse = Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
                velocity.y = jumpImpulse + (IsGrounded ? velocity.y : 0f);
                // Q: Shouldn't the jump velocity always be additive?
                // A: No. An additive impulse makes in-air jumping ("second jump") extra powerful when
                // quickly double-tapping the jump button. The max jump height then becomes dependent
                // on the frame-rate, instead of just player skill/timing. By overriding the vertical
                // velocity with the jump impulse, we ensure that max double-jump height is dependent
                // only on the player correctly timing the jump at the apex of the grounded "first jump".
                if (activePlatform != null)
                {
                    velocity += activePlatform.Velocity;
                }
                jumpRequested = false;
                isSecondJumpReady = isDoubleJumpEnabled && !isSecondJumpReady;
            }
            else
            {
                float g = Physics.gravity.y * Time.deltaTime;
                if (IsGrounded)
                {
                    isSecondJumpReady = false;
                    // Extra gravity feels better when going down slopes (less bouncing).
                    // It also contributes to better jumping by keeping the capsule grounded a bit longer.
                    // In real life, our back foot is still on ground when our body is just past the edge.
                    // Some game developers call this moment "coyote time".
                    g *= gravityScalarWhenOnGround;
                }
                velocity.y += g;
            }

            TurnTowards(inputVelocityWS);
            activePlatform = null; // forget current active platform
            collisionFlags = body.Move(velocity * Time.deltaTime);

            Debug.DrawRay(transform.position, velocity, Color.yellow);
            Debug.DrawRay(transform.position, inputVelocityWS, Color.white);
            Debug.DrawRay(transform.position, groundNormal * 3f, IsGrounded ? Color.cyan : Color.blue);

            groundedMarker.SetActive(IsGrounded);

            if (transform.position.y < -50f)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            vCam_offsetDistance = Mathf.Clamp(vCam_offsetDistance - input_dolly * 10f * Time.deltaTime, 3f, 20f);
            vCam_transposer.m_FollowOffset = vCam_offsetDirection * vCam_offsetDistance;
        }

        private void FixedUpdate()
        {
            // Follow elevators up & down in order to stay grounded and not jitter
            if (followElevators && activePlatform != null)
            {
                Vector3 movementWS = activePlatform.Velocity * Time.deltaTime;
                body.transform.Translate(movementWS, Space.World); // DO NOT use body.Move() here!
            }
        }

        private void ApplyInAirMovement(Vector2 camForward)
        {
            // Allow some lateral movement when in air
            // Preserve momentum and enfore speed limits
            Quaternion localToWorld = Quaternion.LookRotation(camForward.ToVector3());
            Vector3 moveDirectionWS = localToWorld * input_move.ToVector3();
            Vector2 lateralAccelerationWS = moveDirectionWS.ToVector2() * inAirAcceleration;
            Vector2 lateralVelocityWS = velocity.ToVector2();
            lateralVelocityWS += lateralAccelerationWS * Time.deltaTime;
            lateralVelocityWS = Vector2.ClampMagnitude(lateralVelocityWS, runSpeed * (IsDashActive ? dashSpeedMultiplier : 1f));
            velocity.x = lateralVelocityWS.x;
            velocity.z = lateralVelocityWS.y;
        }

        private Vector3 ComputeGroundNormal()
        {
            if (!computeGroundNormal) return Vector3.up;
            bool touching = Physics.SphereCast(
                transform.position + Vector3.up * body.radius,
                body.radius, Vector3.down,
                out RaycastHit hitInfo,
                maxDistance: body.skinWidth * 4f,
                groundLayers,
                QueryTriggerInteraction.Ignore);
            return touching ? hitInfo.normal : Vector3.up;
        }

        private void UpdateDash()
        {
            if (IsDashActive)
            {
                Vector2 dir = transform.forward.ToVector2().normalized;
                float dashSpeed = runSpeed * dashSpeedMultiplier;
                velocity.x = dir.x * dashSpeed;
                velocity.z = dir.y * dashSpeed;
            }
            else
            {
                float speed = velocity.ToVector2().magnitude;
                if (speed > runSpeed)
                {
                    // Blend back down to run speed
                    float newSpeed = Mathf.MoveTowards(speed, runSpeed, Time.deltaTime * 30f);
                    float ratio = newSpeed / speed;
                    velocity.x *= ratio;
                    velocity.z *= ratio;
                }

            }
            dash.Update(Time.deltaTime);
        }

        private void UpdateStamina()
        {
            Stamina = Mathf.MoveTowards(Stamina, 1f, staminaRecoveryRate * Time.deltaTime);
        }

        private void TurnTowards(Vector3 direction)
        {
            Vector2 direction2D = direction.ToVector2();
            if (direction2D.sqrMagnitude > 0f)
            {
                direction2D.Normalize();
                float targetYaw = Mathf.Atan2(direction2D.x, direction2D.y) * Mathf.Rad2Deg;
                Vector3 pitchYawRoll = transform.eulerAngles;
                float turnDelta = turnSpeed * Time.deltaTime;
                if (!IsGrounded) turnDelta /= 2f;
                pitchYawRoll.y = Mathf.MoveTowardsAngle(pitchYawRoll.y, targetYaw, turnDelta);
                transform.eulerAngles = pitchYawRoll;
            }
        }
    }
}
