using UnityEngine;

public class HierarchyManager : MonoBehaviour
{
    [field: SerializeField] public Transform folder_scrap {get; private set;}
    [field: SerializeField] public Transform folder_enemies {get; private set;}
    [field: SerializeField] public Transform folder_createdShips {get; private set;}
    [field: SerializeField] public Transform folder_solarSystem {get; private set;}

    public Transform[] GetOPMFolders()
    {
        return ObjectPoolManager.GetPoolParents();
    }
}
