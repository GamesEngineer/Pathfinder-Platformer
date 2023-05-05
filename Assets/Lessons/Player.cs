using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase, RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, PlayerControls_Lesson.IGameplayActions
{
    [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running (meters per second)")]
    float runSpeed = 4f;

    [SerializeField, Range(0f, 5f), Tooltip("Normal jump height (meters above ground)")]
    float normalJumpHeight = 1f;

    [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
    float turnSpeed = 360f;

    [SerializeField, Range(1f, 50f)]
    private float gravityScalarWhenOnGround = 20f;

    [SerializeField]
    GameObject groundedMarker;

    public bool IsGrounded { get; private set; }

    private CharacterController body;
    private Vector3 velocity;
    private bool jumpRequested;
    private Camera cam; // TODO - use Cinemachine Virtual Camera, instead
    private PlayerControls_Lesson controls;
    private Vector2 input_move;

    private void Awake()
    {
        body = GetComponent<CharacterController>();
        controls = new PlayerControls_Lesson();
        controls.gameplay.SetCallbacks(this);
    }

    private void Start()
    {
        cam = Camera.main;
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
        else
        {
            ApplyInAirMovement(camForward);
        }

        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
            jumpRequested = false;
        }
        else
        {
            float g = Physics.gravity.y * Time.deltaTime;
            if (IsGrounded)
            {
                g *= gravityScalarWhenOnGround;
            }
            velocity.y += g;
        }

        body.Move(velocity * Time.deltaTime);

        TurnTowards(inputVelocityWS);
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
        if (context.ReadValueAsButton() && !jumpRequested) // Should this be simplified and ignore current jumpRequested state?
        {
            jumpRequested = body.isGrounded;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        input_move = context.ReadValue<Vector2>();
    }

    #endregion
}
