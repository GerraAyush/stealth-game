using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Enumerators;

namespace BehaviourTrees
{

    public interface IStrategy
    {
        Node.Status Process();
        void Reset()
        {
            // Noop
        }
    }

    public class ActionStrategy : IStrategy
    {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething)
        {
            this.doSomething = doSomething;
        }

        public Node.Status Process()
        {
            doSomething();
            return Node.Status.Success;
        }
    }

    public class Condition : IStrategy
    {
        readonly Func<bool> predicate;

        public Condition(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class PatrolStrategy : IStrategy
    {
        private int targetWaypointIndex = 0;
        readonly GuardMovement guardMovement;

        public PatrolStrategy(GuardMovement guardMovement)
        {
            this.guardMovement = guardMovement;
        }

        public Node.Status Process()
        {
            Vector3? target = guardMovement.GetTargetWaypoint(targetWaypointIndex);
            guardMovement.SetDestination(target.Value);

            guardMovement.LookAt();
            guardMovement.MoveStep();

            if (guardMovement.RemainingDistance < 0.1f)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % guardMovement.WaypointCount;
            }

            return Node.Status.Running;
        }

        public void Reset() => targetWaypointIndex = 0;
    }

    public class ChaseStrategy : IStrategy
    {
        private GuardSensor guardSensor;
        private GuardMovement guardMovement;
        private bool isChasing = false;

        public ChaseStrategy(GuardSensor guardSensor, GuardMovement guardMovement)
        {
            this.guardSensor = guardSensor;
            this.guardMovement = guardMovement;
        }

        public Node.Status Process()
        {
            Player target = guardSensor.GetDetectedPlayer();

            if (target == null) return Node.Status.Failure;

            guardMovement.EnableMovement();
            guardMovement.SetDestination(target.transform.position);

            if (!isChasing)
            {
                isChasing = true;
                GameManager.Instance.Notify(GameState.Player_Being_Chased);
            }

            guardMovement.LookAt();
            guardMovement.MoveStep();

            return Node.Status.Running;
        }

        public void Reset()
        {
            if (isChasing)
            {
                isChasing = false;
                GameManager.Instance.Notify(GameState.Player_Juked_Chased);
            }
        }
    }

    public class CatchStrategy : IStrategy
    {
        private GuardSensor guardSensor;
        private GuardMovement guardMovement;

        public CatchStrategy(GuardSensor guardSensor, GuardMovement guardMovement)
        {
            this.guardSensor = guardSensor;
            this.guardMovement = guardMovement;
        }

        public Node.Status Process()
        {
            if (guardSensor.InCatchRange())
            {
                guardMovement.DisableMovement();
                GameManager.Instance.Notify(GameState.Player_Got_Caught);
                return Node.Status.Success;
            }

            return Node.Status.Failure;
        }

        public void Reset()
        {
            guardMovement.EnableMovement();
        }

    }
}
