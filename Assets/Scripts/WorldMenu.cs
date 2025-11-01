using UnityEngine;

public class WorldMenu : MonoBehaviour
{
    [SerializeField] private Transform criticalSpeedWarning;

    public void ShowCriticalSpeedWarning(bool show)
    {
        criticalSpeedWarning.gameObject.SetActive(show);
    }
}
