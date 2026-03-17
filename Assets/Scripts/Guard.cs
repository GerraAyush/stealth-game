using System.Collections;
using UnityEngine;


public class Guard : MonoBehaviour {
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waitTime = 0.3f;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private Light spotlight;
    [SerializeField] private float viewDistance;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    private SphereCollider sphereCollider;
    private Color originalSpotlightColor;
    private Transform playerInRange;
    private bool isPlayerVisible;
    private float viewAngle;

    void Start() {
        if (spotlight) {
            viewAngle = spotlight.spotAngle;
            originalSpotlightColor = spotlight.color;
        }

        sphereCollider = GetComponentInChildren<SphereCollider>();
        sphereCollider.radius = viewDistance;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int idx = 0; idx < waypoints.Length; idx++) {
            waypoints[idx] = pathHolder.GetChild(idx).position;
            waypoints[idx] = new Vector3(waypoints[idx].x, transform.position.y, waypoints[idx].z);
        }

        StartCoroutine(FollowPath(waypoints));
        StartCoroutine(CheckGuardVision());
    }

    bool CanSeePlayer(Transform target) {
        Vector3 dir = (target.transform.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dir) < viewAngle / 2)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            Debug.Log(Physics.Raycast(transform.position, dir, dist, obstacleMask));
            if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
            {
                Debug.Log(isPlayerVisible);
                return true;
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints) {
        transform.position = waypoints[0];

        int targetWaypointIndex = 0;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);
        while (true) {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if (transform.position == targetWaypoint) {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnHead(targetWaypoint));
            }
            yield return null;
        }
    }

    IEnumerator TurnHead(Vector3 targetWaypoint) {
        Vector3 directionToLook = (targetWaypoint - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(directionToLook.z, directionToLook.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f) {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    IEnumerator CheckGuardVision() {
        while (true) {
            if (playerInRange != null) {
                isPlayerVisible = CanSeePlayer(playerInRange);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    void Update() {
        Color targetColor = isPlayerVisible ? Color.red : originalSpotlightColor;

        if (spotlight.color != targetColor) {
            spotlight.color = targetColor;
        }
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("Triggered by: " + other.name);
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = other.transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.name);
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Triggered by: " + other.name);
        if ((targetMask & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = null;
            isPlayerVisible = false;
        }
    }

    void OnDrawGizmos()
    {
        if (pathHolder == null) {
            return;
        }

        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3? previousPosition = null;

        foreach(Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position, .3f);
            if (previousPosition.HasValue) {
                Gizmos.DrawLine(previousPosition.Value, waypoint.position);
            }
            previousPosition = waypoint.position;
        }

        if (previousPosition.HasValue) {
            Gizmos.DrawLine(previousPosition.Value, startPosition);
        }

        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, transform.position + viewDistance * Vector3.forward);
    }
}