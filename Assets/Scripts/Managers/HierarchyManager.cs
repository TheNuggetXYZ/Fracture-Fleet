using System;
using UnityEngine;

public class HierarchyManager : MonoBehaviour
{
    [field: SerializeField] public Transform folder_scrap {get; private set;}
    [field: SerializeField] public Transform folder_enemies {get; private set;}
    [field: SerializeField] public Transform folder_createdShips {get; private set;}
    [field: SerializeField] public Transform folder_solarSystem {get; private set;}

    public Action OnEnemiesChanged;
    public Action OnCreatedShipsChanged;

    private int oldEnemiesChildCount = -1;
    private int oldCreatedShipsChildCount = -1;

    private void Update()
    {
        if (folder_enemies.childCount != oldEnemiesChildCount)
        {
            oldEnemiesChildCount = folder_enemies.childCount;
            OnEnemiesChanged?.Invoke();
        }

        if (folder_createdShips.childCount != oldCreatedShipsChildCount)
        {
            oldCreatedShipsChildCount = folder_createdShips.childCount;
            OnCreatedShipsChanged?.Invoke();
        }
    }

    public Transform[] GetOPMFolders()
    {
        return ObjectPoolManager.GetPoolParents();
    }
}
