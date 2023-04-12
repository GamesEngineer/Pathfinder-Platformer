using UnityEngine;
using UnityEngine.InputSystem;

namespace GameU
{
    [SelectionBase, RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour, PlayerControls.IGameplayActions
    {
        [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running")]
        float runSpeed = 4f;

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

        [SerializeField, Range(0f, 3f), Tooltip("Maximum jump height when doing a normal jump action")]
        float normalJumpHeight = 1f;

        //[SerializeField, Range(0f, 3f), Tooltip("Additional jump height when doing a high jump action")]
        //float extraJumpHeight = 1f;

        [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
        float turnSpeed = 360f;

        [SerializeField]
        LayerMask groundLayers;

        [SerializeField, Range(1f, 50f)]
        float gravityScalarWhenOnGround = 30f;

        [SerializeField, Range(0f, 50f)]
        float inAirAcceleration = 15f;

        [SerializeField]
        bool computeGroundNormal;

        public GameObject groundedMarker; // HACK to see state of isGrounded

        public float Stamina { get; private set; } = 1f;
        public bool IsGrounded { get; private set; }
        public bool IsDashReady => dash.State == Countdown.Phase.Ready && Stamina >= staminaPerDash;
        public bool IsDashActive => dash.State == Countdown.Phase.Active;
        public bool IsDashInCooldown => dash.State == Countdown.Phase.Cooling;

        private PlayerControls controls;
        private CharacterController body;
        private Vector2 input_move;
        private Vector3 velocity;
        private bool jumpRequested;
        private bool secondJumpEnabled;
        private CollisionFlags collisionFlags;
        private Camera cam;
        private Countdown dash;
        private float jumpCooldown;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            body.minMoveDistance = 0f; // force this to zero to ensure movement with small deltaTime (i.e., during high frame rate)
            controls = new PlayerControls();
            dash = new Countdown(dashDuration, dashCooldown);
            //dash.OnElapsed += Dash_OnElapsed;
        }

        private void Dash_OnElapsed()
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        private void Start()
        {
            cam = Camera.main;
            Cursor.lockState = CursorLockMode.Locked;
            print($"FPS {Screen.currentResolution.refreshRate}");
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
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
            if (context.ReadValueAsButton() && !jumpRequested)
            {
                //print($"JUMP {normalJumpHeight}");
                jumpRequested = body.isGrounded || secondJumpEnabled;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
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

        private void Update()
        {
            IsGrounded = body.isGrounded;

            // Movement is relative to the camera and parallel to the ground
            Vector2 camForward = cam.transform.forward.ToVector2().normalized;
            Vector3 groundNormal = ComputeGroundNormal();
            Quaternion localToGround = Quaternion.LookRotation(camForward.ToVector3(), groundNormal);
            Vector3 groundVelocityWS = localToGround * input_move.ToVector3() * runSpeed;

            Debug.DrawRay(transform.position, groundVelocityWS * 2f, Color.yellow);
            Debug.DrawRay(transform.position, groundNormal * 5f, IsGrounded ? Color.cyan : Color.blue);
            groundedMarker.SetActive(IsGrounded);

            if (IsGrounded)
            {
                velocity = groundVelocityWS;
            }
            else if (inAirAcceleration > 0f)
            {
                ApplyInAirMovement(camForward);
            }

            UpdateDash();
            UpdateStamina();

            if (jumpRequested)
            {
                velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
                jumpRequested = false;
                secondJumpEnabled = !secondJumpEnabled;
            }
            else
            {
                float g = Physics.gravity.y * Time.deltaTime;
                if (IsGrounded)
                {
                    secondJumpEnabled = false;
                    // Extra gravity feels better when going down slopes (less bouncing).
                    // It also contributes to better jumping by keeping the capsule grounded a bit longer.
                    // In real life, our back foot is still on ground when our body is just past the edge.
                    // Some game developers call this moment "coyote time".
                    g *= gravityScalarWhenOnGround;
                }
                velocity.y += g;
            }

            TurnTowards(groundVelocityWS);
            collisionFlags = body.Move(velocity * Time.deltaTime);
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
            IsGrounded = Physics.SphereCast(
                transform.position + Vector3.up * body.radius,
                body.radius, Vector3.down,
                out RaycastHit hitInfo,
                maxDistance: body.skinWidth * 2f,
                groundLayers,
                QueryTriggerInteraction.Ignore);
            return IsGrounded ? hitInfo.normal : Vector3.up;
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
                if (!body.isGrounded) turnDelta /= 2f;
                pitchYawRoll.y = Mathf.MoveTowardsAngle(pitchYawRoll.y, targetYaw, turnDelta);
                transform.eulerAngles = pitchYawRoll;
            }
        }

        public void OnExit(InputAction.CallbackContext context)
        {
            Application.Quit();
        }
    }
}
