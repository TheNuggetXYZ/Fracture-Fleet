using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceshipTracker : MonoBehaviour
{
    [SerializeField] private List<SpaceshipPartManager> spaceships = new();

    public Action OnListUpdated;

    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    public void ShipSpawned(SpaceshipPartManager ship)
    {
        Debug.Log("SpaceshipTracker: ShipSpawned");
        UpdateShipList();
        OnListUpdated?.Invoke();
    }
    
    private void UpdateShipList()
    {
        spaceships.Clear();
        
        for (int i = 0; i < game.hierarchyManager.folder_enemies.childCount; i++)
        {
            Debug.Log(i);
            if (game.hierarchyManager.folder_enemies.GetChild(i).TryGetComponent(out SpaceshipPartManager spm))
            {
                spaceships.Add(spm);
            }
        }

        for (int i = 0; i < game.hierarchyManager.folder_createdShips.childCount; i++)
        {
            if (game.hierarchyManager.folder_createdShips.GetChild(i).TryGetComponent(out SpaceshipPartManager spm))
            {
                spaceships.Add(spm);
            }
        }
        
        Debug.Log("Updated spaceship list, current count: " + spaceships.Count);
    }

    public List<SpaceshipPartManager> shipList
    {
        get
        {
            return spaceships;
        }
    }
}
