using System;
using UnityEngine;

// TODO: spaceship melting near sun

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    
    [field: SerializeField] public PrefabAtlas prefabs {get; private set;}
    
    public PlayerController player {get; private set;}
    public WorldMenu worldMenu {get; private set;}
    
    private void Awake()
    {
        if (I == null)
            I = this;
        else
            Debug.LogError("More than one instance of GameManager");
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        player = FindAnyObjectByType<PlayerController>();
        worldMenu = FindAnyObjectByType<WorldMenu>();
    }
}

public static class Utils
{
    // TODO: add class that has a current value and a max value, useful for e.g. health (no need for two variable)
    
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

    public static Vector3 ResizeVector(Vector3 vectorToResize, float magnitude)
    {
        return vectorToResize.normalized * magnitude;
    }
    
    public static Vector3 ExtendVector(Vector3 vectorToExtend, float addedMagnitude)
    {
        return vectorToExtend.normalized * (vectorToExtend.magnitude + addedMagnitude);
    }

    public static bool RandomEventInTime(float ratePerMinute)
    {
        return UnityEngine.Random.value < ratePerMinute / 60 * Time.deltaTime;
    }

    public static bool IsNull<T>(T obj)
    {
        return obj == null || obj.Equals(null);
    }
}