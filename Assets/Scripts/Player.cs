using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GameU
{
    [SelectionBase, RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour, PlayerControls.IGameplayActions
    {
        [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running")]
        float runSpeed = 5f;

        [SerializeField, Range(0f, 3f), Tooltip("Maximum jump height when doing a normal jump action")]
        float normalJumpHeight = 1f;

        //[SerializeField, Range(0f, 3f), Tooltip("Additional jump height when doing a high jump action")]
        //float extraJumpHeight = 1f;

        [SerializeField]
        bool isDoubleJumpEnabled = true;

        [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
        float turnSpeed = 720f;

        [SerializeField, Range(1f, 5f)]
        float dashSpeedMultiplier = 4f;

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
        LayerMask groundLayers = 1;

        [SerializeField, Range(1f, 50f)]
        float gravityScalarWhenOnGround = 30f;

        [SerializeField, Range(0f, 1f)]
        float inAirTurnSpeedRatio = 0.5f;

        [SerializeField, Range(0f, 50f)]
        float inAirAcceleration = 20f;

        [SerializeField]
        bool computeGroundNormal;

        [SerializeField]
        bool followElevators = true;

        [SerializeField]
        GameObject groundedMarker; // HACK to visualize the state of IsGrounded

        [SerializeField]
        GameObject bodyModel; // HACK to visualize the state of dashing

        public float Stamina { get; private set; } = 1f;
        public bool IsGrounded { get; private set; }
        public bool IsDashReady => dash.State == Countdown.Phase.Ready && Stamina >= staminaPerDash;
        public bool IsDashActive => dash.State == Countdown.Phase.Active;
        public bool IsDashInCooldown => dash.State == Countdown.Phase.Cooling;
        public CollisionFlags Contacts => contacts;

        const float MIN_HEIGHT = -50f;

        private PlayerControls controls;
        private CharacterController body;
        private Vector2 input_move;
        private float input_dolly;
        private Vector3 velocity;
        private bool jumpRequested;
        private bool isSecondJumpReady;
        private CollisionFlags contacts;
        private Vector2 dashDir;
        private Countdown dash;
        private CinemachineVirtualCamera vCam;
        private CinemachineOrbitalTransposer vCam_transposer;
        private Vector3 vCam_offsetDirection;
        private float vCam_offsetDistance;
        private Material bodyMaterial;
        private Color normalColor;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            body.minMoveDistance = 0f; // force this to zero to ensure movement with small deltaTime (i.e., during high frame rate)
            controls = new PlayerControls();
            controls.gameplay.SetCallbacks(this);
            dash = new Countdown(dashDuration, dashCooldown);
            bodyMaterial = bodyModel.GetComponent<Renderer>().material;
            normalColor = bodyMaterial.color;
        }

        private void Start()
        {
            vCam = FindObjectOfType<CinemachineVirtualCamera>();
            Cursor.lockState = CursorLockMode.Locked;

            // Support camera dolly
            vCam_transposer = vCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            vCam_offsetDirection = vCam_transposer.m_FollowOffset;
            vCam_offsetDistance = vCam_offsetDirection.magnitude;
            vCam_offsetDirection.Normalize();
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }

        private void OnDestroy()
        {
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
        
        /// <summary>
        /// Invoked only when the Move context changes
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            input_move = context.ReadValue<Vector2>();
            // IMPORTANT! Remember to use a StickDeadzone processor on the Move action.
            // This ensures that diagonal movement is the same speed as orthognal movement,
            // and it accounts joysticks that no longer center perfectly due to wear damage.
            //print($"MOVE {input_move}");
        }
     
        public void OnLook(InputAction.CallbackContext context)
        {
            // Instead of writing our own code to orbit camera around the avatar, we will
            // use a CinemachineVirtualCamera with an Orbital Transposer in the Body field.
            // We also add a CinemachineInputProvider component to the virtual camera,
            // and set the "XY Axis" field to the desired input action.
            // NOTE: The "Control Type" of the input action must be "Vector2", even when
            // the virtual camera is only using an Orbital Transposer. In this case, the
            // Y-axis of the input action is ignored.
        }

        public void OnDolly(InputAction.CallbackContext context)
        {
            input_dolly = context.ReadValue<float>();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton() && IsDashReady)
            {
                dashDir = transform.forward.ToVector2().normalized;
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
            Vector2 camForward = vCam.transform.forward.ToVector2().normalized;
            Vector3 groundNormal = computeGroundNormal ? ComputeGroundNormal() : Vector3.up;
            Quaternion localToGround = Quaternion.LookRotation(camForward.ToVector3(), groundNormal);
            Vector3 inputVelocityWS = localToGround * input_move.ToVector3() * runSpeed;

            if (IsGrounded)
            {
                velocity = inputVelocityWS; // CHALLENGE! Change this to allow momentum and friction.
            }
            else
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

            activePlatform = null; // forget current active platform
            contacts = body.Move(velocity * Time.deltaTime);

            TurnTowards(inputVelocityWS);

            // Debug visualizations
            Debug.DrawRay(transform.position, velocity, Color.yellow);
            Debug.DrawRay(transform.position, inputVelocityWS, Color.white);
            Debug.DrawRay(transform.position, groundNormal * 3f, IsGrounded ? Color.cyan : Color.blue);
            groundedMarker.SetActive(IsGrounded);
            bodyMaterial.color = IsDashActive ? Color.cyan : normalColor;

            // Reload the scene when the player falls out of bounds
            if (transform.position.y < MIN_HEIGHT)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            // Apply dolly movement to the virtual camera
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

        private void TurnTowards(Vector3 directionWS)
        {
            if (IsDashActive) return;

            Vector2 direction2D = directionWS.ToVector2();
            if (direction2D.sqrMagnitude > 0f)
            {
                direction2D.Normalize();
                float targetYaw = Mathf.Atan2(direction2D.x, direction2D.y) * Mathf.Rad2Deg;
                Vector3 pitchYawRoll = transform.eulerAngles;
                float turnDelta = turnSpeed * Time.deltaTime;
                if (!IsGrounded) turnDelta *= inAirTurnSpeedRatio;
                pitchYawRoll.y = Mathf.MoveTowardsAngle(pitchYawRoll.y, targetYaw, turnDelta);
                transform.eulerAngles = pitchYawRoll;
            }
        }
        
        private void ApplyInAirMovement(Vector2 camForward)
        {
            if (inAirAcceleration <= 0f) return;

            // Allow some lateral movement when in air
            // Preserve momentum and enforce speed limits
            Quaternion localToWorld = Quaternion.LookRotation(camForward.ToVector3());
            Vector3 moveDirectionWS = localToWorld * input_move.ToVector3();
            Vector2 lateralAccelerationWS = moveDirectionWS.ToVector2() * inAirAcceleration;
            Vector2 lateralVelocityWS = velocity.ToVector2();
            lateralVelocityWS += lateralAccelerationWS * Time.deltaTime;
            float maxSpeed = runSpeed * (IsDashActive ? dashSpeedMultiplier : 1f); // CHALLENGE: account for moving platforms
            lateralVelocityWS = Vector2.ClampMagnitude(lateralVelocityWS, maxSpeed);
            velocity.x = lateralVelocityWS.x;
            velocity.z = lateralVelocityWS.y;
        }

        private Vector3 ComputeGroundNormal()
        {
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
                float dashSpeed = runSpeed * dashSpeedMultiplier;
                // CHALLENGE! respect the ground normal
                velocity.x = dashDir.x * dashSpeed;
                velocity.z = dashDir.y * dashSpeed;
            }
            dash.Update(Time.deltaTime);
        }

        private void UpdateStamina()
        {
            if (IsDashActive) return;
            Stamina = Mathf.MoveTowards(Stamina, 1f, staminaRecoveryRate * Time.deltaTime);
        }

    }
}
