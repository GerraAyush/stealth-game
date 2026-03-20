using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 50f;
    [SerializeField] private float smoothMoveTime = 0.1f;
    [SerializeField] private float turnSpeed = 50f;
    private Vector3 initialPlayerPosition, initialPlayerRotation;
    private bool movementDisabled, beingChased;
    private float angle, smoothMoveVelocity, smoothInputMagnitude;
    private InputAction moveAction;
    private Vector3 velocity;
    private Rigidbody rb;


    // Public Methods

    public void DisableMovement() {
        movementDisabled = true;
    }

    public void EnableMovement() {
        movementDisabled = false;
    }

    public void Reset()
    {
        transform.position = initialPlayerPosition;
        transform.rotation = Quaternion.Euler(initialPlayerRotation);
        angle = transform.eulerAngles.y;
    }

    public bool IsBeingChased() {
        return beingChased;
    }

    public void SetBeingChased() {
        beingChased = true;
    }

    public void UnSetBeingChased() {
        beingChased = false;
    }


    // Lifecycle Methods

    void Start()
    {
        // Initialize Polling InputSystem
        moveAction = InputSystem.actions.FindAction("Move");

        // Initialize components
        rb = GetComponent<Rigidbody>();
        initialPlayerPosition = transform.position;
        initialPlayerRotation = transform.eulerAngles;
        movementDisabled = true;
        beingChased = false;
    }

    void Update()
    {
        if (!movementDisabled) {
            Vector2 inputDir = moveAction.ReadValue<Vector2>();

            // Horizontal change => For change in angle
            float turnAngle = inputDir.x;
            
            // Vertical change => For moving the player in forward or backward direction
            float moveMagnitude = inputDir.y;

            // Smooth movement
            smoothInputMagnitude = Mathf.SmoothDamp(
                smoothInputMagnitude,
                moveMagnitude,
                ref smoothMoveVelocity,
                smoothMoveTime
            );

            // Rotate only when moving => No instant 90deg turns
            if (Mathf.Abs(smoothInputMagnitude) > 0.1f)
            {
                float turnAmount = turnAngle * turnSpeed * Time.deltaTime * smoothInputMagnitude;
                angle += turnAmount;
            }

            // Movement is forward or backward only
            velocity = movementSpeed * smoothInputMagnitude * transform.forward * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (!movementDisabled) {
            // Perform motions using Rigidbody
            rb.MoveRotation(Quaternion.Euler(Vector3.up * angle));
            rb.MovePosition(rb.position + velocity);
        }
    }
}
