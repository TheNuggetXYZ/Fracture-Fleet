using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private AudioSource PlayerGrabSFX;
    [SerializeField] private AudioSource PlayerMoveSFX;
    [SerializeField] private AudioSource PartGrabSFX;
    [SerializeField] private AudioSource PartPutSFX;
    
    private SpaceshipPart[] fetchedKilledParts;
    private bool isRepairing;
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    private void OnEnable()
    {
        game.input.Player.RepairStation.performed += OnPlayerArrived;
    }
    
    private void OnDisable()
    {
        game.input.Player.RepairStation.performed -= OnPlayerArrived;
    }
    
    private void Update()
    {
        game.worldMenu.ShowObject(game.worldMenu.repairKeyPopup, Vector3.Distance(game.player.transform.position, repairStationCenter.position) < triggerUIPopupDistance);
    }

    private void OnPlayerArrived(InputAction.CallbackContext cc)
    {
        if (isRepairing)
            return;
        
        fetchedKilledParts = player.GetKilledParts();

        if (fetchedKilledParts.Length > 0 && Vector3.Distance(player.transform.position, repairStationCenter.position) < repairDistance)
            StartCoroutine(StartRepairing());
    }

    private IEnumerator StartRepairing()
    {
        isRepairing = true;
        
        player.spaceshipController.Lock();
        yield return ArmSmoothMove(player.transform.position, 0.5f);
        PlayerGrabSFX?.Play();
        PlayerMoveSFX?.Play();
        
        // Move the player and the arm closer and rotate player
        Quaternion playerOGRotation = player.transform.rotation;
        Quaternion playerTargetRotationQ = Quaternion.Euler(playerTargetRotation);
        yield return ArmSmoothMove(armKinematics.originalGoalPosition, 2f, (Vector3 pos, float i) => { player.transform.position = pos;
            player.transform.rotation = Quaternion.Lerp(playerOGRotation, playerTargetRotationQ, i); return 0;});
        
        foreach (var p in fetchedKilledParts)
        {
            yield return ArmSmoothMove(partSpawnPlace.position, 0.5f);
            PartGrabSFX?.Play();
            
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
                
                PartPutSFX?.Play();
                
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
