using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class BuildStation : MonoBehaviour
{
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private Transform scrapStoringPlace;
    [SerializeField] private Transform shipBuildPlace;
    [SerializeField] private ScrapStation scrapStation;
    [SerializeField] private ShipModelsSO shipModelsSO;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private bool tryBuildModel;
    [SerializeField] private int modelNumber;

    GameManager game;
    
    private struct ScrapPartPartModelPair
    {
        public SpaceshipPart scrapPart;
        public ShipModelsSO.ShipPartModel partModel;
    }

    private void Awake()
    {
        game = GameManager.I;
    }

    private void Update()
    {
        if (tryBuildModel)
        {
            tryBuildModel = false;
            StartCoroutine(TryBuildModel(modelNumber));
        }
    }

    private IEnumerator TryBuildModel(int _modelNumber)
    {
        if (!TestBuildability(_modelNumber, out var pairs, out var scrap))
            yield break;
        
        scrapStation.UnstoreScrapParts(scrap);

        Transform ship = new GameObject("new ship").transform;
        ship.parent = game.hierarchyManager.folder_createdShips;
        
        foreach (var pair in pairs)
        {
            yield return new WaitForSeconds(armKinematics.SetGoalPositionSmooth(scrapStoringPlace.position,
                moveSpeed));
            
            pair.scrapPart.gameObject.SetActive(true);
            pair.scrapPart.transform.parent = ship;
            pair.scrapPart.transform.position = scrapStoringPlace.position;
            pair.scrapPart.transform.rotation = pair.partModel.rotation;
            
            yield return new WaitForSeconds(armKinematics.SetGoalPositionSmooth(shipBuildPlace.position + pair.partModel.position,
                moveSpeed, (deltaPos, i) => { pair.scrapPart.transform.position += deltaPos; }));
            
            pair.scrapPart.SetPosition(shipBuildPlace.position + pair.partModel.position);
                
            pair.scrapPart.Repair(false, false);
        }
    }

    private bool TestBuildability(int _modelNumber, out ScrapPartPartModelPair[] pairs, out List<SpaceshipPart> usedScrap)
    {
        pairs = null;
        usedScrap = new();
        
        if (shipModelsSO.shipModels.Length <= _modelNumber || _modelNumber < 0)
            return false;
        
        tryBuildModel = false;
        ShipModelsSO.ShipModel model = shipModelsSO.shipModels[_modelNumber];
        ShipModelsSO.ShipPartModel[] partModels = model.PartModels;
        List<SpaceshipPart> scrapParts = scrapStation.storedScrapParts;
        pairs =  new ScrapPartPartModelPair[partModels.Length];

        int i = 0;
        bool canBuild = true;
        foreach (var partModel in partModels)
        {
            bool found = false;
            foreach (var scrapPart in scrapParts)
            {
                if (WasntUsedYet(scrapPart, usedScrap) && IsSamePart(scrapPart, partModel))
                {
                    usedScrap.Add(scrapPart);
                    pairs[i] = new ScrapPartPartModelPair {scrapPart = scrapPart, partModel = partModel};
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                canBuild = false;
                Debug.Log("Missing part: " + partModel.prefab.name);
                break;
            }

            i++;
        }

        if (canBuild)
            Debug.Log("Can build model number " + _modelNumber);
        else
            Debug.Log("Can't build model number " + _modelNumber);
            
        return canBuild;
    }

    private bool WasntUsedYet(SpaceshipPart scrapPart, List<SpaceshipPart> usedParts)
    {
        foreach (var usedPart in usedParts)
        {
            if (scrapPart == usedPart)
                return false;
        }
        
        return true;
    }

    private bool IsSamePart(SpaceshipPart scrapPart, ShipModelsSO.ShipPartModel partModel)
    {
        return scrapPart.prefabInfo.id == partModel.prefabInfo.id && scrapPart.transform.lossyScale == partModel.lossyScale;
    }
}