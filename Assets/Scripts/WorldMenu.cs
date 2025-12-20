using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeInPanelPrefab;
    [field: SerializeField] public Transform criticalSpeedWarning {get; private set;}
    [field: SerializeField] public Transform repairKeyPopup {get; private set;}
    [field: SerializeField] public Transform menu {get; private set;}

    private GameObject currentMenuItem;
    private bool canEnableObjects;
    private AudioSource[] fetchedAudioSources;
    private bool lastCurrentMenuItem;

    private void Awake()
    {
        GameManager.I.input.UI.Cancel.performed += ToggleMenu;
    }

    private void Update()
    {
        if (!lastCurrentMenuItem && currentMenuItem)
        {
            CursorClear();
            FetchAndPauseAllAudio();
            GameManager.I.PauseGame();
        }
        else if (lastCurrentMenuItem && !currentMenuItem)
        {
            CursorAim();
            UnpauseAllAudio();
            GameManager.I.UnpauseGame();
        }
        
        lastCurrentMenuItem = currentMenuItem;
    }

    public void ExitGame()
    {
        GameManager.I.ExitGame();
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

    public void BlackFadeIn(float duration, Action endAction = null)
    {
        CanvasGroup cg = Instantiate(fadeInPanelPrefab, transform);
        cg.alpha = 0;
        
        StartCoroutine(BlackFadeInCoroutine(cg, duration, endAction));
    }

    private IEnumerator BlackFadeInCoroutine(CanvasGroup panel, float duration, Action endAction = null)
    {
        float i = 0;
        
        while (i < duration)
        {
            yield return new WaitForEndOfFrame();
            
            i += Time.deltaTime;

            panel.alpha = i / duration;
        }
        
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
