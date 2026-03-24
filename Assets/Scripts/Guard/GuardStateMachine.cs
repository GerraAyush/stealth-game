using BehaviourTrees;
using System;

public class GuardStateMachine {

    private BehaviourTree tree;
    private GuardSensor sensor;
    private GuardMovement movement;

    public GuardStateMachine(GuardSensor sensor, GuardMovement movement) {
        this.sensor = sensor;
        this.movement = movement;
        SetupBehaviorTree();
    }

    public void Update() {
        tree.Process();
    }

    private void SetupBehaviorTree() {
        tree = new BehaviourTree("Guard");

        PrioitySelector patrolOrChase = new PrioitySelector("PatrolOrChase");

        Sequence catchPlayer = new Sequence("Catch");
        catchPlayer.AddChild(new Leaf("InCatchRange", new Condition(sensor.InCatchRange)));
        catchPlayer.AddChild(new Leaf("CatchPlayer", new CatchStrategy(sensor, movement)));
        catchPlayer.Priority = 10;

        Sequence anomalyChase = new Sequence("PlayerInAnomalyRange");
        bool Condition() => sensor.InAnomalyRange() && (GameManager.Instance.CountOfChasers > 0);
        anomalyChase.AddChild(new Leaf("InAnomalyRange", new Condition(Condition)));
        anomalyChase.AddChild(new Leaf("ChasePlayer", new ChaseStrategy(sensor, movement)));
        anomalyChase.Priority = 5;

        Sequence visibilityChase = new Sequence("PlayerIsVisible");
        visibilityChase.AddChild(new Leaf("InVision", new Condition(sensor.IsPlayerVisible)));
        visibilityChase.AddChild(new Leaf("ChasePlayer", new ChaseStrategy(sensor, movement)));
        visibilityChase.Priority = 5;

        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(movement));

        patrolOrChase.AddChild(catchPlayer);
        patrolOrChase.AddChild(visibilityChase);
        patrolOrChase.AddChild(anomalyChase);
        patrolOrChase.AddChild(patrol);

        tree.AddChild(patrolOrChase);
    }
}