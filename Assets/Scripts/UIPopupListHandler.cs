using System.Collections;
using UnityEngine;

public class UIPopupListHandler : MonoBehaviour
{
    [field: SerializeField] public Transform popup_ChargingWarp {get; private set;}
    [field: SerializeField] public Transform popup_Warping {get; private set;}
    [field: SerializeField] public Transform popup_IntegrityRestored {get; private set;}
    [field: SerializeField] public Transform popup_ShipSustainedDamage {get; private set;}
    [field: SerializeField] public Transform popup_EnemyNeutralized {get; private set;}
    [field: SerializeField] public Transform popup_DetectingSignals {get; private set;}
    
    [field: SerializeField] public Transform warning_ShipModuleLost {get; private set;}
    [field: SerializeField] public Transform warning_ImpactDamage {get; private set;}
    [field: SerializeField] public Transform warning_DestructionImminent {get; private set;}
    [field: SerializeField] public Transform warning_HighGravity {get; private set;}
    
    public void ShowPopup(Transform obj, bool show, float delay = 0, float duration = -1)
    {
        bool originalState = obj.gameObject.activeInHierarchy;
        
        if (delay == 0)
        {
            obj.gameObject.SetActive(show);
        }
        else
            StartCoroutine(ObjectSetActiveDelayed(obj, show, delay));

        if (duration != -1)
        {
            StartCoroutine(ObjectSetActiveDelayed(obj, !show, delay + duration));
        }

        if (originalState != obj.gameObject.activeInHierarchy) // state changed
        {
            obj.SetAsLastSibling();

            SpawnSFX(show);
        }
    }
    
    private IEnumerator ObjectSetActiveDelayed(Transform obj, bool show, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        
        bool originalState = obj.gameObject.activeInHierarchy;
        obj.gameObject.SetActive(show);
        
        if (originalState != obj.gameObject.activeInHierarchy)
            SpawnSFX(show);
    }

    private void SpawnSFX(bool UIToggledOn)
    {
        AudioObject audioPrefab;
        if (UIToggledOn)
            audioPrefab = GameManager.I.prefabs.UIOnSFX;
        else
            audioPrefab = GameManager.I.prefabs.UIOffSFX;
            
        ObjectPoolManager.SpawnObject(audioPrefab);
    }
}
