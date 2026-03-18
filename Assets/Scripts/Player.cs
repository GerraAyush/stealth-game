using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 50f;
    [SerializeField] private float smoothMoveTime = 0.1f;
    [SerializeField] private float turnSpeed = 50f;
    private Vector3 initialPlayerPosition, initialPlayerRotation;
    private bool movementDisabled;
    private float angle, smoothMoveVelocity, smoothInputMagnitude;
    private InputAction moveAction;
    private Vector3 velocity;
    private Rigidbody rb;

    void Start()
    {
        // Initialize Polling InputSystem
        moveAction = InputSystem.actions.FindAction("Move");

        // Initialize components
        rb = GetComponent<Rigidbody>();
        initialPlayerPosition = transform.position;
        initialPlayerRotation = transform.eulerAngles;
        movementDisabled = true;
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

    public void Caught() {
        movementDisabled = true;
    }

    public void Reset()
    {
        transform.position = initialPlayerPosition;
        transform.rotation = Quaternion.Euler(initialPlayerRotation);
        angle = transform.eulerAngles.y;
        movementDisabled = false;
    }
}
