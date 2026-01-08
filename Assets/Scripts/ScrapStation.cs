using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrapStation : MonoBehaviour
{
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private Transform scrapStoringPlace;
    [SerializeField] private LayerMask scrapMask;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float reachDistance = 10;
    [SerializeField] private float grabDistance = 0.2f;

    private List<SpaceshipPart> foundScrapParts = new();
    public List<SpaceshipPart> storedScrapParts { get; private set; } = new();
    
    private SpaceshipPart currentScrapPart;

    private void Update()
    {
        if (armKinematics.isBusy)
            return;
        
        foundScrapParts = GetAllParts();

        if (foundScrapParts.Count == 0)
            return;
        
        SpaceshipPart part = foundScrapParts[0];

        armKinematics.MoveGoalPosition(part.transform.position, moveSpeed);

        if (part && !currentScrapPart && Vector3.Distance(part.transform.position, armKinematics.goalPosition) <= grabDistance)
        {
            currentScrapPart = part;
            currentScrapPart.RemoveRigidbody();
            currentScrapPart.TurnOffCollisions();

            armKinematics.SetGoalPositionSmooth(scrapStoringPlace.position, moveSpeed,
                (deltaPos, i) => { currentScrapPart.transform.position += deltaPos;}, StoreCurrentScrapPart);
        }
    }

    private void StoreCurrentScrapPart()
    {
        currentScrapPart.gameObject.SetActive(false);
        
        storedScrapParts.Add(currentScrapPart);
        
        Debug.Log("Stored part of type " + currentScrapPart.gameObject);
        
        currentScrapPart = null;
    }

    private List<SpaceshipPart> GetAllParts()
    {
        List<SpaceshipPart> list = new();
        
        Collider[] cols = Physics.OverlapSphere(transform.position, reachDistance, scrapMask);

        foreach (var col in cols)
        {
            if (col.attachedRigidbody && col.attachedRigidbody.TryGetComponent(out SpaceshipPart part))
                list.Add(part);
        }
        
        return list;
    }

    public void UnstoreScrapParts(List<SpaceshipPart> scrapPartsToUnstore)
    {
        foreach (SpaceshipPart part in storedScrapParts.ToList())
        {
            foreach (SpaceshipPart unstorePart in scrapPartsToUnstore)
            {
                if (part == unstorePart)
                {
                    storedScrapParts.Remove(unstorePart);
                }
            }
        }
    }
}
