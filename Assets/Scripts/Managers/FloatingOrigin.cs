using System;
using Unity.Cinemachine;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [field: SerializeField] public Transform origin {get; private set;}
    [field: SerializeField, Tooltip("Empty transform")] public Transform originalOrigin {get; private set;}
    [SerializeField] private Transform hangar;

    public Vector3 totalPositionOffset {get; private set;}
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    private void LateUpdate()
    {
        OriginShift();
    }

    private void OriginShift()
    {
        Vector3 positionOffset = -origin.position;
        totalPositionOffset += positionOffset;
        
        originalOrigin.position += positionOffset;
        hangar.position += positionOffset;
        game.hierarchyManager.folder_enemies.position += positionOffset;
        game.hierarchyManager.folder_createdShips.position += positionOffset;
        game.hierarchyManager.folder_scrap.position += positionOffset;
        game.hierarchyManager.folder_solarSystem.position += positionOffset;
        MoveObjects(game.hierarchyManager.GetOPMFolders(), positionOffset); // ERRORS AFTER DYING AND SCENE RELOAD
        
        origin.position = Vector3.zero;
    }

    private void MoveObjects(Transform[] objects, Vector3 move)
    {
        foreach (var obj in objects)
            obj.position += move;
    }
}