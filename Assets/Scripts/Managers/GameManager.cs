using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

// TODO: spaceship melting near sun

public class GameManager : MonoBehaviour
{
    public static GameManager I {get; private set;}

    [field: Header("References")]
    [field: SerializeField] public PrefabAtlas prefabs {get; private set;}
    [field: SerializeField] public UIPopupListHandler popupListHandler {get; private set;}
    [field: SerializeField] public EnemyWaveManager waveManager {get; private set;}
    [field: SerializeField] public HierarchyManager hierarchyManager {get; private set;}
    [field: SerializeField] public CameraManager cameraManager {get; private set;}
    [field: SerializeField] public new AudioManager audio {get; private set;}
    [field: SerializeField] public FloatingOrigin floatingOrigin {get; private set;}
    [field: SerializeField] public SpaceshipTracker spaceshipTracker {get; private set;}
    
    public InputSystem_Actions input {get; private set;}
    public PlayerController player {get; private set;}
    public WorldMenu worldMenu {get; private set;}

    public Action OnGamePaused;
    public Action OnGameUnpaused;
    public Action OnPlayerDeath;
    
    public bool gamePaused {get; private set;}
    
    private void Awake()
    {
        if (I == null)
            I = this;
        else
        {
            Debug.LogError("More than one instance of GameManager");
            Destroy(this);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        ObjectPoolManager.RemoveDestroyedPools();
        
        player = FindAnyObjectByType<PlayerController>();
        worldMenu = FindAnyObjectByType<WorldMenu>();
        
        input = new InputSystem_Actions();
        input.Enable();
        input.World.Enable();
        input.Player.Enable();
        input.UI.Enable();
        
        player.gameObject.SetActive(false);
        waveManager.enabled = false;
    }

    private void OnDestroy()
    {
        input.Disable();
        input.World.Disable();
        input.Player.Disable();
        input.Cargoship.Disable();
        input.UI.Disable();
        input.Dispose();
        I = null;
    }

    public void StartGame()
    {
        player.gameObject.SetActive(true);
        waveManager.enabled = true;
        worldMenu.EnableObjectEnabling();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        gamePaused = true;
        OnGamePaused?.Invoke();
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        gamePaused = false;
        OnGameUnpaused?.Invoke();
    }

    public void ExitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void PlayerDied()
    {
        OnPlayerDeath?.Invoke();
    }
    
    public Vector3 ToOriginalPosition(Vector3 transformDotPosition, bool isMovedByFloatingOrigin)
    {
        if (isMovedByFloatingOrigin)
            return ToOriginalPosition(transformDotPosition);
        else
            return transformDotPosition;
    }

    /// Converts the actual position, which is altered by FloatingOrigin, into the position the object would have without FloatingOrigin acting on the object.
    public Vector3 ToOriginalPosition(Vector3 transformDotPosition)
    {
        return transformDotPosition - floatingOrigin.totalPositionOffset;
    }

    /// Opposite of "ToOriginalPosition". Returns the same value as "Transform.position" on an object the FloatingOrigin is moving.
    public Vector3 ToActualPosition(Vector3 position)
    {
        return position + floatingOrigin.totalPositionOffset;
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

    public enum TextTypingAnimationType
    {
        typing = 0,
        deleting = 1,
    }

    public static IEnumerator TextTypingAnimation(TextMeshProUGUI uiText, string text, float characterCooldown, TextTypingAnimationType type)
    {
        if (type == TextTypingAnimationType.typing)
        {
            uiText.text = "";

            for (int i = 1; i <= text.Length; i++)
            {
                uiText.text = text.Substring(0, i);
                yield return new WaitForSeconds(characterCooldown);
            }
        }
        else
        {
            uiText.text = text;

            for (int i = text.Length - 1; i >= 0; i--)
            {
                uiText.text = text.Substring(0, i);
                yield return new WaitForSeconds(characterCooldown);
            }
        }
    }

    public static bool IsInRange(float value, Vector2 range)
    {
        return IsInRange(value, range.x, range.y);
    }

    public static bool IsInRange(float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    
    public static bool IsInArrayRange(float value, Array array)
    {
        return value >= 0 && value < array.Length;
    }
}