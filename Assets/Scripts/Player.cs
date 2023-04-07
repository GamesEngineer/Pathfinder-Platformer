using UnityEngine;
using UnityEngine.InputSystem;

namespace GameU
{
    static class Extensions
    {
        public static Vector3 ToVector3(this Vector2 v, float y = 0f) => new(v.x, y, v.y);
        public static Vector2 ToVector2(this Vector3 v) => new(v.x, v.z);
    }

    [SelectionBase, RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour, PlayerControls.IGameplayActions
    {
        [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running")]
        float runSpeed = 4f;

        [SerializeField, Range(0f, 3f), Tooltip("Maximum jump height when doing a normal jump action")]
        float normalJumpHeight = 1f;

        //[SerializeField, Range(0f, 3f), Tooltip("Additional jump height when doing a high jump action")]
        //float extraJumpHeight = 1f;

        [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
        float turnSpeed = 360f;

        [SerializeField]
        LayerMask groundLayers;

        private PlayerControls controls;
        private CharacterController body;
        private Vector2 input_move;
        private bool input_jump;
        private Vector3 velocity;
        private bool jumpRequested;
        private CollisionFlags collisionFlags;
        private Camera cam;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            body.minMoveDistance = 0f; // force this to zero to ensure movement with small deltaTime (i.e., during high frame rate)
            controls = new PlayerControls();
        }

        private void Start()
        {
            cam = Camera.main;
            Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
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
            input_jump = context.ReadValueAsButton();
            //print($"JUMP {input_jump}");
            jumpRequested |= input_jump;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            // TODO
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            // TODO
            if (context.started)
            {
                computeGroundNormal = !computeGroundNormal;
            }
        }

        public bool computeGroundNormal;

        private void Update()
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

            Debug.DrawRay(transform.position, velocity * 2f, Color.yellow);
            Debug.DrawRay(transform.position, groundNormal * 5f, body.isGrounded ? Color.cyan : Color.blue);

            if (body.isGrounded)
            {
                velocity = localToWorld * localVelocity;
                if (jumpRequested)
                {
                    velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
                }
            }

            // CHALLENGE! Allow some lateral movement when in air, but preserve momentum

            if (!jumpRequested)
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }

            TurnTowardsMovementDirection();
            collisionFlags = body.Move(velocity * Time.deltaTime);
            jumpRequested = false;
        }

        private void TurnTowardsMovementDirection()
        {
            Vector2 direction = new(velocity.x, velocity.z);
            if (direction.sqrMagnitude > 0f)
            {
                direction.Normalize();
                float targetYaw = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                Vector3 pitchYawRoll = transform.eulerAngles;
                pitchYawRoll.y = Mathf.MoveTowardsAngle(pitchYawRoll.y, targetYaw, turnSpeed * Time.deltaTime);
                transform.eulerAngles = pitchYawRoll;
            }
        }
    }
}
