using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

public static class Utils
{
    public class Timer
    {
        private float cooldown;
        private float timer;

        public Timer(float cooldown)
        {
            this.cooldown = cooldown;
            Reset();
        }
        
        public void Reset(float newCooldown = -1f) => timer = newCooldown == -1f ? cooldown : newCooldown;

        public bool IsDone() => timer <= 0;
        
        public void Decrement() => timer -= Time.deltaTime;
    }
}