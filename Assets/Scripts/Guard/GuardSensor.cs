using Utils;
using UnityEngine;


public class GuardSensor
{
    private readonly Guard guard;
    private readonly CountdownTimer timer;
    private readonly SphereCollider visionSphereCollider;
    private readonly Light spotlight;
    private readonly float viewAngle, viewDistance, chaseViewDistance;
    private Player detectedPlayer = null;
    private Color originalSpotlightColor;
    private bool lastVisibilityStatus = false, isPlayerVisible = false;

    public GuardSensor(
        Guard guard,
        float senseInterval,
        SphereCollider visionSphereCollider,
        float viewDistance,
        float chaseViewDistance,
        Light spotlight
    )
    {
        this.guard = guard;
        this.timer = new CountdownTimer(senseInterval);
        this.visionSphereCollider = visionSphereCollider;
        this.spotlight = spotlight;
        this.viewDistance = viewDistance;
        this.chaseViewDistance = chaseViewDistance;
        this.viewAngle = spotlight.spotAngle;
        this.originalSpotlightColor = spotlight.color;

        this.SetupSensor();
    }

    public void Update(float deltaTime) => timer.Tick(deltaTime);
    public bool InRangeOf(Vector3 position, float range) => Vector3.Distance(position, guard.transform.position) < range;
    public bool IsPlayerVisible() => detectedPlayer != null && isPlayerVisible;
    public void ChangeColor() => spotlight.color = isPlayerVisible ? Color.red : originalSpotlightColor;
    public bool InCatchRange() => detectedPlayer != null && InRangeOf(detectedPlayer.transform.position, guard.CatchDistance);
    public bool InAnomalyRange() => detectedPlayer != null && InRangeOf(detectedPlayer.transform.position, guard.AnomalyDistance);
    public void SetViewDistance(float viewDistance) => visionSphereCollider.radius = viewDistance;
    public Player GetDetectedPlayer() => detectedPlayer;


    // Private Methods

    private void SetupSensor()
    {
        visionSphereCollider.isTrigger = true;
        SetViewDistance(viewDistance);

        guard.OnTriggerEntered += HandleEnter;
        guard.OnTriggerStayed += HandleStay;
        guard.OnTriggerExited += HandleExit;

        timer.OnTimerStop += () =>
        {
            lastVisibilityStatus = isPlayerVisible;

            if (detectedPlayer != null) {
                isPlayerVisible = IsTargetVisible(detectedPlayer.transform);
            } else {
                isPlayerVisible = false;
            }

            if (isPlayerVisible != lastVisibilityStatus)
            {
                SetViewDistance(isPlayerVisible ? chaseViewDistance : viewDistance);
            }

            ChangeColor();
            timer.Start();
        };
        timer.Start();
    }

    private void HandleEnter(Collider other)
    {
        if ((Layers.TargetMask & (1 << other.gameObject.layer)) != 0)
        {
            detectedPlayer = other.GetComponent<Player>();
        }
    }

    private void HandleStay(Collider other)
    {
        if ((Layers.TargetMask & (1 << other.gameObject.layer)) != 0)
        {
            detectedPlayer = other.GetComponent<Player>();
        }
    }

    private void HandleExit(Collider other)
    {
        if (detectedPlayer != null && other.gameObject == detectedPlayer.gameObject)
        {
            detectedPlayer = null;
        }
    }

    private bool IsTargetVisible(Transform target)
    {
        if (target == null)
        {
            return false;
        }


        Vector3 dir = (target.transform.position - guard.transform.position).normalized;

        if (Vector3.Angle(guard.transform.forward, dir) < viewAngle / 2)
        {
            float dist = Vector3.Distance(guard.transform.position, target.transform.position);

            if (!Physics.Raycast(guard.transform.position, dir, dist, Layers.ObstacleMask))
            {
                return true;
            }
        }

        return false;
    }
}