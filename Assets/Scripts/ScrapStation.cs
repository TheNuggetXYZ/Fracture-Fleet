using System;
using System.Collections.Generic;
using UnityEngine;

public class ScrapStation : MonoBehaviour
{
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private Transform scrapStoringPlace;
    [SerializeField] private LayerMask scrapMask;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float reachDistance = 10;
    [SerializeField] private float grabDistance = 0.2f;

    private List<SpaceshipPart> scrapParts = new();
    
    private SpaceshipPart currentScrapPart;

    private void Update()
    {
        if (armKinematics.isBusy)
            return;
        
        scrapParts = GetAllParts();

        if (scrapParts.Count == 0)
            return;
        
        SpaceshipPart part = scrapParts[0];

        armKinematics.MoveGoalPosition(part.transform.position, moveSpeed);

        if (!currentScrapPart && Vector3.Distance(part.transform.position, armKinematics.goalPosition) <= grabDistance)
        {
            currentScrapPart = part;
            currentScrapPart.RemoveRigidbody();
            currentScrapPart.TurnOffCollisions();

            float time = armKinematics.SetGoalPositionSmooth(scrapStoringPlace.position, moveSpeed,
                (deltaPos, i) => { currentScrapPart.transform.position += deltaPos;});
            
            Invoke(nameof(HideCurrentScrapPart), time);
        }
    }

    private void HideCurrentScrapPart()
    {
        currentScrapPart.gameObject.SetActive(false);
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
}
