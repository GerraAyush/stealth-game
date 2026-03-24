using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GuardMovement
{
    private Guard guard;
    private float movementSpeed;
    private Vector3 targetWaypoint, guardInitialPosition, guardInitialRotation;
    private Vector3[] waypoints;
    private bool movementDisabled;

    public GuardMovement(Guard guard, float movementSpeed, Transform pathHolder)
    {
        this.guard = guard;
        this.movementSpeed = movementSpeed;
        // this.waitTime = waitTime;
        // this.turnSpeed = turnSpeed;

        waypoints = new Vector3[pathHolder.childCount];
        for (int idx = 0; idx < waypoints.Length; idx++)
        {
            waypoints[idx] = pathHolder.GetChild(idx).position;
            waypoints[idx] = new Vector3(waypoints[idx].x, guard.transform.position.y, waypoints[idx].z);
        }

        guardInitialPosition = guard.transform.position;
        guardInitialRotation = guard.transform.eulerAngles;

        movementDisabled = true;
    }

    public void SetDestination(Vector3 targetWaypoint) => this.targetWaypoint = targetWaypoint;
    public float RemainingDistance => Vector3.Distance(targetWaypoint, guard.transform.position);
    public bool IsMovementDisabled() => movementDisabled;
    public void EnableMovement() => movementDisabled = false;
    public void DisableMovement() => movementDisabled = true;
    public int WaypointCount => waypoints.Length;
    public Vector3? GetTargetWaypoint(int targetWaypointIndex) => (targetWaypointIndex >= 0 && targetWaypointIndex < waypoints.Length) ? waypoints[targetWaypointIndex] : null;

    public void LookAt()
    {
        if (!movementDisabled)
            guard.transform.LookAt(targetWaypoint);
    }

    public void LookAt(Vector3 lookDirection)
    {
        if (!movementDisabled)
            guard.transform.LookAt(lookDirection);
    }

    public void Reset()
    {
        guard.transform.position = guardInitialPosition;
        guard.transform.rotation = Quaternion.Euler(guardInitialRotation);
    }

    public void MoveStep()
    {
        targetWaypoint.y = guard.transform.position.y;

        if (!movementDisabled) {
            guard.transform.position = Vector3.MoveTowards(
                guard.transform.position, 
                targetWaypoint, 
                movementSpeed * Time.deltaTime
            );
        }
    }

}