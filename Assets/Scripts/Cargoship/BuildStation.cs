using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildStation : MonoBehaviour
{
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private Transform scrapStoringPlace;
    [SerializeField] private Transform shipBuildPlace;
    [SerializeField] private Transform shipRotationReferenceObject;
    [SerializeField] private Vector3 shipRotationOffset;
    [SerializeField] private ScrapStation scrapStation;
    [SerializeField] private ShipModelsSO shipModelsSO;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float enableBuildUIPlayerDistance = 20;
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

    private void OnEnable()
    {
        game.input.Player.BuildStation.performed += OnBuildMenuTriggered;
        game.worldMenu.buildMenuScript.onBuildShip += TryBuildModel;
    }

    private void OnDisable()
    {
        game.input.Player.BuildStation.performed -= OnBuildMenuTriggered;
        game.worldMenu.buildMenuScript.onBuildShip -= TryBuildModel;
    }

    private void OnBuildMenuTriggered(InputAction.CallbackContext cc)
    {
        if (Vector3.Distance(game.player.transform.position, transform.position) <= enableBuildUIPlayerDistance || game.worldMenu.buildMenuVisual.gameObject.activeInHierarchy)
            game.worldMenu.ToggleBuildMenu();
    }
    
    private void Update()
    {
        if (Vector3.Distance(game.player.transform.position, transform.position) <= enableBuildUIPlayerDistance)
        {
            game.worldMenu.ShowObject(game.worldMenu.buildKey, true);
        }
        
        if (tryBuildModel)
        {
            tryBuildModel = false;
            TryBuildModel(modelNumber);
        }
    }

    private void TryBuildModel(int _modelNumber)
    {
        StartCoroutine(TryBuildModelCoroutine(_modelNumber));
    }

    private IEnumerator TryBuildModelCoroutine(int _modelNumber)
    {
        if (!TestBuildability(_modelNumber, out var pairs, out var scrap, out string report))
        {
            game.worldMenu.buildMenuScript.OnFailBuildShip(report);
            yield break;
        }
        
        if (game.worldMenu.buildMenuVisual.gameObject.activeInHierarchy)
            game.worldMenu.ToggleBuildMenu();
        
        scrapStation.UnstoreScrapParts(scrap);

        Transform ship = new GameObject("new ship").transform;
        ship.parent = shipRotationReferenceObject;
        ship.position = shipBuildPlace.position;
        ship.rotation = shipRotationReferenceObject.rotation;
        ship.Rotate(shipRotationOffset);
        
        foreach (var pair in pairs)
        {
            // "reach" for the part
            yield return new WaitForSeconds(armKinematics.SetGoalPositionSmooth(scrapStoringPlace.position,
                moveSpeed));
            
            // prepare the part
            pair.scrapPart.gameObject.SetActive(true);
            pair.scrapPart.transform.parent = ship;
            pair.scrapPart.transform.position = scrapStoringPlace.position;
            pair.scrapPart.transform.rotation = pair.partModel.rotation * ship.rotation;
            //pair.scrapPart.transform.Rotate(ship.rotation.eulerAngles);
            
            // move it to the right place
            Vector3 desiredPartPosition = ship.TransformPoint(pair.partModel.position);
            
            yield return new WaitForSeconds(armKinematics.SetGoalPositionSmooth(desiredPartPosition,
                moveSpeed, (deltaPos, i) => { pair.scrapPart.transform.position += deltaPos;
                    pair.scrapPart.transform.rotation = Quaternion.Euler(pair.partModel.rotation.eulerAngles + ship.rotation.eulerAngles);
                }));
            
            // ensure its exactly in the right position, position has to be recalculated!!
            pair.scrapPart.SetPosition(ship.TransformPoint(pair.partModel.position));
                
            pair.scrapPart.Repair(false, false, false);
        }
        
        // destroy the built ship and instantiate a fresh prefab one
        Destroy(ship.gameObject);
        
        GameObject workingShip = Instantiate(shipModelsSO.shipModels[_modelNumber].shipPrefab, game.hierarchyManager.folder_createdShips);
        workingShip.transform.position = shipBuildPlace.position;
        workingShip.transform.rotation = shipRotationReferenceObject.rotation;
        workingShip.transform.Rotate(shipRotationOffset);
        SpaceshipPartManager workingShipSPM = workingShip.GetComponent<SpaceshipPartManager>();
        workingShipSPM.SetShipToComrade();
    }

    private bool TestBuildability(int _modelNumber, out ScrapPartPartModelPair[] pairs, out List<SpaceshipPart> usedScrap, out string report)
    {
        pairs = null;
        usedScrap = new();
        report = "";
        
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
                report += "Missing part: " + partModel.prefab.name + "\n";
            }

            i++;
        }

        if (canBuild)
            Debug.Log("Can build model number " + _modelNumber);
        else
            Debug.Log("Can't build model number " + _modelNumber + "\n" + report);
            
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
        return scrapPart.prefabInfo.id == partModel.prefabInfoID && scrapPart.transform.lossyScale == partModel.lossyScale;
    }
}