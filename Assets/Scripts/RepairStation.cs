using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : MonoBehaviour
{
    [SerializeField] private SpaceshipPartManager player;
    [SerializeField] private Transform partSpawnPlace;
    [SerializeField] private Transform repairStationCenter;
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private float partMoveSpeed = 5;
    [SerializeField] private float triggerUIPopupDistance = 10;
    [SerializeField] private float repairDistance = 10;
    [SerializeField] private Vector3 playerTargetRotation;
    
    private SpaceshipPart[] fetchedKilledParts;
    private bool isRepairing;

    private void Update()
    {
        if (!isRepairing && Input.GetKeyDown(KeyCode.R))
            OnPlayerArrived();
        
        GameManager.I.worldMenu.ShowObject(GameManager.I.worldMenu.repairKeyPopup, Vector3.Distance(GameManager.I.player.transform.position, repairStationCenter.position) < triggerUIPopupDistance);
    }

    private void OnPlayerArrived()
    {
        fetchedKilledParts = player.GetKilledParts();

        if (fetchedKilledParts.Length > 0 && Vector3.Distance(player.transform.position, repairStationCenter.position) < repairDistance)
            StartCoroutine(StartRepairing());
    }

    private IEnumerator StartRepairing()
    {
        isRepairing = true;
        
        player.spaceshipController.Lock();
        yield return ArmSmoothMove(player.transform.position, 0.5f);
        Quaternion playerOGRotation = player.transform.rotation;
        Quaternion playerTargetRotationQ = Quaternion.Euler(playerTargetRotation);
        yield return ArmSmoothMove(armKinematics.originalGoalPosition, 2f, (Vector3 pos, float i) => { player.transform.position = pos;
            player.transform.rotation = Quaternion.Lerp(playerOGRotation, playerTargetRotationQ, i); return 0;});
        
        foreach (var p in fetchedKilledParts)
        {
            float goToSpawnPlaceTime = 0.5f;
            armKinematics.SetGoalPositionSmooth(partSpawnPlace.position, goToSpawnPlaceTime);
            yield return new WaitForSeconds(goToSpawnPlaceTime);
            
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
        player.spaceshipController.Unlock();
        
        isRepairing = false;
    }

    private WaitForSeconds ArmSmoothMove(Vector3 position, float time, Func<Vector3, float, int> action = null)
    {
        armKinematics.SetGoalPositionSmooth(position, time, action);
        return new WaitForSeconds(time);
    }
}
