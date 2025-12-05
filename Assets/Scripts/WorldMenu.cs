using UnityEngine;

public class WorldMenu : MonoBehaviour
{
    [field: SerializeField] public Transform criticalSpeedWarning {get; private set;}
    [field: SerializeField] public Transform repairKeyPopup {get; private set;}
    
    public void ShowObject(GameObject obj, bool show)
    {
        ShowObject(obj.transform, show);
    }
    
    public void ShowObject(Transform obj, bool show)
    {
        obj.gameObject.SetActive(show);
    }
}
