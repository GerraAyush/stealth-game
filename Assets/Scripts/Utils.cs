using System;
using UnityEngine;

namespace Utils
{

    public static class Layers {
        public const string Player = "Player";
        public const string Obstacle = "Obstacles";
        
        public static int TargetMask => LayerMask.GetMask(Player);
        public static int ObstacleMask => LayerMask.GetMask(Obstacle);
    }

    public class CountdownTimer
    {
        float timerInterval;
        float timeRemaining;
        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        public CountdownTimer(float timerInterval)
        {
            this.timerInterval = timerInterval;
        }

        public void Start()
        {
            timeRemaining = timerInterval;
            OnTimerStart.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (timeRemaining <= 0) return;

            timeRemaining -= deltaTime;

            if (timeRemaining <= 0)
            {
                OnTimerStop.Invoke();
            }
        }
    }
    
}