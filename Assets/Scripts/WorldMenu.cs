using System;
using System.Collections;
using UnityEngine;

public class WorldMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeInPanelPrefab;
    [field: SerializeField] public Transform criticalSpeedWarning {get; private set;}
    [field: SerializeField] public Transform repairKeyPopup {get; private set;}

    private bool canEnableObjects;
    
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
}
