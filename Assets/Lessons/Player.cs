using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase, RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, PlayerControls_Lesson.IGameplayActions
{
    [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running (meters per second)")]
    private float runSpeed = 5f;

    [SerializeField, Range(0f, 5f), Tooltip("Normal jump height (meters above ground)")]
    private float normalJumpHeight = 1f;

    [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
    private float turnSpeed = 360f;

    [SerializeField, Range(1f, 50f)]
    private float gravityScalarWhenOnGround = 20f;

    [SerializeField]
    private GameObject groundedMarker;

    [SerializeField, Range(1e-5f, 0.5f)]
    private float maxWallSlope = 0.1f;

    [SerializeField, Range(0.5f, 5f)]
    private float wallRunSpeedScalar = 2.0f;

    public bool IsGrounded { get; private set; }

    private CharacterController body;
    private Vector3 velocity;
    private bool jumpRequested;
    private bool jumpStarted;
    private Camera cam; // TODO - use Cinemachine Virtual Camera, instead
    private CinemachineBrain camBrain;
    private PlayerControls_Lesson controls;
    private Vector2 input_move;

    private GameObject activeWall;
    private Vector3 activeWallNormalWS;
    private Vector3 activeWallForwardWS;

    private void Awake()
    {
        body = GetComponent<CharacterController>();
        controls = new PlayerControls_Lesson();
        controls.gameplay.SetCallbacks(this);
    }

    private void Start()
    {
        cam = Camera.main;
        camBrain = cam.GetComponent<CinemachineBrain>();
        Cursor.lockState = CursorLockMode.Locked;
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
        //controls.gameplay.RemoveCallbacks(this);
    }

    private void Update()
    {
        IsGrounded = body.isGrounded;
        groundedMarker.SetActive(IsGrounded);

        // Move relative to the camera and parallel to the ground

        Vector3 groundNormal = Vector3.up;
        Vector2 camForward = cam.transform.forward.ToVector2().normalized;
        Quaternion localToGround = Quaternion.LookRotation(camForward.ToVector3(), groundNormal);
        Vector3 inputVelocityWS = localToGround * input_move.ToVector3() * runSpeed;

        if (IsGrounded)
        {
            velocity = inputVelocityWS;
        }
        else if (activeWall && jumpStarted)
        {
            velocity.x = activeWallForwardWS.x - activeWallNormalWS.x;
            // do not touch velocity.y (allow it to jump or fall)
            velocity.z = activeWallForwardWS.z - activeWallNormalWS.z;
        }
        else
        {
            ApplyInAirMovement(camForward);
        }

        float jumpImpulse = Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
        if (jumpStarted && body.isGrounded)
        {
            velocity.y = jumpImpulse;
            if (activePlatform)
            {
                velocity += activePlatform.Velocity;
            }
        }
        else if (jumpRequested && activeWall)
        {
            velocity.y = jumpImpulse;
            velocity += activeWallNormalWS * (jumpImpulse * 2f);
        }
        else
        {
            float g = Physics.gravity.y * Time.deltaTime;
            if (IsGrounded)
            {
                g *= gravityScalarWhenOnGround;
            }

            if (activeWall && jumpStarted)
            {
                // Wall running
                velocity.y = 0f;
            }
            else
            {
                // Falling
                velocity.y += g;
            }
        }

        activePlatform = null; // forget current active platform
        CollisionFlags contacts = body.Move(velocity * Time.deltaTime);
        if (!contacts.HasFlag(CollisionFlags.Sides))
        {
            activeWall = null; // forget the current wall
        }

        // Animated platforms update with FixedUpdate, so change our camera's update method when we ride a platform
        camBrain.m_UpdateMethod = activePlatform ?
            CinemachineBrain.UpdateMethod.FixedUpdate :
            CinemachineBrain.UpdateMethod.LateUpdate;

        // Turn towards wallForward when wall running
        Vector3 desiredForwardWS = activeWall ? activeWallForwardWS : inputVelocityWS;
        TurnTowards(desiredForwardWS);

        jumpRequested = false;
    }

    private void FixedUpdate()
    {
        if (activePlatform)
        {
            Vector3 movementWS = activePlatform.Velocity * Time.deltaTime;
            body.transform.Translate(movementWS, Space.World); // DO NOT use body.Move here
        }
    }

    private void TurnTowards(Vector3 directionWS)
    {
        Vector2 direction2D = directionWS.ToVector2();
        if (direction2D.sqrMagnitude <= float.Epsilon) return;

        direction2D.Normalize();
        float targetYaw = Mathf.Atan2(direction2D.x, direction2D.y) * Mathf.Rad2Deg;
        Vector3 pitchYawRoll = transform.eulerAngles;
        float turnDelta = turnSpeed * Time.deltaTime;
        if (!IsGrounded)
        {
            turnDelta *= inAirTurnSpeedRatio;
        }
        pitchYawRoll.y = Mathf.MoveTowardsAngle(pitchYawRoll.y, targetYaw, turnDelta);
        transform.eulerAngles = pitchYawRoll;
    }

    #region In-air movement

    [SerializeField, Range(0f, 50f)]
    float inAirAcceleration = 20f;

    [SerializeField, Range(0f, 1f)]
    float inAirTurnSpeedRatio = 0.5f;

    private void ApplyInAirMovement(Vector2 camForward)
    {
        // Allow some lateral movement when in air
        // Preserve momentum and enforce speed limits
        Quaternion localToWorld = Quaternion.LookRotation(camForward.ToVector3());
        Vector3 moveDirectionWS = localToWorld * input_move.ToVector3();
        Vector2 lateralAccelerationWS = moveDirectionWS.ToVector2() * inAirAcceleration;
        Vector2 lateralVelocityWS = velocity.ToVector2();
        lateralVelocityWS += lateralAccelerationWS * Time.deltaTime;
        float maxSpeed = runSpeed;
        lateralVelocityWS = Vector2.ClampMagnitude(lateralVelocityWS, maxSpeed);
        velocity.x = lateralVelocityWS.x;
        velocity.z = lateralVelocityWS.y;
    }

    #endregion

    #region IGameplayActions

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpStarted = context.performed;
        jumpRequested = context.canceled;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        input_move = context.ReadValue<Vector2>();
    }

    #endregion

    #region Moving Platform support

    private MovingPlatform activePlatform;

    public Vector3 PlatformVelocity => (activePlatform != null) ? activePlatform.Velocity : Vector3.zero;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var platform = hit.collider.GetComponentInParent<MovingPlatform>();
        if (platform && hit.normal.y > 0.5f)
        {
            activePlatform = platform;
        }

        if (hit.gameObject.CompareTag("Wall") &&
            Mathf.Abs(hit.normal.y) < maxWallSlope &&
            velocity.y <= 0f &&
            !IsGrounded)
        {
            activeWall = hit.gameObject;

            activeWallNormalWS = hit.normal;

            activeWallForwardWS = Vector3.ProjectOnPlane(velocity, activeWallNormalWS);
            activeWallForwardWS.y = 0f;
            activeWallForwardWS.Normalize();
            activeWallForwardWS *= runSpeed * wallRunSpeedScalar;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Handled by CinemachineInputProvider
    }

    #endregion
}
