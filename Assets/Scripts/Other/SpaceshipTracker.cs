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
        Debug.Log("Ship spawned");
        UpdateShipList();
        OnListUpdated?.Invoke();
    }
    
    // destroy as in 'Destroy()' not kill
    public void ShipDestroyed()
    {
        Debug.Log("Ship destroyed");
        UpdateShipList();
        OnListUpdated?.Invoke();
    }
    
    private void UpdateShipList()
    {
        spaceships.Clear();
        Debug.Log(game.hierarchyManager.folder_enemies.childCount);
        for (int i = 0; i < game.hierarchyManager.folder_enemies.childCount; i++)
        {
            Debug.Log(i);
            if (game.hierarchyManager.folder_enemies.GetChild(i).TryGetComponent(out SpaceshipPartManager spm))
            {
                spaceships.Add(spm);
                Debug.Log("Added to spaceship list, current count: " + spaceships.Count);
            }
        }

        for (int i = 0; i < game.hierarchyManager.folder_createdShips.childCount; i++)
        {
            if (game.hierarchyManager.folder_createdShips.GetChild(i).TryGetComponent(out SpaceshipPartManager spm))
            {
                spaceships.Add(spm);
                Debug.Log("Added to spaceship list, current count: " + spaceships.Count);
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
