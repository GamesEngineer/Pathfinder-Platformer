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

        public float Stamina { get; private set; } = 1f;

        private PlayerControls controls;
        private CharacterController body;
        private Vector2 input_move;
        private Vector3 velocity;
        private bool jumpRequested;
        private CollisionFlags collisionFlags;
        private Camera cam;
        private Countdown dash;

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
            if (context.ReadValueAsButton() && body.isGrounded)
            {
                print($"JUMP {normalJumpHeight}");
                jumpRequested = true;
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
                print($"DASH x{dashSpeedMultiplier} for {dashDuration:0.00}s with {Stamina:0.00} stamina remaining");
            }
        }

        public bool IsDashReady => dash.State == Countdown.Phase.Ready && Stamina >= staminaPerDash;
        public bool IsDashActive => dash.State == Countdown.Phase.Active;
        public bool IsDashInCooldown => dash.State == Countdown.Phase.Cooling;

        public bool computeGroundNormal;

        private void FixedUpdate()
        {
            // Movement is relative to the camera and parallel to the ground
            Vector3 localVelocity = (input_move * runSpeed).ToVector3();
            Vector2 camForward = cam.transform.forward.ToVector2().normalized;
            Vector3 groundNormal = Vector3.up;
            if (computeGroundNormal && Physics.SphereCast(transform.position + Vector3.up * body.radius, body.radius, Vector3.down, out RaycastHit hitInfo, maxDistance: 0f, groundLayers, QueryTriggerInteraction.Ignore))
            {
                groundNormal = hitInfo.normal;
            }
            Quaternion localToWorld = Quaternion.LookRotation(camForward.ToVector3(), groundNormal);
            Vector3 desiredGroundVelocity = localToWorld * localVelocity;
            Debug.DrawRay(transform.position, desiredGroundVelocity * 2f, Color.yellow);
            Debug.DrawRay(transform.position, groundNormal * 5f, body.isGrounded ? Color.cyan : Color.blue);

            if (body.isGrounded)
            {
                velocity = desiredGroundVelocity;
            }

            UpdateDash();
            UpdateStamina();

            // CHALLENGE! Allow some lateral movement when in air, but preserve momentum

            if (jumpRequested)
            {
                velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
                jumpRequested = false;
            }
            else
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }

            TurnTowards(desiredGroundVelocity);
            collisionFlags = body.Move(velocity * Time.deltaTime);
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
