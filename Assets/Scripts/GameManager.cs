using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

// TODO: spaceship melting near sun

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    
    [field: SerializeField] public PrefabAtlas prefabs {get; private set;}
    [field: SerializeField] public AudioMixerGroup SFXAudioMixerGroup {get; private set;}
    [field: SerializeField] public AudioMixerGroup ambienceAudioMixerGroup {get; private set;}
    [field: SerializeField] public UIPopupListHandler popupListHandler {get; private set;}
    [field: SerializeField] public EnemyWaveManager waveManager {get; private set;}
    
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
    
    [System.Serializable]
    public class Timer
    {
        private float time;
        private float counter;
        private bool returnedIsDoneOnce;

        public float Counter => counter;

        public Timer(float time)
        {
            this.time = time;
            Reset();
        }

        public void Reset(float newTime = -1f)
        {
            counter = newTime == -1f ? time : newTime;
            returnedIsDoneOnce = false;
        }

        public bool IsDoneOnce()
        {
            if (!returnedIsDoneOnce && IsDone())
            {
                returnedIsDoneOnce = true;
                return true;
            }
            
            return false;
        }

        public bool IsDone() => counter <= 0;
        
        public void Decrement() => counter -= Time.deltaTime;
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

    public static bool RandomBool()
    {
        return UnityEngine.Random.value < 0.5f;
    }

    public static Vector3 RandomPointInSphere(Vector3 sphereCenter, float radius)
    {
        Vector3 randomPointInCube = RandomPointInCube(sphereCenter, radius*2);

        if (Vector3.Distance(randomPointInCube, sphereCenter) < radius)
            return randomPointInCube;
        
        return RandomPointInSphere(sphereCenter, radius);
    }
    
    public static Vector3 RandomPointInCube(Vector3 cubeCenter, float edge)
    {
        return cubeCenter + new Vector3(Random.Range(-edge/2, edge/2), Random.Range(-edge/2, edge/2), Random.Range(-edge/2, edge/2));
    }
}