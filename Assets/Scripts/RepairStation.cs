using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : MonoBehaviour
{
    [SerializeField] private SpaceshipPartManager player;
    [SerializeField] private Transform partSpawnPlace;
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private float partMoveSpeed = 5;
    [SerializeField] private float triggerUIPopupDistance = 10;
    
    private SpaceshipPart[] fetchedKilledParts;
    private bool isRepairing;

    private void Update()
    {
        if (!isRepairing && Input.GetKeyDown(KeyCode.R))
            OnPlayerArrived();
        
        GameManager.I.worldMenu.ShowObject(GameManager.I.worldMenu.repairKeyPopup, Vector3.Distance(GameManager.I.player.transform.position, transform.position) < triggerUIPopupDistance);
    }

    private void OnPlayerArrived()
    {
        fetchedKilledParts = player.GetKilledParts();

        if (fetchedKilledParts.Length > 0)
            StartCoroutine(StartRepairing());
    }

    private IEnumerator StartRepairing()
    {
        isRepairing = true;
        
        foreach (var p in fetchedKilledParts)
        {
            armKinematics.SetGoalPositionSmooth(partSpawnPlace.position, 0.5f);
            yield return new WaitForSeconds(0.5f);
            
            // Spawn part in an immovable state at partSpawnPlace
            p.RemoveRigidbody();
            p.TurnOffCollisions();
            p.SetPosition(partSpawnPlace.position);
            
            // Start moving the part to the ship
            Vector3 partPosition = partSpawnPlace.position;
            float distance = Mathf.Infinity;
            
            // Move loop
            while (distance > 0.05f)
            {
                //Vector3 partTargetPosition = p.OriginalPositionLocalToParent + p.OriginalParent.position;
                
                Vector3 partTargetPosition = p.OriginalParent.TransformPoint(p.OriginalPositionLocalToParent);
                Vector3 partMoveDirection = (partTargetPosition - partPosition).normalized;
                
                distance = Vector3.Distance(partTargetPosition, partPosition);
                
                partPosition += Vector3.ClampMagnitude(partMoveDirection * (partMoveSpeed * Time.deltaTime), distance);
                p.SetPosition(partPosition);
                armKinematics.SetGoalPosition(partPosition);
                
                yield return null;
            } 
            
            p.Repair(true);
        }
        
        player.ClearAllModifiers();
        player.Heal();
        player.RemoveSparks();
        
        isRepairing = false;
    }
}
