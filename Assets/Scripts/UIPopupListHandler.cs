using System.Collections;
using UnityEngine;

public class UIPopupListHandler : MonoBehaviour
{
    [field: SerializeField] public Transform popup_ChargingWarp {get; private set;}
    [field: SerializeField] public Transform popup_Warping {get; private set;}
    [field: SerializeField] public Transform popup_IntegrityRestored {get; private set;}
    [field: SerializeField] public Transform popup_ShipSustainedDamage {get; private set;}
    [field: SerializeField] public Transform popup_EnemyNeutralized {get; private set;}
    
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

        if (originalState != show)
            obj.SetAsLastSibling();
    }
    
    private IEnumerator ObjectSetActiveDelayed(Transform obj, bool active, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        obj.gameObject.SetActive(active);
    }
}
