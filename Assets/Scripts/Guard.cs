using System.Collections;
using UnityEngine;
using Enumerators;


public class Guard : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private float turnSpeed = 50f;
    [SerializeField] private float catchDistance = 1f;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private Light spotlight;
    [SerializeField] private float viewDistance;
    [SerializeField] private float chaseViewDistance;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private Coroutine currentStateRoutine, currentMovementRoutine, currentTurnRoutine;

    private Player chaseTarget, detectedPlayer;
    private GuardManager guardManager;

    private SphereCollider visionSphereCollider;

    private GuardState currentGuardPhase;

    private bool disabled = true;
    private bool isPlayerVisible = false;
    private bool lastVisibilityState = true;

    private int targetWaypointIndex;
    private float viewAngle;
    private float finalViewDistance;

    private Color originalSpotlightColor;

    private Vector3[] waypoints;
    private Vector3 guardInitialPosition, guardInitialRotation;


    // Public Methods

    public void Chase(Player target)
    {
        if (target == null) return;
        if (currentGuardPhase == GuardState.Chase && chaseTarget == target) return;

        chaseTarget = target;
        currentGuardPhase = GuardState.Chase;
        spotlight.color = Color.red;
        finalViewDistance = chaseViewDistance;

        StopCurrentRoutine();
        currentStateRoutine = StartCoroutine(ChaseRoutine(target.transform));
    }

    public void Patrol()
    {
        if (currentGuardPhase == GuardState.Patrol) return;

        chaseTarget = null; // FIX
        currentGuardPhase = GuardState.Patrol;
        spotlight.color = originalSpotlightColor;
        finalViewDistance = viewDistance;

        StopCurrentRoutine();
        currentStateRoutine = StartCoroutine(PatrolRoutine());
    }

    public void Reset()
    {
        transform.position = guardInitialPosition;
        transform.rotation = Quaternion.Euler(guardInitialRotation);
        currentGuardPhase = GuardState.Thinking;
        chaseTarget = null;
    }

    public GuardState GetGuardState()
    {
        return currentGuardPhase;
    }

    public Player GetTargetPlayer()
    {
        return chaseTarget;
    }

    public void EnableMovement()
    {
        disabled = false;
    }

    public void DisableMovement()
    {
        disabled = true;
    }


    // Private Methods

    private IEnumerator ChaseRoutine(Transform target)
    {
        while (!disabled && currentGuardPhase == GuardState.Chase && target != null)
        {
            currentMovementRoutine = StartCoroutine(ToWaypoint(target.position, true));
            yield return currentMovementRoutine;

            if (Vector3.Distance(transform.position, target.position) < catchDistance)
            {
                guardManager.notify(this, null, GuardNotificationMessage.Player_Caught);
            }

            yield return null;
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (!disabled && currentGuardPhase == GuardState.Patrol)
        {
            Vector3 waypoint = waypoints[targetWaypointIndex];

            currentMovementRoutine = StartCoroutine(ToWaypoint(waypoint, false));
            yield return currentMovementRoutine;

            targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
        }
    }

    private IEnumerator ToWaypoint(Vector3 targetWaypoint, bool fastTurn)
    {
        if (fastTurn)
        {
            transform.LookAt(targetWaypoint);
        }
        else
        {
            currentTurnRoutine = StartCoroutine(TurnHead(targetWaypoint));
            yield return currentTurnRoutine;
        }

        while (Vector3.Distance(transform.position, targetWaypoint) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWaypoint,
                speed * Time.deltaTime
            );

            yield return null;
        }
    }

    private IEnumerator TurnHead(Vector3 targetWaypoint)
    {
        Vector3 directionToLook = (targetWaypoint - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(directionToLook.z, directionToLook.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    private bool CanSeePlayer(Transform target)
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;

        if (Vector3.Angle(transform.forward, dir) < viewAngle / 2)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator GuardVision()
    {
        while (true)
        {
            isPlayerVisible = (detectedPlayer != null) && CanSeePlayer(detectedPlayer.transform);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void DetectPlayer(Collider other, bool exit)
    {
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            Player player = other.GetComponent<Player>();

            if (exit)
            {
                if (detectedPlayer == player)
                    detectedPlayer = null;
            }
            else
            {
                detectedPlayer = player;
            }
        }
    }

    private void StopCurrentRoutine()
    {
        if (currentStateRoutine != null)
            StopCoroutine(currentStateRoutine);

        if (currentMovementRoutine != null)
            StopCoroutine(currentMovementRoutine);

        if (currentTurnRoutine != null)
            StopCoroutine(currentTurnRoutine);
    }


    // Lifecycle Methods

    void Start()
    {
        Debug.Log("Guard Start");

        if (pathHolder == null)
            Debug.LogError("pathHolder is NULL");
        else
        {
            waypoints = new Vector3[pathHolder.childCount];

            for (int idx = 0; idx < waypoints.Length; idx++)
            {
                waypoints[idx] = pathHolder.GetChild(idx).position;
                waypoints[idx] = new Vector3(waypoints[idx].x, transform.position.y, waypoints[idx].z);
            }
        }

        if (spotlight == null)
            Debug.LogError("spotlight is NULL");
        else
        {
            viewAngle = spotlight.spotAngle;
            originalSpotlightColor = spotlight.color;
        }

        GameObject gm = GameObject.Find("GuardManager");
        if (gm == null)
            Debug.LogError("GuardManager object not found");

        guardManager = gm.GetComponent<GuardManager>();
        guardManager.RegisterGuard(this);

        visionSphereCollider = GetComponentInChildren<SphereCollider>();
        if (visionSphereCollider == null)
            Debug.LogError("SphereCollider not found");

        finalViewDistance = viewDistance;

        StartCoroutine(GuardVision());

        guardInitialPosition = transform.position;
        guardInitialRotation = transform.eulerAngles;
    }

    void Update()
    {
        visionSphereCollider.radius = finalViewDistance;

        if (!disabled)
        {
            if (isPlayerVisible != lastVisibilityState)
            {
                guardManager.notify(
                    this,
                    detectedPlayer,
                    isPlayerVisible
                        ? GuardNotificationMessage.Player_Visible
                        : GuardNotificationMessage.Player_Not_Visible
                );

                lastVisibilityState = isPlayerVisible;
            }
        }
        
        else if (isPlayerVisible && detectedPlayer != null)
        {
            guardManager.notify(
                this,
                detectedPlayer,
                GuardNotificationMessage.Player_Visible
            );
        }
    }

    void OnTriggerStay(Collider other)
    {
        DetectPlayer(other, false);
    }

    void OnTriggerEnter(Collider other)
    {
        DetectPlayer(other, false);
    }

    void OnTriggerExit(Collider other)
    {
        DetectPlayer(other, true);
    }

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