using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase, RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, PlayerControls_Lesson.IGameplayActions
{
    [SerializeField, Range(0f, 10f), Tooltip("Maximum speed when running")]
    float runSpeed = 4f;

    [SerializeField, Range(0f, 5f)]
    float normalJumpHeight = 1f;

    [SerializeField, Range(1f, 1800f), Tooltip("Maximum turning speed (degrees per second)")]
    float turnSpeed = 360f;

    private CharacterController body;
    private Vector3 velocity;
    private bool jumpRequested;
    private Camera cam;
    private PlayerControls_Lesson controls;

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && !jumpRequested)
        {
            jumpRequested = body.isGrounded;
        }
    }

    private void Awake()
    {
        body = GetComponent<CharacterController>();
        controls = new PlayerControls_Lesson();
    }

    private void Start()
    {
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        controls.gameplay.SetCallbacks(this);
        controls.Enable();
    }

    private void OnDisable()
    {
        // TODO - fix errors next time
        controls.gameplay.RemoveCallbacks(this);
        controls.Disable();
    }

    private void Update()
    {
        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * normalJumpHeight);
            jumpRequested = false;
        }
        else
        {
            float g = Physics.gravity.y * Time.deltaTime;
            velocity.y += g;
        }

        // TODO - Lesson 2 - add Move action

        // TODO - Lesson X - TurnTowardsMovementDirection

        body.Move(velocity * Time.deltaTime);
    }
}

// TODO - Lesson 2 - create Extensions for ToVector3 and ToVector2
