using System;
using UnityEngine;


public class Guard : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    // [SerializeField] private float waitTime = 0.2f;
    // [SerializeField] private float turnSpeed = 50f;
    [SerializeField] private float catchDistance = 1f;
    [SerializeField] private float anomalyDistance = 1f;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private float viewDistance;
    [SerializeField] private float chaseViewDistance;
    [SerializeField] private float senseGap;
    public float CatchDistance { get; set; }
    public float AnomalyDistance { get; set; }

    private GuardSensor guardSensor;
    private GuardMovement guardMovement;
    private GuardStateMachine guardStateMachine;

    public Action<Collider> OnTriggerEntered;
    public Action<Collider> OnTriggerStayed;
    public Action<Collider> OnTriggerExited;
    
    public void EnableMovement() => guardMovement.EnableMovement();
    public void DisableMovement() => guardMovement.DisableMovement();
    public void Reset() => guardMovement.Reset();
    

    // Lifecycle Methods

    void Start()
    {
        guardSensor = new GuardSensor(
            this, 
            senseGap, 
            GetComponentInChildren<SphereCollider>(), 
            viewDistance,
            chaseViewDistance,
            GetComponentInChildren<Light>()
        );

        guardMovement = new GuardMovement(
            this,
            speed,
            // waitTime,
            // turnSpeed,
            pathHolder
        );

        guardStateMachine = new GuardStateMachine(
            guardSensor,
            guardMovement
        );
    }

    void Update() {
        if (GameManager.Instance.IsGameOn) {
            CatchDistance = catchDistance;
            AnomalyDistance = anomalyDistance;

            guardSensor.Update(Time.deltaTime);
            guardStateMachine.Update();
        }
    }

    void OnTriggerEnter(Collider other) => OnTriggerEntered.Invoke(other);
    void OnTriggerStay(Collider other) => OnTriggerStayed.Invoke(other);
    void OnTriggerExit(Collider other) => OnTriggerExited.Invoke(other);

    void OnDrawGizmos()
    {
        if (pathHolder == null)
        {
            return;
        }

        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3? previousPosition = null;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);

            if (previousPosition.HasValue)
            {
                Gizmos.DrawLine(previousPosition.Value, waypoint.position);
            }

            previousPosition = waypoint.position;
        }

        if (previousPosition.HasValue)
        {
            Gizmos.DrawLine(previousPosition.Value, startPosition);
        }

        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, transform.position + viewDistance * Vector3.forward);
    }
}