using System.Collections.Generic;
using UnityEngine;
using Enumerators;

public class GuardManager : MonoBehaviour
{

    [SerializeField] private GameManager gameManager;
    [SerializeField] private float guardProximityDistance;
    [SerializeField] private float anamolyProximityDistance;

    private readonly List<Guard> guards = new List<Guard>();
    private readonly Dictionary<Player, List<Guard>> chaseDict = new Dictionary<Player, List<Guard>>();


    // Private Constructor

    private GuardManager() { }


    // Public Methods

    public void RegisterGuard(Guard guard)
    {
        if (!guards.Contains(guard))
            guards.Add(guard);
    }

    public void UnregisterGuard(Guard guard)
    {
        guards.Remove(guard);
    }

    public void notify(Guard guard, GuardNotificationMessage notificationMessage)
    {
        notify(guard, null, notificationMessage);
    }

    public void notify(Guard guard, Player player, GuardNotificationMessage notificationMessage)
    {

        switch (notificationMessage)
        {

            case GuardNotificationMessage.Player_Visible:
                Debug.Log("Guard says player visible");

                if (player == null) return;

                if (guard.GetGuardState() != GuardState.Chase)
                {
                    PutChase(guard, player);
                    guard.Chase(player);
                }
                else if (!GameObject.Equals(guard.GetTargetPlayer(), player))
                {
                    RemoveChase(guard, guard.GetTargetPlayer());
                    PutChase(guard, player);
                    guard.Chase(player);
                }

                gameManager.notify(GameState.Player_Being_Chased);

                break;

            case GuardNotificationMessage.Player_Not_Visible:
                Debug.Log("Guard says player not visible");

                Vector3 guardPosition = guard.transform.position;
                Player playerChasing = guard.GetTargetPlayer();

                if (playerChasing == null)
                {
                    guard.Patrol();
                    return;
                }

                bool shouldContinueChase = false;

                if (chaseDict.ContainsKey(playerChasing))
                {
                    foreach (Guard otherGuard in chaseDict[playerChasing])
                    {
                        if (otherGuard == guard) continue;

                        if (
                            Vector3.Distance(guardPosition, otherGuard.transform.position) < guardProximityDistance ||
                            Vector3.Distance(guardPosition, playerChasing.transform.position) < anamolyProximityDistance
                        )
                        {
                            shouldContinueChase = true;
                            break;
                        }
                    }
                }

                if (shouldContinueChase)
                {
                    guard.Chase(playerChasing);
                    return;
                }

                RemoveChase(guard, playerChasing);

                if (chaseDict.ContainsKey(playerChasing) && chaseDict[playerChasing].Count == 0)
                {
                    chaseDict.Remove(playerChasing);
                    gameManager.notify(GameState.Player_Juked_Chased);
                }

                guard.Patrol();

                break;

            case GuardNotificationMessage.Player_Caught:
                gameManager.notify(GameState.Player_Got_Caught);
                break;
        }
    }

    public void ResetGuards()
    {
        foreach (Guard guard in guards)
        {
            guard.Reset();
        }
    }

    public void EnableGuardMovements()
    {
        foreach (Guard guard in guards)
        {
            guard.EnableMovement();
        }
    }

    public void DisableGuardMovements()
    {
        foreach (Guard guard in guards)
        {
            guard.DisableMovement();
        }
    }


    // Private Methods

    private void PutChase(Guard guard, Player player)
    {
        if (player == null) return;

        if (chaseDict.ContainsKey(player))
        {
            if (!chaseDict[player].Contains(guard))
                chaseDict[player].Add(guard);

            return;
        }

        List<Guard> guardList = new List<Guard>
        {
            guard
        };

        chaseDict.Add(player, guardList);
    }

    private void RemoveChase(Guard guard, Player player)
    {
        if (player == null) return;
        if (!chaseDict.ContainsKey(player)) return;

        chaseDict[player].Remove(guard);
    }
}