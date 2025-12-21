using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WorldMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeInPanelPrefab;
    [field: SerializeField] public Transform criticalSpeedWarning {get; private set;}
    [field: SerializeField] public Transform repairKeyPopup {get; private set;}
    [field: SerializeField] public Transform menu {get; private set;}
    [field: SerializeField] public CanvasGroup deathScreen {get; private set;}
    [field: SerializeField] public TextMeshProUGUI deathScreenText {get; private set;}
    [field: SerializeField] public float deathScreenFadeDuration {get; private set;}

    private GameObject currentMenuItem;
    private bool canEnableObjects;
    private AudioSource[] fetchedAudioSources;
    private bool lastCurrentMenuItem;
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    private void OnEnable()
    {
        game.input.UI.Cancel.performed += ToggleMenu;
        game.OnPlayerDeath += FadeInDeathScreen;
    }

    private void OnDisable()
    {
        game.input.UI.Cancel.performed -= ToggleMenu;
        game.OnPlayerDeath -= FadeInDeathScreen;
    }

    private void Update()
    {
        if (!lastCurrentMenuItem && currentMenuItem)
        {
            CursorClear();
            FetchAndPauseAllAudio();
            game.PauseGame();
        }
        else if (lastCurrentMenuItem && !currentMenuItem)
        {
            CursorAim();
            UnpauseAllAudio();
            game.UnpauseGame();
        }
        
        lastCurrentMenuItem = currentMenuItem;
    }

    public void ExitGame()
    {
        game.ExitGame();
    }
    
    private void ToggleMenu(InputAction.CallbackContext cc = default)
    {
        if (currentMenuItem)
            currentMenuItem.SetActive(false);

        if (!currentMenuItem || !currentMenuItem.transform.Equals(menu))
            menu.gameObject.SetActive(!menu.gameObject.activeInHierarchy);
        
        if (menu.gameObject.activeInHierarchy)
            currentMenuItem = menu.gameObject;
        else
            currentMenuItem = null;
    }

    private void FadeInDeathScreen()
    {
        deathScreen.gameObject.SetActive(true);
        FadeIn(deathScreen, deathScreenFadeDuration, StartDeathScreenCoroutine);

        void StartDeathScreenCoroutine()
        {
            StartCoroutine(DeathScreenCoroutine());
        }
    }

    private IEnumerator DeathScreenCoroutine()
    {
        TextMeshProUGUI t = deathScreenText;

        yield return StartCoroutine(Utils.TextTypingAnimation(t, "...", 1, Utils.TextTypingAnimationType.deleting));
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(Utils.TextTypingAnimation(t, "Your ship exploded", .1f, Utils.TextTypingAnimationType.typing));
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(Utils.TextTypingAnimation(t, "Your ship exploded", .05f, Utils.TextTypingAnimationType.deleting));
        yield return StartCoroutine(Utils.TextTypingAnimation(t, "and you died.", .1f, Utils.TextTypingAnimationType.typing));
        yield return new WaitForSeconds(2);
        BlackFadeIn(5, EndAction);

        void EndAction()
        {
            // reload scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
    }

    public void SetCurrentMenuPanel(GameObject menuItem)
    {
        currentMenuItem = menuItem;
    }
    
    public void ShowObject(GameObject obj, bool show)
    {
        if (canEnableObjects)
            ShowObject(obj.transform, show);
    }
    
    public void ShowObject(Transform obj, bool show)
    {
        if (canEnableObjects)
            obj.gameObject.SetActive(show);
    }

    public void BlackFadeIn(float duration, Action endAction = null, bool destroyOnEnd = true)
    {
        CanvasGroup cg = Instantiate(fadeInPanelPrefab, transform);
        cg.alpha = 0;
        
        FadeIn(cg, duration, endAction, destroyOnEnd);
    }

    public void FadeIn(CanvasGroup obj, float duration, Action endAction = null, bool destroyOnEnd = false)
    {
        StartCoroutine(BlackFadeInCoroutine(obj, duration, endAction, destroyOnEnd));
    }

    private IEnumerator BlackFadeInCoroutine(CanvasGroup panel, float duration, Action endAction = null, bool destroyOnEnd = false)
    {
        float i = 0;
        
        while (i < duration)
        {
            yield return new WaitForEndOfFrame();
            
            i += Time.deltaTime;

            panel.alpha = i / duration;
        }

        if (destroyOnEnd)
            Destroy(panel.gameObject);
        
        endAction?.Invoke();
    }

    public void EnableObjectEnabling()
    {
        canEnableObjects = true;
    }

    private void FetchAndPauseAllAudio()
    {
        fetchedAudioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var audioSource in fetchedAudioSources)
        {
            audioSource.Pause();
        }
    }

    private void UnpauseAllAudio()
    {
        foreach (var audioSource in fetchedAudioSources)
        {
            audioSource.UnPause();
        }
    }

    private void CursorClear()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CursorAim()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
