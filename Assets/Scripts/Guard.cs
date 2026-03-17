using System.Collections;
using UnityEngine;
using Enumerators;


public class Guard : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private float turnSpeed = 50f;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private Light spotlight;
    [SerializeField] private float viewDistance;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    private SphereCollider sphereCollider;
    private Color originalSpotlightColor;
    private Player playerInRange;
    private bool isPlayerVisible, isPatrolling, isChasing;
    private float viewAngle;
    private GuardPhase currentGuardPhase;
    private Vector3[] waypoints;

    bool CanSeePlayer(Transform target)
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

    IEnumerator FollowPath()
    {
        isPatrolling = true;
        isChasing = false;
        transform.position = waypoints[0];

        int targetWaypointIndex = 0;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);
        while (currentGuardPhase == GuardPhase.Patrol)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnHead(targetWaypoint));
            }
            yield return null;
        }
    }

    IEnumerator FollowPlayer() {
        isChasing = true;
        isPatrolling = false;

        Transform playerTransform = playerInRange.transform;
        while (currentGuardPhase == GuardPhase.Chase) {
            transform.LookAt(playerTransform.position);
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, playerTransform.position) < 1.5f) {
                playerInRange.Caught();
                currentGuardPhase = GuardPhase.Stop;
            }
            yield return null;
        }
    }

    IEnumerator TurnHead(Vector3 targetWaypoint)
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

    IEnumerator CheckGuardVision()
    {
        while (true)
        {
            if (playerInRange != null)
            {
                isPlayerVisible = CanSeePlayer(playerInRange.transform);
                if (currentGuardPhase != GuardPhase.Stop) {
                    currentGuardPhase = isPlayerVisible ? GuardPhase.Chase : GuardPhase.Patrol;
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    void Start()
    {
        if (spotlight)
        {
            viewAngle = spotlight.spotAngle;
            originalSpotlightColor = spotlight.color;
        }

        sphereCollider = GetComponentInChildren<SphereCollider>();
        sphereCollider.radius = viewDistance;

        waypoints = new Vector3[pathHolder.childCount];
        for (int idx = 0; idx < waypoints.Length; idx++)
        {
            waypoints[idx] = pathHolder.GetChild(idx).position;
            waypoints[idx] = new Vector3(waypoints[idx].x, transform.position.y, waypoints[idx].z);
        }

        StartCoroutine(FollowPath());
        StartCoroutine(CheckGuardVision());
    }

    void Update()
    {
        Color targetColor = isPlayerVisible ? Color.red : originalSpotlightColor;

        switch(currentGuardPhase) {

            case GuardPhase.Patrol:
                if (!isPatrolling) {
                    StartCoroutine(FollowPath());
                }
                break;

            case GuardPhase.Chase:
                if (!isChasing) {
                    StartCoroutine(FollowPlayer());
                }
                break;

        }

        if (spotlight.color != targetColor)
        {
            spotlight.color = targetColor;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = other.GetComponent<Player>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = other.GetComponent<Player>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = null;
            isPlayerVisible = false;
        }
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