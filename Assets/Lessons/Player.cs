using UnityEngine;

[SelectionBase, RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
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

    private void Awake()
    {
        body = GetComponent<CharacterController>();
    }

    private void Start()
    {
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // TODO - Lesson 2 - add Jump action

        // TODO - Lesson 2 - add Move action

        // TODO - Lesson X - TurnTowardsMovementDirection

        body.Move(velocity * Time.deltaTime);
    }
}

// TODO - Lesson 2 - create Extensions for ToVector3 and ToVector2
