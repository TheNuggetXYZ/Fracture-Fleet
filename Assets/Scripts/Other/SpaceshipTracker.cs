using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceshipTracker : MonoBehaviour
{
    private List<SpaceshipPartManager> spaceships;

    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }
    
    private void OnEnable()
    {
        game.hierarchyManager.OnEnemiesChanged += UpdateShipList;
        game.hierarchyManager.OnCreatedShipsChanged += UpdateShipList;
    }

    private void OnDisable()
    {
        game.hierarchyManager.OnEnemiesChanged -= UpdateShipList;
        game.hierarchyManager.OnCreatedShipsChanged -= UpdateShipList;
    }

    private void UpdateShipList()
    {
        spaceships = new List<SpaceshipPartManager>();
        
        for (int i = 0; i < game.hierarchyManager.folder_enemies.childCount; i++)
        {
            spaceships.Add(game.hierarchyManager.folder_enemies.GetChild(i).GetComponent<SpaceshipPartManager>());
        }

        for (int i = 0; i < game.hierarchyManager.folder_createdShips.childCount; i++)
        {
            spaceships.Add(game.hierarchyManager.folder_createdShips.GetChild(i).GetComponent<SpaceshipPartManager>());
        }
    }

    public List<SpaceshipPartManager> shipList
    {
        get
        {
            return spaceships;
        }
    }
}
