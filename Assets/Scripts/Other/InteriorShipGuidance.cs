using UnityEngine;

public class InteriorShipGuidance : MonoBehaviour
{
    [SerializeField, Tooltip("Must be BEHIND a door if there is one outside of the interior")] private GameObject exit;

    public GameObject interiorExit => exit;
}
