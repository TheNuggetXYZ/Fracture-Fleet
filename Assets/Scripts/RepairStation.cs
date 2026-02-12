using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RepairStation : MonoBehaviour
{
    [SerializeField] private CCDKinematics armKinematics;
    [SerializeField] private SpaceshipPartManager player;
    [SerializeField] private Transform partSpawnPlace;
    [SerializeField] private float moveSpeed = 5;
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
        if (Vector3.Distance(game.player.transform.position, transform.position) < triggerUIPopupDistance)
            game.worldMenu.ShowObject(game.worldMenu.interactPopup, true);
    }

    private void OnPlayerArrived(InputAction.CallbackContext cc)
    {
        if (isRepairing)
            return;
        
        fetchedKilledParts = player.GetKilledParts();

        if (fetchedKilledParts.Length > 0 && Vector3.Distance(player.transform.position, transform.position) < repairDistance)
            StartCoroutine(StartRepairing());
    }

    private IEnumerator StartRepairing()
    {
        isRepairing = true;
        
        player.spaceshipController.Lock();
        yield return ArmSmoothMove(player.transform.position, moveSpeed * 2);
        PlayerGrabSFX?.Play();
        PlayerMoveSFX?.Play();
        
        // Move the player and the arm closer and rotate player
        Quaternion playerOGRotation = player.transform.rotation;
        Quaternion playerTargetRotationQ = Quaternion.Euler(playerTargetRotation);
        yield return ArmSmoothMove(game.ToActualPosition(armKinematics.originalGoalPosition), moveSpeed * 0.5f, (Vector3 posDelta, float i) => { player.transform.position += posDelta;
            player.transform.rotation = Quaternion.Lerp(playerOGRotation, playerTargetRotationQ, i); });
        
        foreach (var p in fetchedKilledParts)
        {
            if (!p)
                continue;

            if (!p.gameObject.activeInHierarchy)
                p.gameObject.SetActive(true);
            
            yield return ArmSmoothMove(partSpawnPlace.position, moveSpeed * 2);
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
                
                partPosition += Vector3.ClampMagnitude(partMoveDirection * (moveSpeed * Time.deltaTime), distance);
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

    private WaitForSeconds ArmSmoothMove(Vector3 position, float speed, Action<Vector3, float> action = null)
    {
        return new WaitForSeconds(armKinematics.SetGoalPositionSmooth(position, speed, action));
    }
}
