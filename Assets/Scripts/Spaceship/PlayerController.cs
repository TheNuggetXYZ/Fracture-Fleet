using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(Rigidbody))]
public class PlayerController : SpaceshipController
{
    private PlayerInputHandler input;
    
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    [SerializeField] private float warpSpeedBoostMultiplier = 5f;
    [SerializeField] private float warpCancelCollisionMagnitudeThreshold = 10;
    [SerializeField] private float warpChargeRotationSpeedMultiplier = 0.2f;
    
    public float warpSpeedFactor {get; private set;}

    private float currentWarpSpeedMultiplier;
    private bool canWarp;
    private bool warpCharging;

    private new void Awake()
    {
        base.Awake();
        input = GetComponent<PlayerInputHandler>();

        input.onWarp += WarpStart;
    }

    private new void Update()
    {
        base.Update();
        
        warpSpeedFactor = Mathf.InverseLerp(0, MovementSpeed * warpSpeedBoostMultiplier, velocity);
        
        //ManageCriticalSpeedWarning();

        if (!input.isWarping && warpRoutine)
            StopCoroutine(WarpRoutine());
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        float forwardMovementMultiplier = 1;
        if (IsWarping())
            forwardMovementMultiplier = currentWarpSpeedMultiplier;
        else if (input.rollDelta != 0)
            forwardMovementMultiplier = rollSpeedBoostMultiplier;

        // move only forward during warping
        float forwardMovement = IsWarping() ? 1 : input.forwardMovement;
        float verticalMovement = IsWarping() ? 0 : input.verticalMovement;

        Move(MovementSpeed, 
            forwardMovement, 
            verticalMovement, 
            forwardMovementMultiplier);
    }

    private void HandleRotation()
    {
        float rotationMultiplier = warpCharging ? warpChargeRotationSpeedMultiplier : 1;
        
        if (!IsWarping() || warpCharging)
            Rotate(transform.right * input.pitchDelta, 
                transform.up * input.yawDelta, 
                transform.forward * input.rollDelta,
                rotationMultiplier);
    }

    private void WarpStart()
    {
        canWarp = true;
        
        if (!warpRoutine)
            StartCoroutine(WarpRoutine());
    }

    private bool warpRoutine;
    private IEnumerator WarpRoutine()
    {
        warpRoutine = warpCharging = true;
        
        currentWarpSpeedMultiplier = 0;
        yield return new WaitForSeconds(3);
        currentWarpSpeedMultiplier = warpSpeedBoostMultiplier;
        
        warpRoutine = warpCharging = false;
    }

    private bool IsWarping() => input.isWarping && canWarp;

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;

        if (!warpCharging && collisionMagnitude >= warpCancelCollisionMagnitudeThreshold)
            canWarp = false;
    }

    private void ManageCriticalSpeedWarning()
    {
        float somethingCloseToMaxSpeed = MovementSpeed * rollSpeedBoostMultiplier + VerticalMovementSpeed;
        GameManager.I.worldMenu.ShowObject(GameManager.I.worldMenu.criticalSpeedWarning,  
            !input.isWarping && rb.linearVelocity.magnitude > somethingCloseToMaxSpeed - 5);
    }
}
